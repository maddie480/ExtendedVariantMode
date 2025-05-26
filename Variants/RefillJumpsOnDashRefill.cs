using System;

namespace ExtendedVariants.Variants {
    public class RefillJumpsOnDashRefill : AbstractExtendedVariant {
        public RefillJumpsOnDashRefill() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
