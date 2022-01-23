using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace ExtendedVariants.Entities.Legacy {
    // extended variant trigger that can fade _any_ variant (yes, even Color Grading) by fading its legacy integer value.
    // obsoleted by Float Extended Variant Fade Trigger, that fades float variants way more smoothly (that is, not by 0.1x / 10% steps).
    [CustomEntity("ExtendedVariantMode/ExtendedVariantFadeTrigger")]
    public class ExtendedVariantFadeTrigger : Trigger {
        private readonly ExtendedVariantsModule.Variant variantChange;
        private readonly int valueA;
        private readonly int valueB;
        private readonly PositionModes positionMode;
        private readonly bool revertOnDeath;

        public ExtendedVariantFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            variantChange = data.Enum("variantChange", ExtendedVariantsModule.Variant.Gravity);
            valueA = data.Int("valueA", 10);
            valueB = data.Int("valueB", 10);
            positionMode = data.Enum("positionMode", PositionModes.LeftToRight);
            revertOnDeath = data.Bool("revertOnDeath", true);
        }

        public override void OnStay(Player player) {
            base.OnStay(player);

            float targetValue = Calc.ClampedMap(GetPositionLerp(player, positionMode), 0, 1, valueA, valueB);
            ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(variantChange, (int) Math.Round(targetValue), revertOnLeave: false, isFade: true, revertOnDeath, legacy: true);
        }
    }
}
