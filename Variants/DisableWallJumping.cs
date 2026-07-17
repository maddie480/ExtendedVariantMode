using Celeste;
using MonoMod.RuntimeDetour;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class DisableWallJumping : AbstractExtendedVariant {
        public DisableWallJumping() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            using (new DetourConfigContext(new DetourConfig("ExtendedVariantMode_AfterAll").WithPriority(int.MaxValue)).Use()) {
                On.Celeste.Player.WallJump += onWallJump;
            }
            On.Celeste.Player.WallJumpCheck += modWallJumpCheck;
        }

        public override void Unload() {
            On.Celeste.Player.WallJump -= onWallJump;
            On.Celeste.Player.WallJumpCheck -= modWallJumpCheck;
        }

        private static void onWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
            if (GetVariantValue<bool>(Variant.DisableWallJumping)) return;
            orig(self, dir);
        }

        private static bool modWallJumpCheck(On.Celeste.Player.orig_WallJumpCheck orig, Player self, int dir) {
            if (GetVariantValue<bool>(Variant.DisableWallJumping)) return false;
            return orig(self, dir);
        }
    }
}
