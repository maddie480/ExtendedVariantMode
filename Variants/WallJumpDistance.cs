using Celeste.Mod;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class WallJumpDistance : AbstractExtendedVariant {

        public WallJumpDistance() : base(variantType: typeof(int), defaultVariantValue: 3) { }

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
            while (cursor.TryGotoNext(instr => instr.MatchLdcI4(3), instr => instr.MatchStloc(0))) {
                cursor.Index++;
                Logger.Log("ExtendedVariantMode/WallJumpDistance", $"Modding wall jump distance at {cursor.Index} in IL for Player.WallJumpCheck");
                cursor.EmitDelegate<Func<int, int>>(getVariantValue);
            }
        }
        private static int getVariantValue(int orig) {
            if (GetVariantValue<int>(Variant.WallJumpDistance) == 3) return orig;
            return GetVariantValue<int>(Variant.WallJumpDistance);
        }
    }
}
