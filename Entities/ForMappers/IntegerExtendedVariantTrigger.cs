using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/IntegerExtendedVariantTrigger",
        "ExtendedVariantMode/BadelineAttackPatternTrigger", "ExtendedVariantMode/GameSpeedTrigger")]
    public class IntegerExtendedVariantTrigger : AbstractExtendedVariantTrigger<int> {
        public IntegerExtendedVariantTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override int getNewValue(EntityData data) {
            return data.Int("newValue");
        }
    }
}
