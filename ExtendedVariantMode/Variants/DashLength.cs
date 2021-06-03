using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class DashLength : AbstractExtendedVariant {

        private ILHook dashCoroutineHookForTimer;
        private ILHook dashCoroutineHookForCounter;

        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.DashLength;
        }

        public override void SetValue(int value) {
            Settings.DashLength = value;
        }

        public override void Load() {
            On.Celeste.Player.DashBegin += modDashBegin;

            MethodInfo dashCoroutine = typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
            dashCoroutineHookForTimer = new ILHook(dashCoroutine, modDashLength);
            dashCoroutineHookForCounter = new ILHook(dashCoroutine, modDashTrailCounter);
        }

        public override void Unload() {
            On.Celeste.Player.DashBegin -= modDashBegin;

            if (dashCoroutineHookForTimer != null) dashCoroutineHookForTimer.Dispose();
            if (dashCoroutineHookForCounter != null) dashCoroutineHookForCounter.Dispose();
        }

        private void modDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self) {
            orig(self);

            if (Settings.DashLength != 10) {
                DynData<Player> selfData = new DynData<Player>(self);
                bool superDash = SaveData.Instance.Assists.SuperDashing;

                // vanilla dash is 0.15, 0.3 with superdash

                // vanilla is 0.3, 0.45 with superdash => in 2x, you get 0.45, 0.75 with superdash
                selfData["dashAttackTimer"] = (superDash ? 0.3f : 0.15f) * Settings.DashLength / 10f + 0.15f;

                // vanilla is 0.55 all the time => in 2x, you get 0.7, 0.85 with superdash
                selfData["gliderBoostTimer"] = (superDash ? 0.3f : 0.15f) * Settings.DashLength / 10f + (superDash ? 0.25f : 0.4f);
            }
        }

        private void modDashLength(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump where 0.3 or 0.15f are loaded (those are dash times)
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.3f) || instr.MatchLdcR4(0.15f))) {
                Logger.Log("ExtendedVariantMode/DashLength", $"Applying dash length to constant at {cursor.Index} in CIL code for {cursor.Method.FullName}");

                cursor.EmitDelegate<Func<float>>(determineDashLengthFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Returns the current dash length factor.
        /// </summary>
        /// <returns>The dash length factor (1 = default dash length)</returns>
        private float determineDashLengthFactor() {
            return Settings.DashLength / 10f;
        }

        private void modDashTrailCounter(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump wherever dashTrailCounter is saved
            while (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("dashTrailCounter"))) {
                Logger.Log("ExtendedVariantMode/DashLength", $"Modding dash trail counter at {cursor.Index} in CIL code for {cursor.Method.FullName}");

                cursor.EmitDelegate<Func<int, int>>(applyDashTrailCounter);

                // don't forget to move forward in order to avoid infinite loops!
                cursor.Index++;
            }
        }

        private int applyDashTrailCounter(int dashTrailCounter) {
            if (Settings.DashLength != 10) {
                float lastDashDuration = SaveData.Instance.Assists.SuperDashing ? 0.3f : 0.15f;
                return (int) Math.Round(lastDashDuration * Settings.DashLength) - 1;
            }
            return dashTrailCounter;
        }
    }
}
