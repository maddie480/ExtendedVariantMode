using Monocle;
using System;

namespace ExtendedVariants.Variants {
    public class JumpCooldown : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 0f;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            On.Celeste.Player.Update += onPlayerUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.Update -= onPlayerUpdate;
        }

        private float jumpCooldownTimer = 0f;

        public override void VariantValueChanged() {
            // make sure the timer does not exceed the new jump cooldown cap
            jumpCooldownTimer = Math.Min(jumpCooldownTimer, GetVariantValue<float>(Module.ExtendedVariantsModule.Variant.JumpCooldown));
        }

        public void ArmCooldown() {
            jumpCooldownTimer = GetVariantValue<float>(Module.ExtendedVariantsModule.Variant.JumpCooldown);
        }

        public bool CheckCooldown() {
            return jumpCooldownTimer > 0f;
        }

        private void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Celeste.Player self) {
            orig(self);
            if (jumpCooldownTimer > 0f) jumpCooldownTimer -= Engine.DeltaTime;
        }
    }
}
