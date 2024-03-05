using System;
using System.Collections;
using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using MonoMod.Utils;

namespace ExtendedVariants.Variants;

public class SaferDiagonalSmuggles : AbstractExtendedVariant {
    private const float ADD_DASH_ATTACK = 0.0167f;

    public override void Load() => On.Celeste.Player.PickupCoroutine += Player_PickupCoroutine;

    public override void Unload() => On.Celeste.Player.PickupCoroutine -= Player_PickupCoroutine;

    public override Type GetVariantType() => typeof(bool);

    public override object GetDefaultVariantValue() => false;

    public override object ConvertLegacyVariantValue(int value) => value != 0;

    private IEnumerator Player_PickupCoroutine(On.Celeste.Player.orig_PickupCoroutine pickupCoroutine, Player player) {
        if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.SaferDiagonalSmuggles) && player.DashDir.X != 0f && player.DashDir.Y < 0f) {
            var dynamicData = DynamicData.For(player);
            float dashAttackTimer = dynamicData.Get<float>("dashAttackTimer");

            if (dashAttackTimer > 0f)
                dynamicData.Set("dashAttackTimer", dashAttackTimer + ADD_DASH_ATTACK);
        }

        yield return new SwapImmediately(pickupCoroutine(player));
    }
}