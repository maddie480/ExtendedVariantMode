using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/FloatExtendedVariantSliderTrigger=Load")]
    public class FloatExtendedVariantSliderTrigger : Trigger {
        private readonly ExtendedVariantsModule.Variant variantChange;
        private readonly Session.Slider slider;
        private readonly bool revertOnDeath;

        public static FloatExtendedVariantSliderTrigger Load(Level level, LevelData levelData, Vector2 offset, EntityData data)
            => new(data, offset, level.Session.GetSliderObject(data.Attr("slider")));

        public FloatExtendedVariantSliderTrigger(EntityData data, Vector2 offset, Session.Slider slider) : base(data, offset) {
            variantChange = data.Enum("variantChange", ExtendedVariantsModule.Variant.Gravity);
            this.slider = slider;
            revertOnDeath = data.Bool("revertOnDeath", true);
        }

        public override void OnStay(Player player) {
            base.OnStay(player);

            ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(variantChange, slider.Value, revertOnLeave: false, isFade: true, revertOnDeath, legacy: false);
        }
    }
}
