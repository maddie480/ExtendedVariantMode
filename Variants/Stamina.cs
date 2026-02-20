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
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class Stamina : AbstractExtendedVariant {

        private static ILHook playerUpdateHook;
        private static ILHook summitGemSmashRoutineHook;

        private static ILHook badelineBoostRoutineHook;
        private static ILHook boostBeginHook;
        private static ILHook starFlyRoutineHook;

        private static bool forceRefillStamina;

        public Stamina() : base(variantType: typeof(int), defaultVariantValue: 110) { }

        public override object ConvertLegacyVariantValue(int value) {
            // "type 15 to get 150 stamina", of course. :p
            return value * 10;
        }

        public override void Load() {
            IL.Celeste.Player.ClimbUpdate += patchOutStamina;
            IL.Celeste.Player.SwimBegin += patchOutStamina;
            IL.Celeste.Player.DreamDashBegin += patchOutStamina;
            IL.Celeste.Player.ctor += patchOutStamina;

            On.Celeste.Player.OnTransition += modOnTransition;
            On.Celeste.Player.ctor += modPlayerConstructor;
            On.Celeste.Player.UseRefill += modPlayerUseRefill;

            IL.Celeste.Cassette.OnPlayer += wrapRefillStamina;
            IL.Celeste.Player.OnTransition += wrapRefillStamina;
            IL.Celeste.Player.Bounce += wrapRefillStamina;
            IL.Celeste.Player.SuperBounce += wrapRefillStamina;
            IL.Celeste.Player.SideBounce += wrapRefillStamina;
            IL.Celeste.Player.UseRefill += wrapRefillStamina;
            IL.Celeste.Player.PointBounce += wrapRefillStamina;
            IL.Celeste.Player.BoostBegin += wrapRefillStamina;
            IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += wrapRefillStamina;
            IL.Celeste.Player.FinalBossPushLaunch += wrapRefillStamina;
            IL.Celeste.Player.BadelineBoostLaunch += wrapRefillStamina;
            IL.Celeste.Player.DreamDashEnd += wrapRefillStamina;
            IL.Celeste.Player.StartStarFly += wrapRefillStamina;
            IL.Celeste.Player.FlingBirdBegin += wrapRefillStamina;

            playerUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), patchOutStamina);
            summitGemSmashRoutineHook = new ILHook(typeof(SummitGem).GetMethod("SmashRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), patchOutStamina);

            badelineBoostRoutineHook = new ILHook(typeof(BadelineBoost).GetMethod("BoostRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), wrapRefillStamina);
            boostBeginHook = new ILHook(typeof(Player).GetMethod("orig_BoostBegin", BindingFlags.NonPublic | BindingFlags.Instance), wrapRefillStamina);
            starFlyRoutineHook = new ILHook(typeof(Player).GetMethod("StarFlyCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), wrapRefillStamina);
        }

        public override void Unload() {
            IL.Celeste.Player.ClimbUpdate -= patchOutStamina;
            IL.Celeste.Player.SwimBegin -= patchOutStamina;
            IL.Celeste.Player.DreamDashBegin -= patchOutStamina;
            IL.Celeste.Player.ctor -= patchOutStamina;

            On.Celeste.Player.OnTransition -= modOnTransition;
            On.Celeste.Player.ctor -= modPlayerConstructor;
            On.Celeste.Player.UseRefill -= modPlayerUseRefill;

            IL.Celeste.Cassette.OnPlayer -= wrapRefillStamina;
            IL.Celeste.Player.OnTransition -= wrapRefillStamina;
            IL.Celeste.Player.Bounce -= wrapRefillStamina;
            IL.Celeste.Player.SuperBounce -= wrapRefillStamina;
            IL.Celeste.Player.SideBounce -= wrapRefillStamina;
            IL.Celeste.Player.UseRefill -= wrapRefillStamina;
            IL.Celeste.Player.PointBounce -= wrapRefillStamina;
            IL.Celeste.Player.BoostBegin -= wrapRefillStamina;
            IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= wrapRefillStamina;
            IL.Celeste.Player.FinalBossPushLaunch -= wrapRefillStamina;
            IL.Celeste.Player.BadelineBoostLaunch -= wrapRefillStamina;
            IL.Celeste.Player.DreamDashEnd -= wrapRefillStamina;
            IL.Celeste.Player.StartStarFly -= wrapRefillStamina;
            IL.Celeste.Player.FlingBirdBegin -= wrapRefillStamina;

            if (playerUpdateHook != null) playerUpdateHook.Dispose();
            if (summitGemSmashRoutineHook != null) summitGemSmashRoutineHook.Dispose();

            if (badelineBoostRoutineHook != null) badelineBoostRoutineHook.Dispose();
            if (boostBeginHook != null) boostBeginHook.Dispose();
            if (starFlyRoutineHook != null) starFlyRoutineHook.Dispose();
        }


        /// <summary>
        /// Replaces the default 110 stamina value with the one defined in the settings.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private static void patchOutStamina(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            // now, patch everything stamina-related (every instance of 110)
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(110f))) {
                Logger.Log("ExtendedVariantMode/Stamina", $"Patching stamina at index {cursor.Index} in CIL code for {cursor.Method.FullName}");

                cursor.EmitDelegate<Func<float, float>>(modStaminaAmount);
            }
        }
        private static float modStaminaAmount(float orig) {
            if (GetVariantValue<bool>(Variant.DontRefillStaminaOnGround) && !SaveData.Instance.Assists.InfiniteStamina && !forceRefillStamina) {
                float playerStamina = Engine.Scene.Tracker.GetEntity<Player>()?.Stamina ?? determineBaseStamina();

                // don't prevent refilling stamina on ground if the player has *too much* stamina.
                if (playerStamina <= GetVariantValue<int>(Variant.Stamina)) {
                    // return the player stamina: this will result in player.Stamina = player.Stamina, thus doing absolutely nothing.
                    return playerStamina;
                }
            }
            if (GetVariantValue<int>(Variant.Stamina) != 110) {
                // mod the stamina amount to refill.
                return determineBaseStamina();
            }
            return orig;
        }

        private static void wrapRefillStamina(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(instr => instr.MatchCallvirt<Player>("RefillStamina"))) {
                Logger.Log("ExtendedVariantMode/Stamina", $"Wrapping call to RefillStamina at index {cursor.Index} in CIL code for {cursor.Method.FullName}");

                // if (!shouldSkipRefillStamina()) {
                //     player.RefillStamina();
                //     afterRefillStamina(player);
                // }
                cursor.Emit(OpCodes.Dup);
                cursor.EmitDelegate(shouldSkipRefillStamina);
                cursor.Emit(OpCodes.Brfalse, cursor.Next);
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Br, cursor.Next.Next);
                cursor.Index++;
                cursor.EmitDelegate(afterRefillStamina);
            }
        }

        private static bool shouldSkipRefillStamina() {
            return GetVariantValue<bool>(Variant.DontRefillStaminaOnGround) && !SaveData.Instance.Assists.InfiniteStamina && !forceRefillStamina;
        }

        private static void afterRefillStamina(Player self) {
            if (GetVariantValue<int>(Variant.Stamina) != 110) {
                self.Stamina = determineBaseStamina();
            }
        }

        // transitioning, spawning and using refills are the 3 conditions when we **want** to refill stamina no matter what.

        private static void modOnTransition(On.Celeste.Player.orig_OnTransition orig, Player self) {
            forceRefillStamina = true;
            orig(self);
            forceRefillStamina = false;
        }

        private static void modPlayerConstructor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            forceRefillStamina = true;
            orig(self, position, spriteMode);
            forceRefillStamina = false;
        }

        private static bool modPlayerUseRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes) {
            forceRefillStamina = true;
            bool result = orig(self, twoDashes);
            forceRefillStamina = false;
            return result;
        }

        /// <summary>
        /// Returns the max stamina.
        /// </summary>
        /// <returns>The max stamina (default 110)</returns>
        private static float determineBaseStamina() {
            return GetVariantValue<int>(Variant.Stamina);
        }
    }
}
