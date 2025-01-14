using System;
using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Monocle;

namespace ExtendedVariants.Variants
{
    internal class DashRefillCooldown : AbstractExtendedVariant
    {
        public override Type GetVariantType()
        {
            return typeof(float);
        }

        public override object GetDefaultVariantValue()
        {
            return 0.1f;
        }

        public override object ConvertLegacyVariantValue(int value)
        {
            return value / 10f;
        }

        public override void Load()
        {
            On.Celeste.Player.DashBegin += Player_DashBegin;
            On.Celeste.Player.RedDashBegin += Player_RedDashBegin;
        }

        public override void Unload()
        {
            On.Celeste.Player.DashBegin -= Player_DashBegin;
            On.Celeste.Player.RedDashBegin -= Player_RedDashBegin;
        }

        private void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
        {
            orig(self);

            if (GetVariantValue<float>(ExtendedVariantsModule.Variant.DashRefillCooldown) == 0.1f)
                return;

            DynamicData selfData = DynamicData.For(self);
            selfData.Set("dashRefillCooldownTimer", GetVariantValue<float>(ExtendedVariantsModule.Variant.DashRefillCooldown));
        }

        private void Player_RedDashBegin(On.Celeste.Player.orig_RedDashBegin orig, Player self)
        {
            orig(self);

            if (GetVariantValue<float>(ExtendedVariantsModule.Variant.DashRefillCooldown) == 0.1f)
                return;

            DynamicData selfData = DynamicData.For(self);
            selfData.Set("dashRefillCooldownTimer", GetVariantValue<float>(ExtendedVariantsModule.Variant.DashRefillCooldown));
        }
    }
}
