using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace ExtendedVariants.Variants {
    public class MadelineBackpackMode : AbstractExtendedVariant {
        public enum MadelineBackpackModes { Default, NoBackpack, Backpack }

        public override Type GetVariantType() {
            return typeof(MadelineBackpackModes);
        }

        public override object GetDefaultVariantValue() {
            return MadelineBackpackModes.Default;
        }

        public override object GetVariantValue() {
            return Settings.MadelineBackpackMode;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.MadelineBackpackMode = (MadelineBackpackModes) value;

            Player p = Engine.Scene?.Tracker.GetEntity<Player>();
            if (p != null) {
                if (p.Active) {
                    p.ResetSpriteNextFrame(p.DefaultSpriteMode);
                } else {
                    p.ResetSprite(p.DefaultSpriteMode);
                }
            }
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.MadelineBackpackMode = (MadelineBackpackModes) value;
        }

        public override void Load() {
            On.Celeste.PlayerSprite.ctor += onPlayerSpriteConstructor;
            On.Celeste.LevelLoader.ctor += onLevelLoaderConstructor;

            if (Engine.Scene is Level) {
                // we're late! initialize the sprites now.
                initializeRollBackpackSprites();
            }
        }

        public override void Unload() {
            On.Celeste.PlayerSprite.ctor -= onPlayerSpriteConstructor;
            On.Celeste.LevelLoader.ctor -= onLevelLoaderConstructor;
        }

        private void onPlayerSpriteConstructor(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode) {
            // modify Madeline or MadelineNoBackpack as needed, if the variant is enabled.
            if (mode == PlayerSpriteMode.Madeline || mode == PlayerSpriteMode.MadelineNoBackpack) {
                if (Settings.MadelineBackpackMode == MadelineBackpackModes.Backpack) {
                    mode = PlayerSpriteMode.Madeline;
                } else if (Settings.MadelineBackpackMode == MadelineBackpackModes.NoBackpack) {
                    mode = PlayerSpriteMode.MadelineNoBackpack;
                }
            }

            orig(self, mode);
        }

        private void onLevelLoaderConstructor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            orig(self, session, startPosition);

            // Everest reinitializes GFX.SpriteBank in the LevelLoader constructor, so we need to initialize the sprites again.
            initializeRollBackpackSprites();
        }

        private void initializeRollBackpackSprites() {
            Dictionary<string, Sprite.Animation> player = GFX.SpriteBank.SpriteData["player"].Sprite.Animations;
            Dictionary<string, Sprite.Animation> playerNoBackpack = GFX.SpriteBank.SpriteData["player_no_backpack"].Sprite.Animations;

            // copy the roll animations from player_no_backpack to player, to prevent crashes in Farewell if the backpack is forced.
            if (!player.ContainsKey("roll")) {
                player.Add("roll", playerNoBackpack["roll"]);
            }
            if (!player.ContainsKey("rollGetUp")) {
                player.Add("rollGetUp", playerNoBackpack["rollGetUp"]);
            }
            if (!player.ContainsKey("downed")) {
                player.Add("downed", playerNoBackpack["downed"]);
            }
        }
    }
}
