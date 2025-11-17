using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class WallbouncingSpeed : AbstractExtendedVariant {

        public WallbouncingSpeed() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Player.SuperWallJump += modSuperWallJump;
        }

        public override void Unload() {
            IL.Celeste.Player.SuperWallJump -= modSuperWallJump;
        }

        /// <summary>
        /// Edits the SuperWallJump method in Player (called when super/hyperdashing on a wall.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private static void modSuperWallJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply -160f (height given by a superdash) with the jump height factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-160f))) {
                Logger.Log("ExtendedVariantMode/WallbouncingSpeed", $"Applying wallbouncing speed to constant at {cursor.Index} in CIL code for SuperWallJump");
                cursor.EmitDelegate<Func<float>>(determineWallBouncingSpeedFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Returns the current wallbounce speed factor.
        /// </summary>
        /// <returns>The wallbounce speed factor (1 = default wallbounce speed)</returns>
        private static float determineWallBouncingSpeedFactor() {
            return GetVariantValue<float>(Variant.WallBouncingSpeed);
        }
    }
}
