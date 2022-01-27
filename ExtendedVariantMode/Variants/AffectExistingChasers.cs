using System;

namespace ExtendedVariants.Variants {
    class AffectExistingChasers : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.AffectExistingChasers;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.AffectExistingChasers = (value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            Settings.AffectExistingChasers = (bool) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
