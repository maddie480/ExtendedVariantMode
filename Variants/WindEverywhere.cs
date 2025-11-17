using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class WindEverywhere : AbstractExtendedVariant {

        private Random randomGenerator = new Random();

        private bool snowBackdropAddedByEVM = false;

        public enum WindPattern {
            Default, None, Left, Right, LeftStrong, RightStrong, RightCrazy, LeftOnOff, RightOnOff, Alternating,
            LeftOnOffFast, RightOnOffFast, Down, Up, Random
        }

        private static WindEverywhere instance;
        public WindEverywhere() : base(variantType: typeof(WindPattern), defaultVariantValue: WindPattern.Default) {
            instance = this;
        }

        public override object ConvertLegacyVariantValue(int value) {
            // you know, 5 obviously means RightCrazy :a:
            WindController.Patterns[] allPatterns = new WindController.Patterns[] {
                WindController.Patterns.Left, WindController.Patterns.Right, WindController.Patterns.LeftStrong, WindController.Patterns.RightStrong, WindController.Patterns.RightCrazy,
                WindController.Patterns.LeftOnOff, WindController.Patterns.RightOnOff, WindController.Patterns.Alternating, WindController.Patterns.LeftOnOffFast,
                WindController.Patterns.RightOnOffFast, WindController.Patterns.Down, WindController.Patterns.Up
            };

            if (value == 0) {
                // default pattern
                return WindPattern.Default;
            } else if (value == allPatterns.Length + 1) {
                // random wind
                return WindPattern.Random;
            } else {
                // chosen wind pattern
                return (WindPattern) allPatterns[value - 1];
            }
        }

        public override void SetRandomSeed(int seed) {
            randomGenerator = new Random(seed);
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;
            Everest.Events.Level.OnExit += onLevelExit;

            IL.Celeste.Wire.Render += onWireRender;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
            Everest.Events.Level.OnExit -= onLevelExit;

            IL.Celeste.Wire.Render -= onWireRender;

            // if we are in a level and extended variants added a wind backdrop, clean it up.
            if (snowBackdropAddedByEVM && Engine.Scene.GetType() == typeof(Level)) {
                Level level = Engine.Scene as Level;

                snowBackdropAddedByEVM = false;
                level.Foreground.Backdrops.RemoveAll(backdrop => backdrop.GetType() == typeof(ExtendedVariantWindSnowFG));
            }
        }

        private static void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                instance.applyWind(self);
            }
        }

        private static IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            yield return new SwapImmediately(orig(self, next, direction));
            instance.applyWind(self);
        }

        private void applyWind(Level level) {
            if (GetVariantValue<WindPattern>(Variant.WindEverywhere) != WindPattern.Default) {
                WindController.Patterns selectedPattern;
                if (GetVariantValue<WindPattern>(Variant.WindEverywhere) == WindPattern.Random) {
                    // pick up random wind
                    WindController.Patterns[] allPatterns = new WindController.Patterns[] {
                        WindController.Patterns.Left, WindController.Patterns.Right, WindController.Patterns.LeftStrong, WindController.Patterns.RightStrong, WindController.Patterns.RightCrazy,
                            WindController.Patterns.LeftOnOff, WindController.Patterns.RightOnOff, WindController.Patterns.Alternating, WindController.Patterns.LeftOnOffFast,
                            WindController.Patterns.RightOnOffFast, WindController.Patterns.Down, WindController.Patterns.Up
                    };

                    selectedPattern = allPatterns[randomGenerator.Next(allPatterns.Length)];
                } else {
                    // pick up the chosen wind pattern
                    selectedPattern = (WindController.Patterns) Enum.Parse(typeof(WindController.Patterns),
                        Enum.GetName(typeof(WindPattern), GetVariantValue<WindPattern>(Variant.WindEverywhere)));
                }

                // and apply it; this is basically what Wind Trigger does
                WindController windController = level.Entities.FindFirst<WindController>();
                if (windController == null) {
                    windController = new WindController(selectedPattern);
                    windController.SetStartPattern();
                    level.Add(windController);
                    level.Entities.UpdateLists();
                } else {
                    windController.SetPattern(selectedPattern);
                }
            }

            if (GetVariantValue<WindPattern>(Variant.WindEverywhere) != WindPattern.Default && GetVariantValue<WindPattern>(Variant.WindEverywhere) != WindPattern.None) {
                if (!snowBackdropAddedByEVM) {
                    // add the styleground / backdrop used in Golden Ridge to make wind actually visible.
                    // ExtendedVariantWindSnowFG will hide itself if a vanilla backdrop supporting wind is already present or appears.
                    level.Foreground.Backdrops.Add(new ExtendedVariantWindSnowFG() { Alpha = 0f });
                    snowBackdropAddedByEVM = true;
                }

                // also switch the audio ambience so that wind can actually be heard too
                // (that's done by switching to the ch4 audio ambience. yep)
                Audio.SetAmbience("event:/env/amb/04_main", true);
            } else if (snowBackdropAddedByEVM) {
                // remove the backdrop
                level.Foreground.Backdrops.RemoveAll(backdrop => backdrop.GetType() == typeof(ExtendedVariantWindSnowFG));
                snowBackdropAddedByEVM = false;
            }
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            snowBackdropAddedByEVM = false;
        }

        private static void onWireRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we'll replace "level.VisualWind" with "transformVisualWind(level.VisualWind)"
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Level>("get_VisualWind"))) {
                Logger.Log("ExtendedVariantMode/WindEverywhere", $"Fixing wires with wind at {cursor.Index} in IL code for Wire.Render");

                cursor.EmitDelegate<Func<float, float>>(transformVisualWind);
            }
        }

        private static float transformVisualWind(float vanilla) {
            if (GetVariantValue<WindPattern>(Variant.WindEverywhere) == WindPattern.Default) {
                // variant disabled: don't affect vanilla.
                return vanilla;
            }

            // VisualWind = Wind.X + WindSine. Wind.X seems to make the wires freak out, so remove it.
            return (Engine.Scene as Level).WindSine;
        }
    }
}
