using System;

namespace ExtendedVariants.Variants {
    public class DisableSuperBoosts : AbstractExtendedVariant {
        public DisableSuperBoosts() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
