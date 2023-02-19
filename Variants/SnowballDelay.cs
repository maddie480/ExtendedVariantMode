using System;

namespace ExtendedVariants.Variants {
    public class SnowballDelay : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 0.8f;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }
    }
}
