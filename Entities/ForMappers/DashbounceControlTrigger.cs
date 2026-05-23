using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/DashbounceControlTrigger")]
    public class DashbounceControlTrigger : AbstractExtendedVariantTrigger<DashbounceControl.DashbounceControlMode> {
        public DashbounceControlTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) => ExtendedVariantsModule.Variant.DashbounceControl;

        protected override DashbounceControl.DashbounceControlMode getNewValue(EntityData data) => data.Enum("newValue", DashbounceControl.DashbounceControlMode.Off);
    }
}