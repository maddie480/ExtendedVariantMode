using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/SpinnerColorTrigger")]
    public class SpinnerColorTrigger : AbstractExtendedVariantTrigger<SpinnerColor.Color> {
        public SpinnerColorTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.SpinnerColor;
        }

        protected override SpinnerColor.Color getNewValue(EntityData data) {
            return data.Enum("newValue", SpinnerColor.Color.Default);
        }
    }
}
