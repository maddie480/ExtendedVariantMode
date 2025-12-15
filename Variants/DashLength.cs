using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class DashLength : AbstractExtendedVariant {

        private static ILHook dashCoroutineHookForTimer;
        private static ILHook dashCoroutineHookForCounter;

        public DashLength() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
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

        private static void modDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self) {
            orig(self);

            if (GetVariantValue<float>(Variant.DashLength) != 1f) {
                bool superDash = SaveData.Instance.Assists.SuperDashing;

                // vanilla dash is 0.15, 0.3 with superdash

                // vanilla is 0.3, 0.45 with superdash => in 2x, you get 0.45, 0.75 with superdash
                self.dashAttackTimer = (superDash ? 0.3f : 0.15f) * GetVariantValue<float>(Variant.DashLength) + 0.15f;

                // vanilla is 0.55 all the time => in 2x, you get 0.7, 0.85 with superdash
                self.gliderBoostTimer = (superDash ? 0.3f : 0.15f) * GetVariantValue<float>(Variant.DashLength) + (superDash ? 0.25f : 0.4f);
            }

            if (GetVariantValue<float>(Variant.DashTimerMultiplier) != 1f) {
                self.dashAttackTimer = GetVariantValue<float>(Variant.DashTimerMultiplier) * self.dashAttackTimer;
            }
        }

        private static void modDashLength(ILContext il) {
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
        private static float determineDashLengthFactor() {
            return GetVariantValue<float>(Variant.DashLength);
        }

        private static void modDashTrailCounter(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump wherever dashTrailCounter is saved
            while (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("dashTrailCounter"))) {
                Logger.Log("ExtendedVariantMode/DashLength", $"Modding dash trail counter at {cursor.Index} in CIL code for {cursor.Method.FullName}");

                cursor.EmitDelegate<Func<int, int>>(applyDashTrailCounter);

                // don't forget to move forward in order to avoid infinite loops!
                cursor.Index++;
            }
        }

        private static int applyDashTrailCounter(int dashTrailCounter) {
            if (GetVariantValue<float>(Variant.DashLength) != 1f) {
                float lastDashDuration = SaveData.Instance.Assists.SuperDashing ? 0.3f : 0.15f;
                return (int) Math.Round(lastDashDuration * GetVariantValue<float>(Variant.DashLength)) - 1;
            }
            return dashTrailCounter;
        }
    }
}
