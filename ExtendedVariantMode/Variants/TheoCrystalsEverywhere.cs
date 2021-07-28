using Celeste;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

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

            } else if ((Settings.AllowLeavingTheoBehind || Settings.AllowThrowingTheoOffscreen) &&
                !(self.Tracker.GetEntity<Player>()?.Holding?.Entity is TheoCrystal)) {

                // player is transitioning into a new room, but doesn't have Theo with them.
                injectTheoCrystalAfterTransition(self);

            } else if (self.Tracker.CountEntities<ExtendedVariantTheoCrystal>() == 0) {
                // player is transitioning into a new room, and no Theo crystal is in the room: we should add one if the variant is enabled.
                injectTheoCrystalAfterTransition(self);
            }
        }

        private void injectTheoCrystal(Level level) {
            if (Settings.TheoCrystalsEverywhere) {
                Player player = level.Tracker.GetEntity<Player>();
                bool hasCrystalInBaseLevel = level.Tracker.CountEntities<TheoCrystal>() != 0;

                // check if the base level already has a crystal
                if (player != null && !hasCrystalInBaseLevel) {
                    // add a Theo Crystal where the player is
                    level.Add(Settings.AllowThrowingTheoOffscreen ? new ExtendedVariantTheoCrystalGoingOffscreen(player.Position) : new ExtendedVariantTheoCrystal(player.Position));
                    level.Entities.UpdateLists();
                }
            }
        }

        private void injectTheoCrystalAfterTransition(Level level) {
            if (Settings.TheoCrystalsEverywhere) {
                Player player = level.Tracker.GetEntity<Player>();
                bool hasCrystalInBaseLevel = level.Session.LevelData.Entities.Any(entity => entity.Name == "theoCrystal");

                // check if the base level already has a crystal
                if (player != null && !hasCrystalInBaseLevel) {
                    // add a Theo Crystal where the spawn point nearest to player is
                    Vector2 spawn = level.GetSpawnPoint(player.Position);
                    level.Add(Settings.AllowThrowingTheoOffscreen ? new ExtendedVariantTheoCrystalGoingOffscreen(spawn) : new ExtendedVariantTheoCrystal(spawn));
                    level.Entities.UpdateLists();
                }
            }
        }

        private void modEnforceBounds(On.Celeste.Level.orig_EnforceBounds orig, Level self, Player player) {
            if (isTheoCrystalsEverywhere() && !isAllowLeavingTheoBehind() &&
                // the player is holding nothing...
                (player.Holding == null || player.Holding.Entity == null || !player.Holding.IsHeld ||
                // or this thing is not a Theo crystal (f.e. jellyfish)
                (player.Holding.Entity.GetType() != typeof(TheoCrystal) && player.Holding.Entity.GetType() != typeof(ExtendedVariantTheoCrystal) && player.Holding.Entity.GetType() != typeof(ExtendedVariantTheoCrystalGoingOffscreen))) &&
                // but there is a Theo crystal on screen so the player has to grab it
                self.Tracker.CountEntities<ExtendedVariantTheoCrystal>() != 0) {

                // the player does not hold Theo, check if they try to leave the screen without him
                Rectangle bounds = self.Bounds;
                if (player.Left < bounds.Left) {
                    // prevent the player from going left
                    player.Left = bounds.Left;
                    player.OnBoundsH();
                } else if (player.Right > bounds.Right) {
                    // prevent the player from going right
                    player.Right = bounds.Right;
                    player.OnBoundsH();
                } else if (player.Top < bounds.Top) {
                    // prevent the player from going up
                    player.Top = bounds.Top;
                    player.OnBoundsV();
                } else if (player.Bottom > bounds.Bottom) {
                    if (SaveData.Instance.Assists.Invincible) {
                        // bounce the player off the bottom of the screen (= falling into a pit with invincibility on)
                        player.Play("event:/game/general/assist_screenbottom");
                        player.Bounce(bounds.Bottom);
                    } else {
                        // kill the player if they try to go down
                        player.Die(Vector2.Zero);
                    }
                } else {
                    // no screen transition detected => proceed to vanilla
                    orig(self, player);
                }
            } else {
                // the variant is disabled, we are allowed to leave Theo behind, or the player is holding Theo => we have no business here.
                orig(self, player);
            }
        }

        private bool isTheoCrystalsEverywhere() {
            // the variant is on, or an Extended Variant Theo crystal is in the level.
            return Settings.TheoCrystalsEverywhere ||
                Engine.Scene.Tracker.GetEntities<ExtendedVariantTheoCrystal>().OfType<ExtendedVariantTheoCrystal>().Any(t => t.SpawnedAsEntity);
        }

        private bool isAllowLeavingTheoBehind() {
            // get all Theo crystals in the level that were placed in Ahorn.
            IEnumerable<ExtendedVariantTheoCrystal> entities =
                Engine.Scene.Tracker.GetEntities<ExtendedVariantTheoCrystal>().OfType<ExtendedVariantTheoCrystal>().Where(t => t.SpawnedAsEntity);

            if (entities.Count() == 0) {
                // there is none! just use the extended variant setting.
                return Settings.AllowLeavingTheoBehind;
            } else {
                // allow leaving Theo behind of all Theos on the screen can be left behind.
                return entities.All(t => t.AllowLeavingBehind);
            }
        }
    }
}
