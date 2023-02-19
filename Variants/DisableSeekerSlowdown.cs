using Celeste;
using Monocle;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class DisableSeekerSlowdown : AbstractExtendedVariant {
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
            if (GetVariantValue<bool>(Variant.DisableSeekerSlowdown) && Engine.Scene is Level level && level.Tracker.CountEntities<Seeker>() != 0) {
                // since we are in a map with seekers and we are killing slowdown, set speed to 1 to be sure we aren't making the current slowdown permanent. :maddyS:
                Engine.TimeRate = 1f;
            }
        }
    }
}
