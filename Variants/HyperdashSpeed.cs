using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class HyperdashSpeed : AbstractExtendedVariant {

        public HyperdashSpeed() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
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
        private static void modSuperJump(ILContext il) {
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
        private static float determineHyperdashSpeedFactor() {
            return GetVariantValue<float>(Variant.HyperdashSpeed);
        }
    }
}
