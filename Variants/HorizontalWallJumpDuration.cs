using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class HorizontalWallJumpDuration : AbstractExtendedVariant {
        private static ILHook hookPlayerOrigWallJump = null;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            hookPlayerOrigWallJump = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.NonPublic | BindingFlags.Instance), modForceMoveXTimer);
        }

        public override void Unload() {
            hookPlayerOrigWallJump?.Dispose();
            hookPlayerOrigWallJump = null;
        }

        private void modForceMoveXTimer(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("forceMoveXTimer"))) {
                Logger.Log("ExtendedVariantMode/HorizontalWallJumpDuration", $"Modding forceMoveXTimer at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<float, float>>(orig => orig * GetVariantValue<float>(Variant.HorizontalWallJumpDuration));
                cursor.Index++;
            }
        }
    }
}
