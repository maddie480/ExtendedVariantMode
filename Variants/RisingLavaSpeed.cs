using System;

namespace ExtendedVariants.Variants {
    public class RisingLavaSpeed : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object GetVariantValue() {
            return Settings.RisingLavaSpeed;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.RisingLavaSpeed = value / 10f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.RisingLavaSpeed = (float) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
