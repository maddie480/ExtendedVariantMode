using Celeste;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class DashAssist : AbstractVanillaVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetVariantValue() {
            return SaveData.Instance.Assists.DashAssist;
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override void SetLegacyVariantValue(int value) {
            SaveData.Instance.Assists.DashAssist = (value != 0);
        }

        protected override void DoSetVariantValue(object value) {
            SaveData.Instance.Assists.DashAssist = (bool) value;
        }
    }
}
