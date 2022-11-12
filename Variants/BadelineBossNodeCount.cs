using System;

namespace ExtendedVariants.Variants {
    public class BadelineBossNodeCount : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetDefaultVariantValue() {
            return 1;
        }

        public override object GetVariantValue() {
            return Settings.BadelineBossNodeCount;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.BadelineBossNodeCount = value;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.BadelineBossNodeCount = (int) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
