using System;

namespace ExtendedVariants.Variants {
    public class WaterSurfaceSpeedX : AbstractExtendedVariant {
        public const float VanillaSpeed = 80f;

        public override object ConvertLegacyVariantValue(int value) {
            return (float)value;
        }

        public WaterSurfaceSpeedX() : base(variantType: typeof(float), defaultVariantValue: VanillaSpeed) { }

        // hooks handled in UnderwaterSpeedX
    }
}
