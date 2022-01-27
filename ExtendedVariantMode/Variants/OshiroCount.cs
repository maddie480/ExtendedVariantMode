using System;

namespace ExtendedVariants.Variants {
    class OshiroCount : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetDefaultVariantValue() {
            return 1;
        }

        public override object GetVariantValue() {
            return Settings.OshiroCount;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.OshiroCount = value;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.OshiroCount = (int) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
