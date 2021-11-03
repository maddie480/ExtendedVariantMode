using Celeste;
using System;

namespace ExtendedVariants.Variants {
    public class EveryJumpIsUltra : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.EveryJumpIsUltra ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.EveryJumpIsUltra = (value != 0);
        }

        public override void Load() {
            On.Celeste.Player.Jump += modJump;
            On.Celeste.Player.ClimbJump += modClimbJump;
        }

        public override void Unload() {
            On.Celeste.Player.Jump -= modJump;
            On.Celeste.Player.ClimbJump -= modClimbJump;
        }

        private void modJump(On.Celeste.Player.orig_Jump orig, Celeste.Player self, bool particles, bool playSfx) {
            orig(self, particles, playSfx);
            forceUltra(self);
        }

        private void modClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            orig(self);
            forceUltra(self);
        }

        private void forceUltra(Player self) {
            if (Settings.EveryJumpIsUltra) {
                // I don't know what an ultra is, I'm a beginner-intermediate player, so I hope this is the piece of code I was supposed to copy to replicate one. ~ max480
                self.DashDir.X = Math.Sign(self.DashDir.X);
                self.DashDir.Y = 0f;
                // self.Speed.Y = 0f; // this seems a bit counter-productive when applied after a jump...
                self.Speed.X *= 1.2f;
                self.Ducking = true;
            }
        }
    }
}
