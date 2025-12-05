using Celeste;
using Monocle;
using System;
using System.Linq;

namespace ExtendedVariants.Variants.Vanilla {
    public class GameSpeed : AbstractVanillaVariant {
        public static readonly int[] ValidValues = { 5, 6, 7, 8, 9, 10, 12, 14, 16 };

        public GameSpeed() : base(variantType: typeof(int), defaultVariantValue: 10) { }

        public override object ConvertLegacyVariantValue(int value) {
            if (!ValidValues.Contains(value)) {
                throw new Exception("Game speed " + (value / 10f) + "x is not valid for vanilla variants!");
            }

            return value;
        }

        public override void VariantValueChanged() {
            int gameSpeed = getActiveAssistValues().GameSpeed;
#pragma warning disable CS0618 // ignore the warning, because we're actually manipulating vanilla Variant Mode here
            Engine.TimeRateB = gameSpeed / 10f;
#pragma warning restore CS0618
        }

        protected override Assists applyVariantValue(Assists target, object value) {
            target.GameSpeed = (int) value;
            return target;
        }
    }
}
