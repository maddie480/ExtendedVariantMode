using System;

namespace ExtendedVariants.Variants {
    public class SetSeed : AbstractExtendedVariant {
        public SetSeed() : base(variantType: typeof(int), defaultVariantValue: 0) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }
    }
}
