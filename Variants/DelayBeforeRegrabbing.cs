using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class DelayBeforeRegrabbing : AbstractExtendedVariant {
        public DelayBeforeRegrabbing() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Holdable.Release += modHoldableRelease;
        }

        public override void Unload() {
            IL.Celeste.Holdable.Release -= modHoldableRelease;
        }
        private void modHoldableRelease(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Holdable>("cannotHoldDelay"))) {
                Logger.Log("ExtendedVariantMode/DelayBeforeRegrabbing", $"Modding delay before regrabbing at {cursor.Index} in IL for Holdable.Release");
                cursor.EmitDelegate<Func<float, float>>(orig => orig * GetVariantValue<float>(Variant.DelayBeforeRegrabbing));
            }
        }
    }
}
