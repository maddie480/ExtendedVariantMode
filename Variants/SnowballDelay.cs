using System;

namespace ExtendedVariants.Variants {
    public class SnowballDelay : AbstractExtendedVariant {
        public SnowballDelay() : base(variantType: typeof(float), defaultVariantValue: 0.8f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }
    }
}
