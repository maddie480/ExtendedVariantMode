using System;

namespace ExtendedVariants.Variants {
    class DisableSeekerSlowdown : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.DisableSeekerSlowdown;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.DisableSeekerSlowdown = (value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DisableSeekerSlowdown = (bool) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
