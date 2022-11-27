using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class HorizontalWallJumpDuration : AbstractExtendedVariant {
        private static ILHook hookPlayerOrigWallJump = null;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetVariantValue() {
            return Settings.HorizontalWallJumpDuration;
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.HorizontalWallJumpDuration = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.HorizontalWallJumpDuration = value / 10f;
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
                cursor.EmitDelegate<Func<float, float>>(orig => orig * Settings.HorizontalWallJumpDuration);
                cursor.Index++;
            }
        }
    }
}
