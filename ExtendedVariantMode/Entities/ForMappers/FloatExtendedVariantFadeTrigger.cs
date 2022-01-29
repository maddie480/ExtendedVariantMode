using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/FloatExtendedVariantFadeTrigger")]
    public class FloatExtendedVariantFadeTrigger : Trigger {
        private readonly ExtendedVariantsModule.Variant variantChange;
        private readonly float valueA;
        private readonly float valueB;
        private readonly PositionModes positionMode;
        private readonly bool revertOnDeath;

        public FloatExtendedVariantFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            variantChange = data.Enum("variantChange", ExtendedVariantsModule.Variant.Gravity);
            valueA = data.Float("valueA", 1f);
            valueB = data.Float("valueB", 1f);
            positionMode = data.Enum("positionMode", PositionModes.LeftToRight);
            revertOnDeath = data.Bool("revertOnDeath", true);
        }

        public override void OnStay(Player player) {
            base.OnStay(player);

            float targetValue = Calc.ClampedMap(GetPositionLerp(player, positionMode), 0, 1, valueA, valueB);
            ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(variantChange, targetValue, revertOnLeave: false, isFade: true, revertOnDeath, legacy: false);
        }
    }
}
