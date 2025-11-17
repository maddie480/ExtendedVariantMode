using Celeste.Mod;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class WallBounceDistance : AbstractExtendedVariant {

        public WallBounceDistance() : base(variantType: typeof(int), defaultVariantValue: 5) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }

        public override void Load() {
            IL.Celeste.Player.WallJumpCheck += onPlayerWallJumpCheck;
        }

        public override void Unload() {
            IL.Celeste.Player.WallJumpCheck -= onPlayerWallJumpCheck;
        }

        private static void onPlayerWallJumpCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(instr => instr.MatchLdcI4(5), instr => instr.MatchStloc(0))) {
                cursor.Index++;
                Logger.Log("ExtendedVariantMode/WallBounceDistance", $"Modding wall bounce distance (int) at {cursor.Index} in IL for Player.WallJumpCheck");
                cursor.EmitDelegate<Func<int, int>>(getVariantValueInt);
            }

            cursor.Index = 0;
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(5f))) {
                Logger.Log("ExtendedVariantMode/WallBounceDistance", $"Modding wall bounce distance (float) at {cursor.Index} in IL for Player.WallJumpCheck");
                cursor.EmitDelegate<Func<float, float>>(getVariantValueFloat);
            }
        }
        private static int getVariantValueInt(int orig) {
            if (GetVariantValue<int>(Variant.WallBounceDistance) == 5) return orig;
            return GetVariantValue<int>(Variant.WallBounceDistance);
        }
        private static float getVariantValueFloat(float orig) {
            if (GetVariantValue<int>(Variant.WallBounceDistance) == 5) return orig;
            return GetVariantValue<int>(Variant.WallBounceDistance);
        }
    }
}
