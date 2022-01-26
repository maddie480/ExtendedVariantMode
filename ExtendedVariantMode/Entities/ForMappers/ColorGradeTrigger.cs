using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/ColorGradeTrigger")]
    public class ColorGradeTrigger : AbstractExtendedVariantTrigger<string> {
        public ColorGradeTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.ColorGrading;
        }

        protected override string getNewValue(EntityData data) {
            return data.Attr("colorGrade", "none");
        }
    }
}
