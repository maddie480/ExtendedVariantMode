using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/ColorGradeTrigger")]
    public class ColorGradeTrigger : Trigger {
        private string colorGrade;
        private bool revertOnLeave;
        private bool revertOnDeath;
        private bool onlyOnce;

        public ColorGradeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            colorGrade = data.Attr("colorGrade", "none");
            revertOnLeave = data.Bool("revertOnLeave", false);
            revertOnDeath = data.Bool("revertOnDeath", true);
            onlyOnce = data.Bool("onlyOnce", false);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(ExtendedVariantsModule.Variant.ColorGrading, colorGrade, revertOnLeave, isFade: false, revertOnDeath, legacy: false);

            if (onlyOnce) {
                RemoveSelf();
            }
        }
    }
}
