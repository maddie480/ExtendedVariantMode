using System;

namespace ExtendedVariants.Variants {
    public class DelayBetweenBadelines : AbstractExtendedVariant {
        public DelayBetweenBadelines() : base(variantType: typeof(float), defaultVariantValue: 0.4f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }
    }
}
