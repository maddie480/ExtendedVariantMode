using System;

namespace ExtendedVariants.Variants {
    public class AllowLeavingTheoBehind : AbstractExtendedVariant {
        public AllowLeavingTheoBehind() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
