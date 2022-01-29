using System;

namespace ExtendedVariants.Variants {
    public class DelayBetweenBadelines : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 0.4f;
        }

        public override object GetVariantValue() {
            return Settings.DelayBetweenBadelines;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.DelayBetweenBadelines = value / 10f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DelayBetweenBadelines = (float) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
