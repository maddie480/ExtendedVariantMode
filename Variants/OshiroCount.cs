using System;

namespace ExtendedVariants.Variants {
    public class OshiroCount : AbstractExtendedVariant {
        public OshiroCount() : base(variantType: typeof(int), defaultVariantValue: 1) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }
    }
}
