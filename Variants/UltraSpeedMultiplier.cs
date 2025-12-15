using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class UltraSpeedMultiplier : AbstractExtendedVariant {
        private static ILHook dashCoroutineHookForTimer;

        public UltraSpeedMultiplier() : base(variantType: typeof(float), defaultVariantValue: 1.2f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
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

        private static void modUltraBoosts(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1.2f))) {
                Logger.Log("ExtendedVariantMode/UltraSpeedMultiplier", $"Modifying ultra speed multiplier at {cursor.Index} in IL for Player.OnCollideV");
                cursor.EmitDelegate<Func<float, float>>(modUltraSpeed);
            }
        }
        private static float modUltraSpeed(float orig) {
            return GetVariantValue<float>(Variant.UltraSpeedMultiplier) == 1.2f ? orig : GetVariantValue<float>(Variant.UltraSpeedMultiplier);
        }
    }
}
