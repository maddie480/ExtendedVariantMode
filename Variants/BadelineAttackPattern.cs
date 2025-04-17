using System;

namespace ExtendedVariants.Variants {
    public class BadelineAttackPattern : AbstractExtendedVariant {
        public BadelineAttackPattern() : base(variantType: typeof(int), defaultVariantValue: 0) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }
    }
}
