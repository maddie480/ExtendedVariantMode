using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/MaxFallSpeedTrigger")]
    public class MaxFallSpeedTrigger : FloatExtendedVariantTrigger {
        private readonly bool legacy;

        public MaxFallSpeedTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            legacy = data.Bool("legacy");
        }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.FallSpeed;
        }

        public override void OnEnter(Player player) {
            handleLegacy(base.OnEnter, player);
        }

        public override void OnLeave(Player player) {
            handleLegacy(base.OnLeave, player);
        }

        private void handleLegacy(Action<Player> orig, Player player) {
            if (legacy) {
                // make sure activating the variant does not affect maxFall
                DynData<Player> playerData = new DynData<Player>(player);
                float maxFallBackup = playerData.Get<float>("maxFall");
                orig(player);
                playerData["maxFall"] = maxFallBackup;
            } else {
                orig(player);
            }
        }
    }
}
