using System;

namespace ExtendedVariants.Variants {
    public class ResetJumpCountOnGround : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return true;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
