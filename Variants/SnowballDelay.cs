using System;

namespace ExtendedVariants.Variants {
    public class SnowballDelay : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 0.8f;
        }

        public override object GetVariantValue() {
            return Settings.SnowballDelay;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.SnowballDelay = value / 10f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.SnowballDelay = (float) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
