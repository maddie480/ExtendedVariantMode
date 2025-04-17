using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class DashSpeed : AbstractExtendedVariant {
        private static ILHook dashCoroutineHook;
        private static ILHook redDashCoroutineHook;

        public DashSpeed() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            dashCoroutineHook = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modDashSpeed);
            redDashCoroutineHook = new ILHook(typeof(Player).GetMethod("RedDashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modDashSpeed);
        }

        public override void Unload() {
            dashCoroutineHook?.Dispose();
            redDashCoroutineHook?.Dispose();

            dashCoroutineHook = null;
            redDashCoroutineHook = null;
        }

        private void modDashSpeed(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // find 240f in the method (dash speed) and multiply it with our modifier.
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(240f))) {
                Logger.Log("ExtendedVariantMode/DashSpeed", $"Modding dash speed at index {cursor.Index} in CIL code for {cursor.Method.Name}");

                cursor.EmitDelegate<Func<float>>(getDashSpeedMultiplier);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private float getDashSpeedMultiplier() {
            return GetVariantValue<float>(Variant.DashSpeed);
        }
    }
}
