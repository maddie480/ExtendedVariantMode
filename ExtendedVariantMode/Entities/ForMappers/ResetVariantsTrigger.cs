using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/ResetVariantsTrigger")]
    public class ResetVariantsTrigger : Trigger {
        private readonly bool vanilla;
        private readonly bool extended;

        public ResetVariantsTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            vanilla = data.Bool("vanilla");
            extended = data.Bool("extended");
        }

        public override void OnEnter(Player player) {
            if (vanilla) {
                ExtendedVariantsModule.Instance.TriggerManager.ResetAllVariantsToDefault(isVanilla: true);
            }

            if (extended) {
                ExtendedVariantsModule.Instance.TriggerManager.ResetAllVariantsToDefault(isVanilla: false);
            }
        }
    }
}
