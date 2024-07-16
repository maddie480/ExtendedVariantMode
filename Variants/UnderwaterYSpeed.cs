using System;

namespace ExtendedVariants.Variants {
    public class UnderwaterSpeedY : AbstractExtendedVariant {
        public const float VanillaSpeed = 80f;

        public override object ConvertLegacyVariantValue(int value) {
            return (float)value;
        }

        public override object GetDefaultVariantValue() {
            return VanillaSpeed;
        }

        public override Type GetVariantType() {
            return typeof(float);
        }

        // hooks handled in UnderwaterSpeedX
    }
}
