using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class Stamina : AbstractExtendedVariant {

        private ILHook playerUpdateHook;
        private ILHook summitGemSmashRoutineHook;

        private bool forceRefillStamina;

        public override int GetDefaultValue() {
            return 11;
        }

        public override int GetValue() {
            return Settings.Stamina;
        }

        public override void SetValue(int value) {
            Settings.Stamina = value;
        }

        public override void Load() {
            IL.Celeste.Player.ClimbUpdate += patchOutStamina;
            IL.Celeste.Player.SwimBegin += patchOutStamina;
            IL.Celeste.Player.DreamDashBegin += patchOutStamina;
            IL.Celeste.Player.ctor += patchOutStamina;
            On.Celeste.Player.RefillStamina += modRefillStamina;

            On.Celeste.Player.OnTransition += modOnTransition;
            On.Celeste.Player.ctor += modPlayerConstructor;
            On.Celeste.Player.UseRefill += modPlayerUseRefill;

            playerUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), patchOutStamina);
            summitGemSmashRoutineHook = new ILHook(typeof(SummitGem).GetMethod("SmashRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), patchOutStamina);
        }

        public override void Unload() {
            IL.Celeste.Player.ClimbUpdate -= patchOutStamina;
            IL.Celeste.Player.SwimBegin -= patchOutStamina;
            IL.Celeste.Player.DreamDashBegin -= patchOutStamina;
            IL.Celeste.Player.ctor -= patchOutStamina;
            On.Celeste.Player.RefillStamina -= modRefillStamina;
            On.Celeste.Player.UseRefill -= modPlayerUseRefill;

            On.Celeste.Player.OnTransition -= modOnTransition;
            On.Celeste.Player.ctor -= modPlayerConstructor;

            if (playerUpdateHook != null) playerUpdateHook.Dispose();
            if (summitGemSmashRoutineHook != null) summitGemSmashRoutineHook.Dispose();
        }


        /// <summary>
        /// Replaces the default 110 stamina value with the one defined in the settings.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void patchOutStamina(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            // now, patch everything stamina-related (every instance of 110)
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(110f))) {
                Logger.Log("ExtendedVariantMode/Stamina", $"Patching stamina at index {cursor.Index} in CIL code for {cursor.Method.FullName}");

                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if (Settings.DontRefillStaminaOnGround && !forceRefillStamina) {
                        // return the player stamina: this will result in player.Stamina = player.Stamina, thus doing absolutely nothing.
                        return Engine.Scene.Tracker.GetEntity<Player>()?.Stamina ?? determineBaseStamina();
                    }
                    if (Settings.Stamina != 11) {
                        // mod the stamina amount to refill.
                        return determineBaseStamina();
                    }
                    return orig;
                });
            }
        }

        /// <summary>
        /// Replaces the RefillStamina in the base game.
        /// </summary>
        /// <param name="orig">The original RefillStamina method</param>
        /// <param name="self">The Player instance</param>
        private void modRefillStamina(On.Celeste.Player.orig_RefillStamina orig, Player self) {
            if (Settings.DontRefillStaminaOnGround && !forceRefillStamina) {
                // we don't want to refill stamina at all.
                return;
            }

            // invoking the original method is not really useful, but another mod may try to hook it, so don't break it if the Stamina variant is disabled
            orig.Invoke(self);

            if (Settings.Stamina != 11) {
                self.Stamina = determineBaseStamina();
            }
        }

        // transitioning and spawning are the 2 conditions when we **want** to refill stamina no matter what.

        private void modOnTransition(On.Celeste.Player.orig_OnTransition orig, Player self) {
            forceRefillStamina = true;
            orig(self);
            forceRefillStamina = false;
        }

        private void modPlayerConstructor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            forceRefillStamina = true;
            orig(self, position, spriteMode);
            forceRefillStamina = false;
        }

        private bool modPlayerUseRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes) {
            forceRefillStamina = true;
            bool result = orig(self, twoDashes);
            forceRefillStamina = false;
            return result;
        }

        /// <summary>
        /// Returns the max stamina.
        /// </summary>
        /// <returns>The max stamina (default 110)</returns>
        private float determineBaseStamina() {
            return Settings.Stamina * 10f;
        }
    }
}
