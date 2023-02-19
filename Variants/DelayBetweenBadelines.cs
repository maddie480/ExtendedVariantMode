using System;

namespace ExtendedVariants.Variants {
    public class DelayBetweenBadelines : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 0.4f;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }
    }
}
