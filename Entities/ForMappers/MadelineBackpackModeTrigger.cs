using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/MadelineBackpackModeTrigger")]
    public class MadelineBackpackModeTrigger : AbstractExtendedVariantTrigger<MadelineBackpackMode.MadelineBackpackModes> {
        public MadelineBackpackModeTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.MadelineBackpackMode;
        }

        protected override MadelineBackpackMode.MadelineBackpackModes getNewValue(EntityData data) {
            return data.Enum("newValue", MadelineBackpackMode.MadelineBackpackModes.Default);
        }
    }
}
