using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class RoomLighting : AbstractExtendedVariant {

        private float initialBaseLightingAlpha = -1f;

        public RoomLighting() : base(variantType: typeof(float), defaultVariantValue: -1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            if (value == -1f) {
                return -1f;
            } else {
                return value / 10f;
            }
        }

        public override void VariantValueChanged() {
            if (Engine.Scene?.GetType() == typeof(Level)) {
                // currently in level, change lighting right away
                Level lvl = (Engine.Scene as Level);
                lvl.Lighting.Alpha = (GetVariantValue<float>(Variant.RoomLighting) == -1f ? (lvl.DarkRoom ? lvl.Session.DarkRoomAlpha : lvl.BaseLightingAlpha + lvl.Session.LightingAlphaAdd) : 1 - (GetVariantValue<float>(Variant.RoomLighting)));
            }
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

            if (GetVariantValue<float>(Variant.RoomLighting) != -1f) {
                float lightingTarget = 1 - GetVariantValue<float>(Variant.RoomLighting);
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
            yield return new SwapImmediately(orig(self, next, direction));

            // Resets the BaseLightingAlpha to its initial value (if modified by modLoadLevel)
            if (initialBaseLightingAlpha != -1f) {
                self.BaseLightingAlpha = initialBaseLightingAlpha;
                initialBaseLightingAlpha = -1f;
            }
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

            if (GetVariantValue<float>(Variant.RoomLighting) != -1f) {
                // be sure to lock the lighting alpha to the value set by the player
                float lightingTarget = 1 - GetVariantValue<float>(Variant.RoomLighting);
                (self.Scene as Level).Lighting.Alpha = lightingTarget;
            }
        }
    }
}
