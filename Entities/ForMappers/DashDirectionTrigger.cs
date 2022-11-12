using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/DashDirectionTrigger")]
    public class DashDirectionTrigger : AbstractExtendedVariantTrigger<bool[][]> {
        public DashDirectionTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.DashDirection;
        }

        protected override bool[][] getNewValue(EntityData data) {
            return new bool[][] {
                new bool[] { data.Bool("topLeft"), data.Bool("top"), data.Bool("topRight") },
                new bool[] { data.Bool("left"), true, data.Bool("right") },
                new bool[] { data.Bool("bottomLeft"), data.Bool("bottom"), data.Bool("bottomRight") },
            };
        }
    }
}
