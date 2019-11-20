using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class HyperdashSpeed : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.HyperdashSpeed;
        }

        public override void SetValue(int value) {
            Settings.HyperdashSpeed = value;
        }

        public override void Load() {
            IL.Celeste.Player.SuperJump += modSuperJump;
        }

        public override void Unload() {
            IL.Celeste.Player.SuperJump -= modSuperJump;
        }

        /// <summary>
        /// Edits the SuperJump method in Player (called when super/hyperdashing.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modSuperJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply 260f (speed given by a superdash) with the hyperdash speed factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(260f))) {
                Logger.Log("ExtendedVariantMode/HyperdashSpeed", $"Applying hyperdash speed to constant at {cursor.Index} in CIL code for SuperJump");
                cursor.EmitDelegate<Func<float>>(determineHyperdashSpeedFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Returns the current hyperdash speed factor.
        /// </summary>
        /// <returns>The hyperdash speed factor (1 = default hyperdash speed)</returns>
        private float determineHyperdashSpeedFactor() {
            return Settings.HyperdashSpeed / 10f;
        }
    }
}
