using System;

namespace ExtendedVariants.Variants {
    public class RisingLavaSpeed : AbstractExtendedVariant {
        public RisingLavaSpeed() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }
    }
}
