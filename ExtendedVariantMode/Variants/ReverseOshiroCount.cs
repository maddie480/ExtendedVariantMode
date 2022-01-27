using System;

namespace ExtendedVariants.Variants {
    class ReverseOshiroCount : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetDefaultVariantValue() {
            return 0;
        }

        public override object GetVariantValue() {
            return Settings.ReverseOshiroCount;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.ReverseOshiroCount = value;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.ReverseOshiroCount = (int) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
