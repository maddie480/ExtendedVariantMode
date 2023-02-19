using System;

namespace ExtendedVariants.Variants {
    public class BadelineLag : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1.55f;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value == 0 ? 1.55f : value / 10f;
        }
    }
}
