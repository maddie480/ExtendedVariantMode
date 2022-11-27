using System;

namespace ExtendedVariants.Variants {
    public class DashTimerMultiplier : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1.0f;
        }

        public override object GetVariantValue() {
            return Settings.DashTimerMultiplier;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.DashTimerMultiplier = value / 10f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DashTimerMultiplier = (float) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
