using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class FallSpeed : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.FallSpeed;
        }

        public override void SetValue(int value) {
            Settings.FallSpeed = value;
        }

        public override void Load() {
            IL.Celeste.Player.NormalBegin += modNormalBegin;
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
            IL.Celeste.Player.UpdateSprite += modUpdateSprite;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalBegin -= modNormalBegin;
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
            IL.Celeste.Player.UpdateSprite -= modUpdateSprite;
        }
        
        /// <summary>
        /// Edits the NormalBegin method in Player, so that ma fall speed is applied right when entering the "normal" state.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modNormalBegin(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go wherever the maxFall variable is initialized to 160 (... I mean, that's a one-line method, but maxFall is private so...)
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 160f)) {
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
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && ((float)instr.Operand == 160f || (float)instr.Operand == 240f))) {
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
            return Settings.FallSpeed / 10f;
        }

        private float mixFallSpeedAndGravity() {
            return Math.Min(Settings.FallSpeed / 10f, Settings.Gravity / 10f);
        }
    }
}
