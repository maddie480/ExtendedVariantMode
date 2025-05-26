using System;

namespace ExtendedVariants.Variants {
    public class BadelineLag : AbstractExtendedVariant {
        public BadelineLag() : base(variantType: typeof(float), defaultVariantValue: 1.55f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value == 0 ? 1.55f : value / 10f;
        }
    }
}
