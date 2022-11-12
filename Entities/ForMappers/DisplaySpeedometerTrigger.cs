using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/DisplaySpeedometerTrigger")]
    public class DisplaySpeedometerTrigger : AbstractExtendedVariantTrigger<DisplaySpeedometer.SpeedometerConfiguration> {
        public DisplaySpeedometerTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        protected override ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return ExtendedVariantsModule.Variant.DisplaySpeedometer;
        }

        protected override DisplaySpeedometer.SpeedometerConfiguration getNewValue(EntityData data) {
            return data.Enum("newValue", DisplaySpeedometer.SpeedometerConfiguration.DISABLED);
        }
    }
}
