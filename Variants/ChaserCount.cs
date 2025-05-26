using System;

namespace ExtendedVariants.Variants {
    public class ChaserCount : AbstractExtendedVariant {
        public ChaserCount() : base(variantType: typeof(int), defaultVariantValue: 1) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }
    }
}
