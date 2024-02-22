using System;
using Celeste;
using ExtendedVariants.Module;
using MonoMod.Utils;

namespace ExtendedVariants.Variants {
    public class UltraProtection : AbstractExtendedVariant {
        public override void Load() {
            On.Celeste.Player.Jump += Player_Jump;
            On.Celeste.Player.DashBegin += Player_DashBegin;
        }

        public override void Unload() {
            On.Celeste.Player.Jump -= Player_Jump;
            On.Celeste.Player.DashBegin -= Player_DashBegin;
        }

        public override Type GetVariantType() => typeof(bool);

        public override object GetDefaultVariantValue() => false;

        public override object ConvertLegacyVariantValue(int value) => value != 0;

        private void Player_Jump(On.Celeste.Player.orig_Jump jump, Player player, bool particles, bool playsfx) {
            if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.UltraProtection)
                && player.DashDir.X != 0f && player.DashDir.Y > 0f && player.Speed.Y > 0f
                && DynamicData.For(player).Get<bool>("onGround")) {
                player.DashDir.X = Math.Sign(player.DashDir.X);
                player.DashDir.Y = 0f;
                player.Speed.X *= 1.2f;
            }

            jump(player, particles, playsfx);
        }

        private void Player_DashBegin(On.Celeste.Player.orig_DashBegin dashBegin, Player player) {
            if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.UltraProtection)
                && player.DashDir.X != 0f && player.DashDir.Y > 0f && player.Speed.Y > 0f
                && DynamicData.For(player).Get<bool>("onGround"))
                player.Speed.X *= 1.2f;

            dashBegin(player);
        }
    }
}