using System;

namespace ExtendedVariants.Variants {
    public class BadelineBossCount : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetDefaultVariantValue() {
            return 1;
        }

        public override object GetVariantValue() {
            return Settings.BadelineBossCount;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.BadelineBossCount = value;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.BadelineBossCount = (int) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
