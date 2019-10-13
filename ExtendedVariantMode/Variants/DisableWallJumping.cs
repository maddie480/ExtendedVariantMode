using Celeste;

namespace ExtendedVariants.Variants {
    public class DisableWallJumping : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.DisableWallJumping ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.DisableWallJumping = (value != 0);
        }

        public override void Load() {
            On.Celeste.Player.WallJump += modWallJump;
            On.Celeste.Player.WallJumpCheck += modWallJumpCheck;
        }

        public override void Unload() {
            On.Celeste.Player.WallJump -= modWallJump;
            On.Celeste.Player.WallJumpCheck -= modWallJumpCheck;
        }
        
        /// <summary>
        /// Detour the WallJump method in order to disable it if we want.
        /// </summary>
        /// <param name="orig">the original method</param>
        /// <param name="self">the player</param>
        /// <param name="dir">the wall jump direction</param>
        private void modWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
            if (!Settings.DisableWallJumping) {
                orig(self, dir);
            }
        }

        /// <summary>
        /// Mods the WallJumpCheck method, checking if a walljump is possible.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self">the player</param>
        /// <param name="dir">the direction</param>
        /// <returns>true if walljumping is possible, false otherwise</returns>
        private bool modWallJumpCheck(On.Celeste.Player.orig_WallJumpCheck orig, Player self, int dir) {
            if(Settings.DisableWallJumping) {
                return false;
            }
            return orig(self, dir);
        }
    }
}
