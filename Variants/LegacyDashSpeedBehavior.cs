using ExtendedVariants.Module;
using System;

namespace ExtendedVariants.Variants {
    public class LegacyDashSpeedBehavior : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.LegacyDashSpeedBehavior;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.LegacyDashSpeedBehavior = (value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            Settings.LegacyDashSpeedBehavior = (bool) value;

            // hot swap the "dash speed" variant handler
            ExtendedVariantsModule.Instance.VariantHandlers[ExtendedVariantsModule.Variant.DashSpeed].Unload();
            ExtendedVariantsModule.Instance.VariantHandlers[ExtendedVariantsModule.Variant.DashSpeed] = Settings.LegacyDashSpeedBehavior ? (AbstractExtendedVariant) new DashSpeedOld() : new DashSpeed();
            ExtendedVariantsModule.Instance.VariantHandlers[ExtendedVariantsModule.Variant.DashSpeed].Load();
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
