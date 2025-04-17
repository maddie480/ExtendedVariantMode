using System;

namespace ExtendedVariants.Variants {
    public class ResetJumpCountOnGround : AbstractExtendedVariant {
        public ResetJumpCountOnGround() : base(variantType: typeof(bool), defaultVariantValue: true) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
