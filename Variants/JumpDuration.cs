using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class JumpDuration : AbstractExtendedVariant {
        private static ILHook hookPlayerOrigWallJump = null;

        public JumpDuration() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Player.BeforeUpTransition += modVarJumpTimer;
            IL.Celeste.Player.HiccupJump += modVarJumpTimer;
            IL.Celeste.Player.Jump += modVarJumpTimer;
            IL.Celeste.Player.SuperJump += modVarJumpTimer;
            IL.Celeste.Player.SuperWallJump += modVarJumpTimer;
            IL.Celeste.Player.Bounce += modVarJumpTimer;
            IL.Celeste.Player.SuperBounce += modVarJumpTimer;
            IL.Celeste.Player.SideBounce += modVarJumpTimer;
            IL.Celeste.Player.Rebound += modVarJumpTimer;
            IL.Celeste.Player.StarFlyUpdate += modVarJumpTimer;
            IL.Celeste.Player.FinishFlingBird += modVarJumpTimer;

            hookPlayerOrigWallJump = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.NonPublic | BindingFlags.Instance), modVarJumpTimer);
        }

        public override void Unload() {
            IL.Celeste.Player.BeforeUpTransition -= modVarJumpTimer;
            IL.Celeste.Player.HiccupJump -= modVarJumpTimer;
            IL.Celeste.Player.Jump -= modVarJumpTimer;
            IL.Celeste.Player.SuperJump -= modVarJumpTimer;
            IL.Celeste.Player.SuperWallJump -= modVarJumpTimer;
            IL.Celeste.Player.Bounce -= modVarJumpTimer;
            IL.Celeste.Player.SuperBounce -= modVarJumpTimer;
            IL.Celeste.Player.SideBounce -= modVarJumpTimer;
            IL.Celeste.Player.Rebound -= modVarJumpTimer;
            IL.Celeste.Player.StarFlyUpdate -= modVarJumpTimer;
            IL.Celeste.Player.FinishFlingBird -= modVarJumpTimer;

            hookPlayerOrigWallJump?.Dispose();
            hookPlayerOrigWallJump = null;
        }

        private void modVarJumpTimer(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("varJumpTimer"))) {
                Logger.Log("ExtendedVariantMode/JumpDuration", $"Modding varJumpTimer at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<float, float>>(orig => orig * GetVariantValue<float>(Variant.JumpDuration));
                cursor.Index++;
            }
        }
    }
}
