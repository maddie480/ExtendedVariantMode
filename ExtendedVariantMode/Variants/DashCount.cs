using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;

namespace ExtendedVariants.Variants {
    public class DashCount : AbstractExtendedVariant {
        
        /// <summary>
        /// This event is invoked whenever the dash is refilled.
        /// </summary>
        public event Action OnDashRefill;

        public override int GetDefaultValue() {
            return -1;
        }

        public override int GetValue() {
            return Settings.DashCount;
        }

        public override void SetValue(int value) {
            Settings.DashCount = value;
        }

        public override void Load() {
            On.Celeste.Player.RefillDash += modRefillDash;
            IL.Celeste.Player.UseRefill += modUseRefill;
            IL.Celeste.Player.UpdateHair += modUpdateHair;
            On.Celeste.Player.Added += modAdded;
            On.Celeste.BadelineBoost.BoostRoutine += modBadelineBoostRoutine;
        }

        public override void Unload() {
            On.Celeste.Player.RefillDash -= modRefillDash;
            IL.Celeste.Player.UseRefill -= modUseRefill;
            IL.Celeste.Player.UpdateHair -= modUpdateHair;
            On.Celeste.Player.Added -= modAdded;
            On.Celeste.BadelineBoost.BoostRoutine -= modBadelineBoostRoutine;
        }

        /// <summary>
        /// Replaces the RefillDash in the base game.
        /// </summary>
        /// <param name="orig">The original RefillDash method</param>
        /// <param name="self">The Player instance</param>
        private bool modRefillDash(On.Celeste.Player.orig_RefillDash orig, Player self) {
            OnDashRefill?.Invoke();

            if (Settings.DashCount == -1) {
                return orig.Invoke(self);
            } else if(self.Dashes < Settings.DashCount) {
                self.Dashes = Settings.DashCount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Edits the UseRefill method in Player (called when the player gets a refill, obviously.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modUseRefill(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to insert ourselves just before the first stloc.0
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Stloc_0)) {
                Logger.Log("ExtendedVariantsModule", $"Modding dash count given by refills at {cursor.Index} in CIL code for UseRefill");

                // call our method just before storing the result from get_MaxDashes in local variable 0
                cursor.EmitDelegate<Func<int, int>>(determineDashCount);
            }
        }


        /// <summary>
        /// Edits the UpdateHair method in Player (mainly computing the hair color).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modUpdateHair(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // the goal here is to turn "this.Dashes == 2" checks into "this.Dashes >= 2" to make it look less weird
            // and be more consistent with the behaviour of the "Infinite Dashes" variant.
            // (without this patch, with > 2 dashes, Madeline's hair is red, then turns pink, then red again before becoming blue)
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_I4_2 && (instr.Next.OpCode == OpCodes.Bne_Un_S || instr.Next.OpCode == OpCodes.Ceq))) {
                Logger.Log("ExtendedVariantsModule", $"Fixing hair color when having more than 2 dashes by modding a check at {cursor.Index} in CIL code for UpdateHair");

                if (cursor.Next.OpCode == OpCodes.Bne_Un_S) {
                    // XNA version: this is a branch
                    // small trap: the instruction in CIL code actually says "jump if **not** equal to 2". So we set it to "jump if lower than 2" instead
                    cursor.Next.OpCode = OpCodes.Blt_Un_S;
                } else {
                    // FNA version: this is a boolean FOLLOWED by a branch
                    // we're turning this boolean from "Dashes == 2" to "Dashes > 1"
                    cursor.Prev.OpCode = OpCodes.Ldc_I4_1;
                    cursor.Next.OpCode = OpCodes.Cgt;
                }
            }
        }

        /// <summary>
        /// Wraps the Added method in the base game (used to initialize the player state).
        /// </summary>
        /// <param name="orig">The original Added method</param>
        /// <param name="self">The Player instance</param>
        /// <param name="scene">Argument of the original method (passed as is)</param>
        private void modAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig.Invoke(self, scene);
            self.Dashes = determineDashCount(self.Dashes);
        }
        
        private IEnumerator modBadelineBoostRoutine(On.Celeste.BadelineBoost.orig_BoostRoutine orig, BadelineBoost self, Player player) {
            IEnumerator coroutine = orig(self, player);
            while (coroutine.MoveNext()) {
                yield return coroutine.Current;
            }

            // apply the dash refill rules here (this does not call RefillDash)
            if(Settings.DashCount != -1) {
                // an alarm set off in the coroutine will add one more dash in 0.15 seconds
                player.Dashes = Settings.DashCount - 1;
            }

            OnDashRefill?.Invoke();

            yield break;
        }

        /// <summary>
        /// Returns the dash count.
        /// </summary>
        /// <param name="defaultValue">The default value (= Player.MaxDashes)</param>
        /// <returns>The dash count</returns>
        private int determineDashCount(int defaultValue) {
            OnDashRefill?.Invoke(); // if this is called, we are refilling dashes for whatever reason.

            if (Settings.DashCount == -1) {
                return defaultValue;
            }

            return Settings.DashCount;
        }

    }
}
