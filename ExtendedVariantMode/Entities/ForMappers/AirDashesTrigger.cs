using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/AirDashesTrigger")]
    public class AirDashesTrigger : AbstractExtendedVariantTrigger<Assists.DashModes> {
        public AirDashesTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.AirDashes;
        }

        protected override Assists.DashModes getNewValue(EntityData data) {
            return data.Enum("newValue", Assists.DashModes.Normal);
        }
    }
}
