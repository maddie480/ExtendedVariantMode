using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class UltraSpeedMultiplier : AbstractExtendedVariant {
        private ILHook dashCoroutineHookForTimer;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetVariantValue() {
            return Settings.UltraSpeedMultiplier;
        }

        public override object GetDefaultVariantValue() {
            return 1.2f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.UltraSpeedMultiplier = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.UltraSpeedMultiplier = value / 10f;
        }

        public override void Load() {
            dashCoroutineHookForTimer = new ILHook(
                typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modUltraBoosts);

            IL.Celeste.Player.OnCollideV += modUltraBoosts;
        }

        public override void Unload() {
            dashCoroutineHookForTimer?.Dispose();
            dashCoroutineHookForTimer = null;

            IL.Celeste.Player.OnCollideV -= modUltraBoosts;
        }

        private void modUltraBoosts(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1.2f))) {
                Logger.Log("ExtendedVariantMode/UltraSpeedMultiplier", $"Modifying ultra speed multiplier at {cursor.Index} in IL for Player.OnCollideV");
                cursor.EmitDelegate<Func<float, float>>(orig => Settings.UltraSpeedMultiplier == 1.2f ? orig : Settings.UltraSpeedMultiplier);
            }
        }
    }
}
