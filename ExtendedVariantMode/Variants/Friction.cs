using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class Friction : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.Friction;
        }

        public override void SetValue(int value) {
            Settings.Friction = value;
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
            IL.Celeste.Player.UpdateSprite += modUpdateSprite;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
            IL.Celeste.Player.UpdateSprite -= modUpdateSprite;
        }

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.) to apply ground friction.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to the 500 in "this.Speed.X = Calc.Approach(this.Speed.X, 0f, 500f * Engine.DeltaTime);"
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(500f))) {
                Logger.Log("ExtendedVariantMode/Friction", $"Applying friction to constant at {cursor.Index} (ducking stop speed on ground) in CIL code for NormalUpdate");

                cursor.EmitDelegate<Func<float>>(determineFrictionFactor);
                cursor.Emit(OpCodes.Mul);
            }

            // jump to "float num = this.onGround ? 1f : 0.65f;" by jumping to 0.65 then 1 (the numbers are swapped in the IL code)
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.65f))
                && cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1f))) {

                Logger.Log("ExtendedVariantMode/Friction", $"Applying friction to constant at {cursor.Index} (friction factor on ground) in CIL code for NormalUpdate");

                // 1 is the acceleration when on the ground. Apply the friction factor to it.
                cursor.EmitDelegate<Func<float>>(determineFrictionFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Edits the UpdateSprite method in Player (updating the player animation) to fix the animations when using modded friction.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modUpdateSprite(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're jumping to this line: "if (Math.Abs(this.Speed.X) <= 25f && this.moveX == 0)"
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float) instr.Operand == 25f)) {
                Logger.Log("ExtendedVariantMode/Friction", $"Modding constant at {cursor.Index} in CIL code for UpdateSprite to fix animation with friction");

                // call our method which will essentially replace the 25 with whatever value we want
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate<Func<float>>(getIdleAnimationThreshold);
            }
        }

        /// <summary>
        /// Compute the idle animation threshold (when the player lets go every button, Madeline will use the walking animation until
        /// her X speed gets below this value. Under this value, she will use her idle animation.)
        /// </summary>
        /// <returns>The idle animation threshold (minimum 25, gets higher as the friction factor is lower)</returns>
        private float getIdleAnimationThreshold() {
            if (Settings.Friction >= 10) {
                // keep the default value
                return 25f;
            }

            // shift the "stand still" threshold towards max walking speed, which is 90f
            // for example, it will give 83.5 when friction factor is 0.1, Madeline will appear to slip standing still.
            return 25f + (90f * Settings.SpeedX / 10f - 25f) * (1 - determineFrictionFactor());
        }

        /// <summary>
        /// Returns the currently configured friction factor.
        /// </summary>
        /// <returns>The friction factor (1 = default friction)</returns>
        private float determineFrictionFactor() {
            switch (Settings.Friction) {
                case -1: return 0f;
                case 0: return 0.05f;
                default: return Settings.Friction / 10f;
            }
        }
    }
}
