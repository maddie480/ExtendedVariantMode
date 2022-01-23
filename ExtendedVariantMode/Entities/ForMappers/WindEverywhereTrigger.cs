using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/WindEverywhereTrigger")]
    class WindEverywhereTrigger : AbstractExtendedVariantTrigger<WindEverywhere.WindPattern> {
        public WindEverywhereTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.WindEverywhere;
        }

        protected override WindEverywhere.WindPattern getNewValue(EntityData data) {
            return data.Enum("newValue", WindEverywhere.WindPattern.Default);
        }
    }
}
