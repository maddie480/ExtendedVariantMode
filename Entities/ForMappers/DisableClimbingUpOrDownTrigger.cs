using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/DisableClimbingUpOrDownTrigger")]
    public class DisableClimbingUpOrDownTrigger : AbstractExtendedVariantTrigger<DisableClimbingUpOrDown.ClimbUpOrDownOptions> {
        public DisableClimbingUpOrDownTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.DisableClimbingUpOrDown;
        }

        protected override DisableClimbingUpOrDown.ClimbUpOrDownOptions getNewValue(EntityData data) {
            return data.Enum("newValue", DisableClimbingUpOrDown.ClimbUpOrDownOptions.Disabled);
        }
    }
}
