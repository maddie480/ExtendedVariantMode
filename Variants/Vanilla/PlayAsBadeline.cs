using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Monocle;
using System;

namespace ExtendedVariants.Variants.Vanilla
{
    public class PlayAsBadeline : AbstractVanillaVariant
    {
        public override Type GetVariantType()
        {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue()
        {
            return false;
        }

        public override object ConvertLegacyVariantValue(int value)
        {
            return value != 0;
        }

        public static void ChangePlayerSprite(Player player, bool playAsBadeline)
        {
            PlayerSpriteMode mode = playAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode;
            if (player.Active)
            {
                player.ResetSpriteNextFrame(mode);
            }
            else
            {
                player.ResetSprite(mode);
            }
        }

        public override void VariantValueChanged()
        {
            bool playAsBadeline = getActiveAssistValues().PlayAsBadeline;
            Player player = Engine.Scene?.Tracker.GetEntity<Player>();

            if (player != null)
            {
                ChangePlayerSprite(player, playAsBadeline);
            }
        }

        protected override Assists applyVariantValue(Assists target, object value)
        {
            target.PlayAsBadeline = (bool)value;
            return target;
        }


        public override void Load()
        {
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
        }

        public override void Unload()
        {
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
        }

        private static void Player_OnSpawn(Player player)
        {
            bool playAsBadeline = (bool)ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.PlayAsBadeline);
            ChangePlayerSprite(player, playAsBadeline);
        }

    }
}
