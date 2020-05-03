using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants {
    [CustomEntity("ExtendedVariantMode/ColorGradeTrigger")]
    class ColorGradeTrigger : Trigger {
        private string colorGrade;

        public ColorGradeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            colorGrade = data.Attr("colorGrade", "none");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            (ExtendedVariantsModule.Instance.VariantHandlers[ExtendedVariantsModule.Variant.ColorGrading] as ColorGrading)
                .SetColorGrade(colorGrade);
        }
    }
}
