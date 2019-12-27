using Celeste;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Linq;

namespace ExtendedVariants.Variants {
    class RisingLavaEverywhere : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.RisingLavaEverywhere ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.RisingLavaEverywhere = (value != 0);
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
        }
        
        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                addRisingLavaToLevel(self);
            }
        }
        
        private IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            // just make sure the whole transition routine is over
            IEnumerator origEnum = orig(self, next, direction);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

            addRisingLavaToLevel(self);

            yield break;
        }

        private void addRisingLavaToLevel(Level level) {
            if(Settings.RisingLavaEverywhere && level.Entities.All(entity => entity.GetType() != typeof(RisingLava) && entity.GetType() != typeof(SandwichLava))) {
                // we should add a rising lava entity to the level, since there isn't any at the moment.
                Player player = level.Tracker.GetEntity<Player>();
                if(player != null) {
                    // spawn lava if the player is at the bottom of the screen, ice if they are at the top.
                    bool shouldBeIce = (player.Y < level.Bounds.Center.Y);
                    level.Add(new ExtendedVariantSandwichLava(shouldBeIce, player.X - 10f));
                    level.Entities.UpdateLists();
                }
            }
        }
    }
}
