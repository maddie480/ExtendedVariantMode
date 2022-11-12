using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/JungleSpidersEverywhereTrigger")]
    public class JungleSpidersEverywhereTrigger : AbstractExtendedVariantTrigger<JungleSpidersEverywhere.SpiderType> {
        public JungleSpidersEverywhereTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.JungleSpidersEverywhere;
        }

        protected override JungleSpidersEverywhere.SpiderType getNewValue(EntityData data) {
            return data.Enum("newValue", JungleSpidersEverywhere.SpiderType.Disabled);
        }
    }
}
