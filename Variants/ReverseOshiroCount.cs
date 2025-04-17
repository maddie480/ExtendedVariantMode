using System;

namespace ExtendedVariants.Variants {
    public class ReverseOshiroCount : AbstractExtendedVariant {
        public ReverseOshiroCount() : base(variantType: typeof(int), defaultVariantValue: 0) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }
    }
}
