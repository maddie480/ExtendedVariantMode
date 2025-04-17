using System;

namespace ExtendedVariants.Variants {
    public class AffectExistingChasers : AbstractExtendedVariant {
        public AffectExistingChasers() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
