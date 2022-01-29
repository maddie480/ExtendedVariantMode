using Celeste;
using Monocle;
using System;

namespace ExtendedVariants.Variants {
    public class DisableSeekerSlowdown : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.DisableSeekerSlowdown;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.DisableSeekerSlowdown = (value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DisableSeekerSlowdown = (bool) value;

            if (Settings.DisableSeekerSlowdown && Engine.Scene is Level level && level.Tracker.CountEntities<Seeker>() != 0) {
                // since we are in a map with seekers and we are killing slowdown, set speed to 1 to be sure we aren't making the current slowdown permanent. :maddyS:
                Engine.TimeRate = 1f;
            }
        }

        public override void Load() {
            // this setting is used elsewhere
        }

        public override void Unload() {
            // this setting is used elsewhere
        }
    }
}
