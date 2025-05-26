using System;

namespace ExtendedVariants.Variants {
    public class FirstBadelineSpawnRandom : AbstractExtendedVariant {
        public FirstBadelineSpawnRandom() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
