using System;
using System.Collections.Generic;
using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using Platform = Celeste.Platform;

namespace ExtendedVariants.Variants {
    public class UltraProtection : AbstractExtendedVariant {
        private static void PlayLandingEffects(Player player) {
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);

            var dynamicData = DynamicData.For(player);
            var platform = SurfaceIndex.GetPlatformByPriority(player.CollideAll<Platform>(player.Position + Vector2.UnitY, dynamicData.Get<List<Entity>>("temp")));
            int surfaceIndex = -1;

            if (platform != null) {
                surfaceIndex = platform.GetLandSoundIndex(player);

                if (surfaceIndex >= 0 && !player.MuffleLanding)
                    player.Play($"{SurfaceIndex.GetPathFromIndex(surfaceIndex)}/landing", "surface_index", surfaceIndex);

                if (platform is DreamBlock dreamBlock)
                    dreamBlock.FootstepRipple(player.Position);

                player.MuffleLanding = false;
            }

            if (player.Speed.Y >= 80.0)
                Dust.Burst(player.Position, (-Vector2.UnitY).Angle(), 8, dynamicData.Invoke<ParticleType>("DustParticleFromSurfaceIndex", surfaceIndex));
        }

        public override void Load() {
            On.Celeste.Player.Jump += Player_Jump;
            On.Celeste.Player.DashBegin += Player_DashBegin;
        }

        public override void Unload() {
            On.Celeste.Player.Jump -= Player_Jump;
            On.Celeste.Player.DashBegin -= Player_DashBegin;
        }

        public UltraProtection() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) => value != 0;

        private void Player_Jump(On.Celeste.Player.orig_Jump jump, Player player, bool particles, bool playsfx) {
            if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.UltraProtection)
                && player.DashDir.X != 0f && player.DashDir.Y > 0f && player.Speed.Y > 0f
                && DynamicData.For(player).Get<bool>("onGround")) {
                player.DashDir.X = Math.Sign(player.DashDir.X);
                player.DashDir.Y = 0f;
                player.Speed.X *= GetVariantValue<float>(ExtendedVariantsModule.Variant.UltraSpeedMultiplier);
                PlayLandingEffects(player);
            }

            jump(player, particles, playsfx);
        }

        private void Player_DashBegin(On.Celeste.Player.orig_DashBegin dashBegin, Player player) {
            if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.UltraProtection)
                && player.DashDir.X != 0f && player.DashDir.Y > 0f && player.Speed.Y > 0f
                && DynamicData.For(player).Get<bool>("onGround")) {
                player.Speed.X *= GetVariantValue<float>(ExtendedVariantsModule.Variant.UltraSpeedMultiplier);
                PlayLandingEffects(player);
            }

            dashBegin(player);
        }
    }
}