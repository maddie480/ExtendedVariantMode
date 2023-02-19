using System;

namespace ExtendedVariants.Variants {
    public class BadelineAttackPattern : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetDefaultVariantValue() {
            return 0;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }
    }
}
