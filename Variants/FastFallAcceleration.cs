using Celeste.Mod;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class FastFallAcceleration : AbstractExtendedVariant {
        public FastFallAcceleration() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= modPlayerNormalUpdate;
        }

        private void modPlayerNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(300f))) {
                Logger.Log("ExtendedVariantMode/FastFallAcceleration", $"Modding fast fall acceleration at {cursor.Index} in IL for Player.NormalUpdate");
                cursor.EmitDelegate<Func<float, float>>(orig => orig * GetVariantValue<float>(Variant.FastFallAcceleration));
            }
        }
    }
}
