using System;

namespace ExtendedVariants.Variants {
    public class FirstBadelineSpawnRandom : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.FirstBadelineSpawnRandom;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.FirstBadelineSpawnRandom = (value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            Settings.FirstBadelineSpawnRandom = (bool) value;
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
