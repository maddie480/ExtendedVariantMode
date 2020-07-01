using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;

namespace ExtendedVariants.Variants {
    public class DashSpeed : AbstractExtendedVariant {
        private static ILHook dashCoroutineHook;
        private static ILHook redDashCoroutineHook;

        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.DashSpeed;
        }

        public override void SetValue(int value) {
            Settings.DashSpeed = value;
        }

        public override void Load() {
            dashCoroutineHook = ExtendedVariantsModule.HookCoroutine("Celeste.Player", "DashCoroutine", modDashSpeed);
            redDashCoroutineHook = ExtendedVariantsModule.HookCoroutine("Celeste.Player", "RedDashCoroutine", modDashSpeed);
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
            return Settings.DashSpeed / 10f;
        }
    }
}
