using System;

namespace ExtendedVariants.Variants {
    public class BadelineBossNodeCount : AbstractExtendedVariant {
        public BadelineBossNodeCount() : base(variantType: typeof(int), defaultVariantValue: 1) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }
    }
}
