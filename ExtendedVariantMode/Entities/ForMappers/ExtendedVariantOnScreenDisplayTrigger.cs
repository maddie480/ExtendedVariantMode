using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/ExtendedVariantOnScreenDisplayTrigger")]
    public class ExtendedVariantOnScreenDisplayTrigger : Trigger {
        private readonly bool enable;

        public ExtendedVariantOnScreenDisplayTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            enable = data.Bool("enable");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            ExtendedVariantsModule.Session.ExtendedVariantsDisplayedOnScreenViaTrigger = enable;
        }
    }
}
