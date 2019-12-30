using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
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
                if(!snowBackdropAddedByEVM) {
                    // add the styleground / backdrop used in Golden Ridge to make wind actually visible.
                    // ExtendedVariantWindSnowFG will hide itself if a vanilla backdrop supporting wind is already present or appears.
                    level.Foreground.Backdrops.Add(new ExtendedVariantWindSnowFG() { Alpha = 0f });
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
                level.Foreground.Backdrops.RemoveAll(backdrop => backdrop.GetType() == typeof(ExtendedVariantWindSnowFG));
                snowBackdropAddedByEVM = false;
            }
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            snowBackdropAddedByEVM = false;
        }
        
        private void onWireRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we'll replace "level.VisualWind" with "transformVisualWind(level.VisualWind)"
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Level>("get_VisualWind"))) {
                Logger.Log("ExtendedVariantMode/WindEverywhere", $"Fixing wires with wind at {cursor.Index} in IL code for Wire.Render");

                cursor.EmitDelegate<Func<float, float>>(transformVisualWind);
            }
        }

        private float transformVisualWind(float vanilla) {
            if(Settings.WindEverywhere == 0) {
                // variant disabled: don't affect vanilla.
                return vanilla;
            }

            // VisualWind = Wind.X + WindSine. Wind.X seems to make the wires freak out, so remove it.
            return (Engine.Scene as Level).WindSine;
        }
    }
}
