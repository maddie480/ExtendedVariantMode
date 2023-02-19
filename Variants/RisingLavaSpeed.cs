using System;

namespace ExtendedVariants.Variants {
    public class RisingLavaSpeed : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }
    }
}
