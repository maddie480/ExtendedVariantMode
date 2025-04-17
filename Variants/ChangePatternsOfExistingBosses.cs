using System;

namespace ExtendedVariants.Variants {
    public class ChangePatternsOfExistingBosses : AbstractExtendedVariant {
        public ChangePatternsOfExistingBosses() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
