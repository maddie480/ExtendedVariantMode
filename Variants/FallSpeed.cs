using Celeste;
using Celeste.Mod;
using Monocle;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using MonoMod.Utils;

namespace ExtendedVariants.Variants {
    public class FallSpeed : AbstractExtendedVariant {

        private ILHook hookUpdateSprite;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object GetVariantValue() {
            return Settings.FallSpeed;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.FallSpeed = (float) value;
            OnVariantChanged();
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.FallSpeed = (value / 10f);
            OnVariantChanged();
        }

        public void OnVariantChanged() {
            Player player = Engine.Scene?.Tracker.GetEntity<Player>();

            if (player != null) {
                // forcefully drag back maxFall to a sensical value if going from 100x fall speed to 1x for example.
                DynData<Player> playerData = new DynData<Player>(player);
                playerData["maxFall"] = Math.Min(playerData.Get<float>("maxFall"), 240f * Settings.FallSpeed);
            }
        }

        public override void Load() {
            bool isUpdateSpritePatched = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "Everest", Version = new Version(1, 1432) });

            IL.Celeste.Player.NormalBegin += modNormalBegin;
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
            hookUpdateSprite = new ILHook(typeof(Player).GetMethod(isUpdateSpritePatched ? "orig_UpdateSprite" : "UpdateSprite", BindingFlags.NonPublic | BindingFlags.Instance), modUpdateSprite);
        }

        public override void Unload() {
            IL.Celeste.Player.NormalBegin -= modNormalBegin;
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
            hookUpdateSprite?.Dispose();
        }

        /// <summary>
        /// Edits the NormalBegin method in Player, so that ma fall speed is applied right when entering the "normal" state.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modNormalBegin(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go wherever the maxFall variable is initialized to 160 (... I mean, that's a one-line method, but maxFall is private so...)
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float) instr.Operand == 160f)) {
                Logger.Log("ExtendedVariantMode/FallSpeed", $"Applying max fall speed factor to constant at {cursor.Index} in CIL code for NormalBegin");

                // add two instructions to multiply those constants with the "fall speed factor"
                cursor.EmitDelegate<Func<float>>(determineFallSpeedFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we will edit 2 constants here:
            // * 160 = max falling speed
            // * 240 = max falling speed when holding Down

            // find out where those constants are loaded into the stack
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && ((float) instr.Operand == 160f || (float) instr.Operand == 240f))) {
                Logger.Log("ExtendedVariantMode/FallSpeed", $"Applying max fall speed factor to constant at {cursor.Index} in CIL code for NormalUpdate");

                // add two instructions to multiply those constants with the "fall speed factor"
                cursor.EmitDelegate<Func<float>>(determineFallSpeedFactor);
                cursor.Emit(OpCodes.Mul);
            }

            cursor.Index = 0;

            // go back to the first 240f, then to the next "if" implying MoveY
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(240f))
                && cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Input), "MoveY"))
                && cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Bne_Un || instr.OpCode == OpCodes.Brfalse)) {
                Logger.Log("ExtendedVariantMode/FallSpeed", $"Injecting code to fix animation with 0 fall speed at {cursor.Index} in CIL code for NormalUpdate");

                // save the target of this branch
                object label = cursor.Prev.Operand;

                // the goal here is to add another condition to the if: FallSpeedFactor should not be zero
                // so that the game does not try computing the animation (doing a nice division by 0 by the way)
                cursor.EmitDelegate<Func<float>>(determineFallSpeedFactor);
                cursor.Emit(OpCodes.Ldc_R4, 0f);
                cursor.Emit(OpCodes.Beq, label); // we jump (= skip the "if") if DetermineFallSpeedFactor is equal to 0.
            }
        }

        /// <summary>
        /// Edits the UpdateSprite method in Player (updating the player animation.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modUpdateSprite(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // the goal is to multiply 160 (max falling speed) with the fall speed factor to fix the falling animation
            // let's search for all 160 occurrences in the IL code
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(160f))) {
                Logger.Log("ExtendedVariantMode/FallSpeed", $"Applying fall speed and gravity to constant at {cursor.Index} in CIL code for UpdateSprite to fix animation");

                // add two instructions to multiply those constants with a mix between fall speed and gravity
                cursor.EmitDelegate<Func<float>>(mixFallSpeedAndGravity);
                cursor.Emit(OpCodes.Mul);
                // also remove 0.1 to prevent an animation glitch caused by rounding (I guess?) on very low fall speeds
                cursor.Emit(OpCodes.Ldc_R4, 0.1f);
                cursor.Emit(OpCodes.Sub);
            }
        }

        /// <summary>
        /// Returns the currently configured fall speed factor.
        /// </summary>
        /// <returns>The fall speed factor (1 = default fall speed)</returns>
        private float determineFallSpeedFactor() {
            return Settings.FallSpeed;
        }

        private float mixFallSpeedAndGravity() {
            return Math.Min(Settings.FallSpeed, Settings.Gravity);
        }
    }
}
