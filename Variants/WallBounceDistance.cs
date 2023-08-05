using Celeste.Mod;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class WallBounceDistance : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetDefaultVariantValue() {
            return 5;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }

        public override void Load() {
            IL.Celeste.Player.WallJumpCheck += onPlayerWallJumpCheck;
        }

        public override void Unload() {
            IL.Celeste.Player.WallJumpCheck -= onPlayerWallJumpCheck;
        }

        private void onPlayerWallJumpCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(instr => instr.MatchLdcI4(5), instr => instr.MatchStloc(0))) {
                cursor.Index++;
                Logger.Log("ExtendedVariantMode/WallBounceDistance", $"Modding wall bounce distance (int) at {cursor.Index} in IL for Player.WallJumpCheck");
                cursor.EmitDelegate<Func<int, int>>(orig => GetVariantValue<int>(Variant.WallBounceDistance));
            }

            cursor.Index = 0;
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(5f))) {
                Logger.Log("ExtendedVariantMode/WallBounceDistance", $"Modding wall bounce distance (float) at {cursor.Index} in IL for Player.WallJumpCheck");
                cursor.EmitDelegate<Func<float, float>>(orig => GetVariantValue<int>(Variant.WallBounceDistance));
            }
        }
    }
}
