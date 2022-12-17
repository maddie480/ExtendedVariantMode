using System;

namespace ExtendedVariants.Variants {
    public class ResetJumpCountOnGround : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return true;
        }

        public override object GetVariantValue() {
            return Settings.ResetJumpCountOnGround;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.ResetJumpCountOnGround = (value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            Settings.ResetJumpCountOnGround = (bool) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
