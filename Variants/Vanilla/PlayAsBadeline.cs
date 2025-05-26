using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class PlayAsBadeline : AbstractVanillaVariant {
        private static Player latestPlayer = null;
        public PlayAsBadeline() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Player.ctor += onPlayerConstructor;
        }

        public override void Unload() {
            On.Celeste.Player.ctor -= onPlayerConstructor;
        }

        public override void VariantValueChanged() {
            bool playAsBadeline = getActiveAssistValues().PlayAsBadeline;
            Player player = Engine.Scene?.Tracker.GetEntity<Player>() ?? latestPlayer;

            if (player != null) {
                PlayerSpriteMode mode = playAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode;
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

        private static void onPlayerConstructor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);
            latestPlayer = self;
        }

    }
}
