using System;

namespace ExtendedVariants.Variants {
    public class NoFreezeFramesAdvanceCassetteBlocks : AbstractExtendedVariant {
        public NoFreezeFramesAdvanceCassetteBlocks() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
