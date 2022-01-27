using System;

namespace ExtendedVariants.Variants {
    class ChaserCount : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetDefaultVariantValue() {
            return 1;
        }

        public override object GetVariantValue() {
            return Settings.ChaserCount;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.ChaserCount = value;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.ChaserCount = (int) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
