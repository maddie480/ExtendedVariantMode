using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/FloatExtendedVariantTrigger")]
    public class FloatExtendedVariantTrigger : AbstractExtendedVariantTrigger<float> {
        public FloatExtendedVariantTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override float getNewValue(EntityData data) {
            return data.Float("newValue");
        }
    }
}
