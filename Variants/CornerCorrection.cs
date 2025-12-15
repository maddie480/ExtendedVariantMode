using Celeste.Mod;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class CornerCorrection : AbstractExtendedVariant {
        public CornerCorrection() : base(variantType: typeof(int), defaultVariantValue: 4) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }

        public override void Load() {
            IL.Celeste.Player.OnCollideH += hookCollideCornerCorrection;
            IL.Celeste.Player.OnCollideV += hookCollideCornerCorrection;
        }

        public override void Unload() {
            IL.Celeste.Player.OnCollideH -= hookCollideCornerCorrection;
            IL.Celeste.Player.OnCollideV -= hookCollideCornerCorrection;
        }

        private static void hookCollideCornerCorrection(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdloc(1) || instr.MatchLdloc(2) || instr.MatchLdloc(7) || instr.MatchLdloc(8),
                instr => instr.MatchLdcI4(4) || instr.MatchLdcI4(-4) || instr.MatchLdloc(6))) {

                Logger.Log("ExtendedVariantMode/CornerCorrection", $"Editing corner correction pixels at {cursor.Index} in IL for {il.Method.FullName}");

                cursor.EmitDelegate<Func<int, int>>(modifyCornerCorrectionPixels);
            }
        }

        private static int modifyCornerCorrectionPixels(int orig) {
            // vanilla corner correction already is 4 pixels, but we still pass orig through in case another mod mods it.
            if (GetVariantValue<int>(Variant.CornerCorrection) == 4) return orig;

            return GetVariantValue<int>(Variant.CornerCorrection) * Math.Sign(orig);
        }
    }

}
