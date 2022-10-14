using Celeste.Mod;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class CornerCorrection : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetVariantValue() {
            return Settings.CornerCorrection;
        }

        public override object GetDefaultVariantValue() {
            return 4;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.CornerCorrection = (int) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.CornerCorrection = value;
        }

        public override void Load() {
            IL.Celeste.Player.OnCollideH += hookCollideCornerCorrection;
            IL.Celeste.Player.OnCollideV += hookCollideCornerCorrection;
        }

        public override void Unload() {
            IL.Celeste.Player.OnCollideH -= hookCollideCornerCorrection;
            IL.Celeste.Player.OnCollideV -= hookCollideCornerCorrection;
        }

        private void hookCollideCornerCorrection(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdloc(1) || instr.MatchLdloc(2) || instr.MatchLdloc(7) || instr.MatchLdloc(8),
                instr => instr.MatchLdcI4(4) || instr.MatchLdcI4(-4) || instr.MatchLdloc(6))) {

                Logger.Log("ExtendedVariantMode/CornerCorrection", $"Editing corner correction pixels at {cursor.Index} in IL for {il.Method.FullName}");

                cursor.EmitDelegate<Func<int, int>>(orig => {
                    // vanilla corner correction already is 4 pixels, but we still pass orig through in case another mod mods it.
                    if (Settings.CornerCorrection == 4) return orig;

                    return Settings.CornerCorrection * Math.Sign(orig);
                });
            }
        }
    }
}
