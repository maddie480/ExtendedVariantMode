using System;

namespace ExtendedVariants.Variants {
    public class DontRefillStaminaOnGround : AbstractExtendedVariant {
        public DontRefillStaminaOnGround() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
