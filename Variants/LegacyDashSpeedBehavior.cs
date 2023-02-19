using ExtendedVariants.Module;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class LegacyDashSpeedBehavior : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void VariantValueChanged() {
            if ((ExtendedVariantsModule.Instance.VariantHandlers[Variant.DashSpeed] is DashSpeedOld) != GetVariantValue<bool>(Variant.LegacyDashSpeedBehavior)) {
                // hot swap the "dash speed" variant handler
                ExtendedVariantsModule.Instance.VariantHandlers[Variant.DashSpeed].Unload();
                ExtendedVariantsModule.Instance.VariantHandlers[Variant.DashSpeed] = GetVariantValue<bool>(Variant.LegacyDashSpeedBehavior) ? (AbstractExtendedVariant) new DashSpeedOld() : new DashSpeed();
                ExtendedVariantsModule.Instance.VariantHandlers[Variant.DashSpeed].Load();
            }
        }
    }
}
