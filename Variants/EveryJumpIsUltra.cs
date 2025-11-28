using Celeste;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class EveryJumpIsUltra : AbstractExtendedVariant {

        public EveryJumpIsUltra() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Player.Jump += modJump;
            On.Celeste.Player.ClimbJump += modClimbJump;
        }

        public override void Unload() {
            On.Celeste.Player.Jump -= modJump;
            On.Celeste.Player.ClimbJump -= modClimbJump;
        }

        private static void modJump(On.Celeste.Player.orig_Jump orig, Celeste.Player self, bool particles, bool playSfx) {
            orig(self, particles, playSfx);
            forceUltra(self);
        }

        private static void modClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            orig(self);
            forceUltra(self);
        }

        private static void forceUltra(Player self) {
            if (GetVariantValue<bool>(Variant.EveryJumpIsUltra)) {
                // I don't know what an ultra is, I'm a beginner-intermediate player, so I hope this is the piece of code I was supposed to copy to replicate one. ~ Maddie
                self.DashDir.X = Math.Sign(self.DashDir.X);
                self.DashDir.Y = 0f;
                // self.Speed.Y = 0f; // this seems a bit counter-productive when applied after a jump...
                self.Speed.X *= GetVariantValue<float>(Variant.UltraSpeedMultiplier);
                self.Ducking = true;
            }
        }
    }
}
