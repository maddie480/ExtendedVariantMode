using Celeste;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Variants {
    class TheoCrystalsEverywhere : AbstractExtendedVariant {
        
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.TheoCrystalsEverywhere ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.TheoCrystalsEverywhere = (value != 0);
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.EnforceBounds += modEnforceBounds;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.EnforceBounds -= modEnforceBounds;
        }

        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                injectTheoCrystal(self);
            }
        }

        private void injectTheoCrystal(Level level) {
            if (Settings.TheoCrystalsEverywhere) {
                Player player = level.Tracker.GetEntity<Player>();
                bool hasCrystalInBaseLevel = level.Tracker.CountEntities<TheoCrystal>() != 0;

                // check if the base level already has a crystal
                if (player != null && !hasCrystalInBaseLevel) {
                    // add a Theo Crystal where the player is
                    level.Add(new ExtendedVariantTheoCrystal(player.Position));
                    level.Entities.UpdateLists();
                }
            }
        }

        private void modEnforceBounds(On.Celeste.Level.orig_EnforceBounds orig, Level self, Player player) {
            if(Settings.TheoCrystalsEverywhere && (player.Holding == null || !player.Holding.IsHeld) &&
                self.Tracker.CountEntities<ExtendedVariantTheoCrystal>() != 0) {

                // the player does not hold Theo, check if they try to leave the screen without him
                Rectangle bounds = self.Bounds;
                if (player.Left < bounds.Left) {
                    // prevent the player from going left
                    player.Left = bounds.Left;
                    player.OnBoundsH();
                } else if(player.Right > bounds.Right) {
                    // prevent the player from going right
                    player.Right = bounds.Right;
                    player.OnBoundsH();
                } else if(player.Top < bounds.Top) {
                    // prevent the player from going up
                    player.Top = bounds.Top;
                    player.OnBoundsV();
                } else if(player.Bottom > bounds.Bottom) {
                    // kill the player if they try to go down
                    player.Die(Vector2.Zero);
                } else {
                    // no screen transition detected or everything in order => proceed to vanilla
                    orig(self, player);
                }
            } else {
                // the variant is disabled or the player is holding Theo => we have no business here.
                orig(self, player);
            }
        }
    }
}
