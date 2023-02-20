using Celeste;
using Monocle;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class AirDashes : AbstractVanillaVariant {
        public override Type GetVariantType() {
            return typeof(Assists.DashModes);
        }

        public override object GetDefaultVariantValue() {
            return Assists.DashModes.Normal;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return (Assists.DashModes) value;
        }

        public override void VariantValueChanged() {
            Assists.DashModes dashMode = applyAssists(vanillaAssists, out _).DashMode;

            Player player = Engine.Scene?.Tracker.GetEntity<Player>();
            if (player != null) {
                player.Dashes = Math.Min(player.Dashes, dashMode != Assists.DashModes.Normal ? 2 : player.MaxDashes);
            }
        }

        protected override Assists applyVariantValue(Assists target, object value) {
            target.DashMode = (Assists.DashModes) value;
            return target;
        }
    }
}
