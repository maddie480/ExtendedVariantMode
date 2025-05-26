using System;

namespace ExtendedVariants.Variants {
    public class HiccupStrength : AbstractExtendedVariant {
        public HiccupStrength() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }
    }
}
