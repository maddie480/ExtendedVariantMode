using System;
using System.Collections;
using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;

namespace ExtendedVariants.Variants {
    public class SaferDiagonalSmuggle : AbstractExtendedVariant {
        private const float ADD_DASH_ATTACK = 0.033f;
        private const float MIN_DASH_X = 169.705627f;
        private const float COEFF = 11.6568542495f;

        public override void Load() => On.Celeste.Player.PickupCoroutine += Player_PickupCoroutine;

        public override void Unload() => On.Celeste.Player.PickupCoroutine -= Player_PickupCoroutine;

        public SaferDiagonalSmuggle() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) => value != 0;

        private static IEnumerator Player_PickupCoroutine(On.Celeste.Player.orig_PickupCoroutine pickupCoroutine, Player player) {
            if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.SaferDiagonalSmuggle)
                && player.StateMachine.PreviousState == Player.StDash && player.DashDir.X != 0f && player.DashDir.Y < 0f && player.dashAttackTimer > 0f && Math.Abs(player.Speed.X) < 240f) {
                float a = 1f - Math.Max(MIN_DASH_X, Math.Abs(player.Speed.X)) / 240f;

                player.dashAttackTimer += a * a * COEFF * ADD_DASH_ATTACK;
            }

            yield return new SwapImmediately(pickupCoroutine(player));
        }
    }
}