using Celeste;
using Monocle;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class PlayAsBadeline : AbstractVanillaVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetVariantValue() {
            return SaveData.Instance.Assists.PlayAsBadeline;
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override void SetLegacyVariantValue(int value) {
            SetVariantValue(value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            SaveData.Instance.Assists.PlayAsBadeline = (bool) value;

            Player player = Engine.Scene?.Tracker.GetEntity<Player>();
            if (player != null) {
                PlayerSpriteMode mode = (SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode);
                if (player.Active) {
                    player.ResetSpriteNextFrame(mode);
                } else {
                    player.ResetSprite(mode);
                }
            }
        }
    }
}
