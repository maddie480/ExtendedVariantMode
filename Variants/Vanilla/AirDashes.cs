using Celeste;
using Monocle;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class AirDashes : AbstractVanillaVariant {
        public override Type GetVariantType() {
            return typeof(Assists.DashModes);
        }

        public override object GetVariantValue() {
            return SaveData.Instance.Assists.DashMode;
        }

        public override object GetDefaultVariantValue() {
            return Assists.DashModes.Normal;
        }

        public override void SetLegacyVariantValue(int value) {
            SetVariantValue((Assists.DashModes) value);
        }

        protected override void DoSetVariantValue(object value) {
            SaveData.Instance.Assists.DashMode = (Assists.DashModes) value;

            Player player = Engine.Scene?.Tracker.GetEntity<Player>();
            if (player != null) {
                player.Dashes = Math.Min(player.Dashes, player.MaxDashes);
            }
        }
    }
}
