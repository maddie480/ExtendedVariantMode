using Celeste;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class MirrorMode : AbstractVanillaVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetVariantValue() {
            return SaveData.Instance.Assists.MirrorMode;
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override void SetLegacyVariantValue(int value) {
            SetVariantValue(value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            SaveData.Instance.Assists.MirrorMode = (bool) value;
            Input.MoveX.Inverted = (Input.Aim.InvertedX = (Input.Feather.InvertedX = (bool) value));
        }
    }
}
