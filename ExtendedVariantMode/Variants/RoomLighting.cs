using Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedVariants.Variants {
    public class RoomLighting : AbstractExtendedVariant {

        private float initialBaseLightingAlpha = -1f;

        public override int GetDefaultValue() {
            return -1;
        }

        public override int GetValue() {
            return Settings.RoomLighting;
        }

        public override void SetValue(int value) {
            Settings.RoomLighting = value;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;
            On.Celeste.LightFadeTrigger.OnStay += modLightFadeTriggerOnStay;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
            On.Celeste.LightFadeTrigger.OnStay -= modLightFadeTriggerOnStay;
        }

        /// <summary>
        /// Mods the lighting of a new room being loaded.
        /// </summary>
        /// <param name="self">The level we are in</param>
        /// <param name="introType">How the player enters the level</param>
        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (Settings.RoomLighting != -1) {
                float lightingTarget = 1 - Settings.RoomLighting / 10f;
                if (playerIntro == Player.IntroTypes.Transition) {
                    // we force the game into thinking this is not a dark room, so that it uses the BaseLightingAlpha + LightingAlphaAdd formula
                    // (this change is not permanent, exiting and re-entering will reset this flag)
                    self.DarkRoom = false;

                    // we mod BaseLightingAlpha temporarily so that it adds up with LightingAlphaAdd to the right value: the transition routine will smoothly switch to it
                    // (we are sure BaseLightingAlpha will not move for the entire session: it's only set when the map is loaded)
                    initialBaseLightingAlpha = self.BaseLightingAlpha;
                    self.BaseLightingAlpha = lightingTarget - self.Session.LightingAlphaAdd;
                } else {
                    // just set the initial value
                    self.Lighting.Alpha = lightingTarget;
                }
            }
        }
        private IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            // just make sure the whole transition routine is over
            IEnumerator origEnum = orig(self, next, direction);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

            // Resets the BaseLightingAlpha to its initial value (if modified by modLoadLevel)
            if (initialBaseLightingAlpha != -1f) {
                self.BaseLightingAlpha = initialBaseLightingAlpha;
                initialBaseLightingAlpha = -1f;
            }

            yield break;
        }

        /// <summary>
        /// Locks the lighting to the value set by the user even when they hit a Light Fade Trigger.
        /// (The Light Fade Trigger will still change the LightingAlphaAdd, so it will be effective immediately if the variant is disabled.)
        /// </summary>
        /// <param name="orig">The original method</param>
        /// <param name="self">The trigger itself</param>
        /// <param name="player">The player hitting the trigger</param>
        private void modLightFadeTriggerOnStay(On.Celeste.LightFadeTrigger.orig_OnStay orig, LightFadeTrigger self, Player player) {
            orig(self, player);

            if (Settings.RoomLighting != -1) {
                // be sure to lock the lighting alpha to the value set by the player
                float lightingTarget = 1 - Settings.RoomLighting / 10f;
                (self.Scene as Level).Lighting.Alpha = lightingTarget;
            }
        }
    }
}
