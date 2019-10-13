using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using System;
using System.Collections;

namespace ExtendedVariants.Variants {
    public class WindEverywhere : AbstractExtendedVariant {

        private Random randomGenerator = new Random();

        private bool snowBackdropAddedByEVM = false;

        public static readonly WindController.Patterns[] AvailableWindPatterns = new WindController.Patterns[] {
            WindController.Patterns.Left, WindController.Patterns.Right, WindController.Patterns.LeftStrong, WindController.Patterns.RightStrong, WindController.Patterns.RightCrazy,
                WindController.Patterns.LeftOnOff, WindController.Patterns.RightOnOff, WindController.Patterns.Alternating, WindController.Patterns.LeftOnOffFast,
                WindController.Patterns.RightOnOffFast, WindController.Patterns.Down, WindController.Patterns.Up
        };

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.WindEverywhere;
        }

        public override void SetValue(int value) {
            Settings.WindEverywhere = value;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;
            Everest.Events.Level.OnExit += onLevelExit;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
            Everest.Events.Level.OnExit -= onLevelExit;
        }
        
        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                applyWind(self);
            }
        }

        private IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            // just make sure the whole transition routine is over
            IEnumerator origEnum = orig(self, next, direction);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

            applyWind(self);

            yield break;
        }

        private void applyWind(Level level) {
            if (Settings.WindEverywhere != 0) {
                if(level.Foreground.Backdrops.Find(backdrop => backdrop.GetType() == typeof(WindSnowFG)) == null) {
                    // add the styleground / backdrop used in Golden Ridge to make wind actually visible
                    level.Foreground.Backdrops.Add(new WindSnowFG());
                    snowBackdropAddedByEVM = true;
                }

                // also switch the audio ambience so that wind can actually be heard too
                // (that's done by switching to the ch4 audio ambience. yep)
                Audio.SetAmbience("event:/env/amb/04_main", true);

                WindController.Patterns selectedPattern;
                if (Settings.WindEverywhere == AvailableWindPatterns.Length + 1) {
                    // pick up random wind
                    selectedPattern = AvailableWindPatterns[randomGenerator.Next(AvailableWindPatterns.Length)];
                } else {
                    // pick up the chosen wind pattern
                    selectedPattern = AvailableWindPatterns[Settings.WindEverywhere - 1];
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
            } else if (snowBackdropAddedByEVM) {
                // remove the backdrop
                level.Foreground.Backdrops.RemoveAll(backdrop => backdrop.GetType() == typeof(WindSnowFG));
                snowBackdropAddedByEVM = false;
            }
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            snowBackdropAddedByEVM = false;
        }
    }
}
