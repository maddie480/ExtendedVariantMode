using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class JellyfishEverywhere : AbstractExtendedVariant {

        public JellyfishEverywhere() : base(variantType: typeof(int), defaultVariantValue: 0) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
        }

        private static void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                addJellyfishToLevel(self);
            }
        }

        private static IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            yield return new SwapImmediately(orig(self, next, direction));
            addJellyfishToLevel(self);
        }

        private static void addJellyfishToLevel(Level level) {
            for (int i = 0; i < GetVariantValue<int>(Variant.JellyfishEverywhere); i++) {
                Player player = level.Tracker.GetEntity<Player>();
                if (player != null && player.Holding?.Entity?.GetType() != typeof(Glider)) {
                    // player is here, and is not holding jellyfish.
                    // spawn a jellyfish near them.

                    Vector2 playerPosition = player.Position;

                    Glider jellyfish = new Glider(playerPosition, true, false);

                    // offset the jellyfish if there are multiple
                    if (GetVariantValue<int>(Variant.JellyfishEverywhere) == 2) {
                        if (i == 0) jellyfish.Position.X -= 10;
                        else jellyfish.Position.X += 10;
                    } else if (GetVariantValue<int>(Variant.JellyfishEverywhere) == 3) {
                        if (i == 1) jellyfish.Position.X -= 20;
                        else if (i == 2) jellyfish.Position.X += 20;
                    }

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

                    if (collideOrOffscreenCheck(level, jellyfish)) {
                        Logger.Log(LogLevel.Warn, "ExtendedVariantMode/JellyfishEverywhere", "Could not find a position to spawn that jellyfish!");
                    } else {
                        // spawn that jellyfish then
                        // (we spawn a new one because we want its startPos to be the right one.)
                        level.Add(new Glider(jellyfish.Position, true, false));
                        level.Entities.UpdateLists();
                    }
                }
            }
        }

        private static bool collideOrOffscreenCheck(Level level, Glider jellyfish) {
            return jellyfish.Position.X + jellyfish.Collider.Right > level.Bounds.Right
                || jellyfish.Position.X + jellyfish.Collider.Left < level.Bounds.Left
                || jellyfish.Position.Y + jellyfish.Collider.Top < level.Bounds.Top
                || Collide.Check(jellyfish, level.Tracker.Entities[typeof(Solid)]);
        }
    }
}
