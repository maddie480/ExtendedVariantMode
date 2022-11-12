using Celeste;
using Monocle;
using System;
using System.Linq;

namespace ExtendedVariants.Variants.Vanilla {
    public class GameSpeed : AbstractVanillaVariant {
        public static readonly int[] ValidValues = { 5, 6, 7, 8, 9, 10, 12, 14, 16 };

        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetVariantValue() {
            return SaveData.Instance.Assists.GameSpeed;
        }

        public override object GetDefaultVariantValue() {
            return 10;
        }

        protected override void DoSetVariantValue(object value) {
            SetLegacyVariantValue((int) value);
        }

        public override void SetLegacyVariantValue(int value) {
            if (!ValidValues.Contains(value)) {
                throw new Exception("Game speed " + (value / 10f) + "x is not valid for vanilla variants!");
            }

            SaveData.Instance.Assists.GameSpeed = value;
            Engine.TimeRateB = SaveData.Instance.Assists.GameSpeed / 10f;
        }
    }
}
