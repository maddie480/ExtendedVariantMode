using System;

namespace ExtendedVariants.Variants {
    class DontRefillStaminaOnGround : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.DontRefillStaminaOnGround;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.DontRefillStaminaOnGround = (value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DontRefillStaminaOnGround = (bool) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
