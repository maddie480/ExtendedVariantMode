using System;

namespace ExtendedVariants.Variants {
    class BadelineAttackPattern : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetDefaultVariantValue() {
            return 0;
        }

        public override object GetVariantValue() {
            return Settings.BadelineAttackPattern;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.BadelineAttackPattern = value;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.BadelineAttackPattern = (int) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
