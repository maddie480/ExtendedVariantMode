using Celeste;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class LowFriction : AbstractVanillaVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        protected override Assists applyVariantValue(Assists target, object value) {
            target.LowFriction = (bool) value;
            return target;
        }
    }
}
