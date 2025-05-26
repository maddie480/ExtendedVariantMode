using System;

namespace ExtendedVariants.Variants {
    public class DashTimerMultiplier : AbstractExtendedVariant {
        public DashTimerMultiplier() : base(variantType: typeof(float), defaultVariantValue: 1.0f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }
    }
}
