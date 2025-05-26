using Celeste;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class Invincible : AbstractVanillaVariant {
        public Invincible() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        protected override Assists applyVariantValue(Assists target, object value) {
            target.Invincible = (bool) value;
            return target;
        }
    }
}
