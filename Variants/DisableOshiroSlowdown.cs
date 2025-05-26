using System;

namespace ExtendedVariants.Variants {
    public class DisableOshiroSlowdown : AbstractExtendedVariant {
        public DisableOshiroSlowdown() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
