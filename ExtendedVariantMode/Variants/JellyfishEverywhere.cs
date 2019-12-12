using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;

namespace ExtendedVariants.Variants {
    public class JellyfishEverywhere : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.JellyfishEverywhere ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.JellyfishEverywhere = (value != 0);
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
                addJellyfishToLevel(self);
            }
        }
        
        private IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            // just make sure the whole transition routine is over
            IEnumerator origEnum = orig(self, next, direction);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

            addJellyfishToLevel(self);

            yield break;
        }

        private void addJellyfishToLevel(Level level) {
            if (Settings.JellyfishEverywhere) {
                Player player = level.Tracker.GetEntity<Player>();
                if (player != null && player.Holding?.Entity?.GetType() != typeof(Glider)) {
                    // player is here, and is not holding jellyfish.
                    // spawn a jellyfish near them.

                    Vector2 playerPosition = player.Position;

                    Glider jellyfish = new Glider(playerPosition, true, false);

                    // move it up 20px
                    jellyfish.Position.Y -= 20;
                    if (collideOrOffscreenCheck(level, jellyfish)) {
                        // ... 20px right then?
                        jellyfish.Position.Y += 20;
                        jellyfish.Position.X += 20;

                        if (collideOrOffscreenCheck(level, jellyfish)) {
                            // okay, let's try 20px left
                            jellyfish.Position.X -= 40;

                            if (collideOrOffscreenCheck(level, jellyfish)) {
                                // still not good? last try: 20px below
                                jellyfish.Position.X += 20;
                                jellyfish.Position.Y += 20;
                            }
                        }
                    }

                    if(collideOrOffscreenCheck(level, jellyfish)) {
                        Logger.Log("ExtendedVariantMode/JellyfishEverywhere", "Could not find a position to spawn that jellyfish!");
                    } else {
                        // spawn that jellyfish then
                        // (we spawn a new one because we want its startPos to be the right one.)
                        level.Add(new Glider(jellyfish.Position, true, false));
                        level.Entities.UpdateLists();
                    }
                }
            }
        }

        private bool collideOrOffscreenCheck(Level level, Glider jellyfish) {
            return jellyfish.Position.X + jellyfish.Collider.Right > level.Bounds.Right
                || jellyfish.Position.X + jellyfish.Collider.Left < level.Bounds.Left
                || jellyfish.Position.Y + jellyfish.Collider.Top < level.Bounds.Top
                || Collide.Check(jellyfish, level.Tracker.Entities[typeof(Solid)]);
        }
    }
}
