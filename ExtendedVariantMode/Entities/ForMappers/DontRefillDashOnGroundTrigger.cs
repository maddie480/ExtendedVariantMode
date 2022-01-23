using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/DontRefillDashOnGroundTrigger")]
    class DontRefillDashOnGroundTrigger : AbstractExtendedVariantTrigger<DontRefillDashOnGround.DashRefillOnGroundConfiguration> {
        public DontRefillDashOnGroundTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.DontRefillDashOnGround;
        }

        protected override DontRefillDashOnGround.DashRefillOnGroundConfiguration getNewValue(EntityData data) {
            return data.Enum("newValue", DontRefillDashOnGround.DashRefillOnGroundConfiguration.DEFAULT);
        }
    }
}
