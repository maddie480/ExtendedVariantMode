using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/IntegerExtendedVariantTrigger", "ExtendedVariantMode/BadelineAttackPatternTrigger", "ExtendedVariantMode/JumpCountTrigger")]
    public class IntegerExtendedVariantTrigger : AbstractExtendedVariantTrigger<int> {
        public IntegerExtendedVariantTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override int getNewValue(EntityData data) {
            if (data.Bool("infinite")) return int.MaxValue;
            return data.Int("newValue");
        }
    }
}
