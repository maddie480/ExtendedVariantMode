using System;

namespace ExtendedVariants.Variants {
    class BadelineLag : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1.55f;
        }

        public override object GetVariantValue() {
            return Settings.BadelineLag;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.BadelineLag = (value == 0 ? 1.55f : value / 10f);
        }

        protected override void DoSetVariantValue(object value) {
            Settings.BadelineLag = (float) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
