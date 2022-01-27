using System;

namespace ExtendedVariants.Variants {
    class HiccupStrength : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object GetVariantValue() {
            return Settings.HiccupStrength;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.HiccupStrength = value / 10f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.HiccupStrength = (float) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
