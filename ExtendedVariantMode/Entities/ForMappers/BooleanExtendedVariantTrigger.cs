using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/BooleanExtendedVariantTrigger")]
    public class BooleanExtendedVariantTrigger : AbstractExtendedVariantTrigger<bool> {
        public BooleanExtendedVariantTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override bool getNewValue(EntityData data) {
            return data.Bool("newValue");
        }
    }
}
