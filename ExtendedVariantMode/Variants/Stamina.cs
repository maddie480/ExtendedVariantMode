using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;

namespace ExtendedVariants.Variants {
    public class Stamina : AbstractExtendedVariant {

        private ILHook playerUpdateHook;
        private ILHook summitGemSmashRoutineHook;

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

            playerUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), patchOutStamina);
            summitGemSmashRoutineHook = ExtendedVariantsModule.HookCoroutine("Celeste.SummitGem", "SmashRoutine", patchOutStamina);
        }

        public override void Unload() {
            IL.Celeste.Player.ClimbUpdate -= patchOutStamina;
            IL.Celeste.Player.SwimBegin -= patchOutStamina;
            IL.Celeste.Player.DreamDashBegin -= patchOutStamina;
            IL.Celeste.Player.ctor -= patchOutStamina;
            On.Celeste.Player.RefillStamina -= modRefillStamina;

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

                // pop the 110 and call our method instead
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate<Func<float>>(determineBaseStamina);
            }
        }

        /// <summary>
        /// Replaces the RefillStamina in the base game.
        /// </summary>
        /// <param name="orig">The original RefillStamina method</param>
        /// <param name="self">The Player instance</param>
        private void modRefillStamina(On.Celeste.Player.orig_RefillStamina orig, Player self) {
            // invoking the original method is not really useful, but another mod may try to hook it, so don't break it if the Stamina variant is disabled
            orig.Invoke(self);

            if (Settings.Stamina != 11) {
                self.Stamina = determineBaseStamina();
            }
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
