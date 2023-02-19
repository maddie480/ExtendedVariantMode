using System;

namespace ExtendedVariants.Variants {
    public class DashTimerMultiplier : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1.0f;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }
    }
}
