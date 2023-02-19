using Celeste;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class MirrorMode : AbstractVanillaVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void VariantValueChanged() {
            bool mirrorMode = applyAssists(SaveData.Instance.Assists, out _).MirrorMode;
            Input.MoveX.Inverted = (Input.Aim.InvertedX = (Input.Feather.InvertedX = mirrorMode));
        }

        protected override Assists applyVariantValue(Assists target, object value) {
            target.MirrorMode = (bool) value;
            return target;
        }
    }
}
