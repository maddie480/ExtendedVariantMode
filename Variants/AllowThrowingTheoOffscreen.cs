using System;

namespace ExtendedVariants.Variants {
    public class AllowThrowingTheoOffscreen : AbstractExtendedVariant {
        public AllowThrowingTheoOffscreen() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
