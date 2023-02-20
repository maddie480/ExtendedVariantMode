using Celeste;
using Monocle;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class PlayAsBadeline : AbstractVanillaVariant {
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
            bool playAsBadeline = applyAssists(vanillaAssists, out _).PlayAsBadeline;

            Player player = Engine.Scene?.Tracker.GetEntity<Player>();
            if (player != null) {
                PlayerSpriteMode mode = (playAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode);
                if (player.Active) {
                    player.ResetSpriteNextFrame(mode);
                } else {
                    player.ResetSprite(mode);
                }
            }
        }

        protected override Assists applyVariantValue(Assists target, object value) {
            target.PlayAsBadeline = (bool) value;
            return target;
        }
    }
}
