using System;

namespace ExtendedVariants.Variants {
    public class AllowLeavingTheoBehind : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.AllowLeavingTheoBehind;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.AllowLeavingTheoBehind = (value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            Settings.AllowLeavingTheoBehind = (bool) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
