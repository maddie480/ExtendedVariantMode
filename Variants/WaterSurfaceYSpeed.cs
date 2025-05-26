using System;

namespace ExtendedVariants.Variants {
    public class WaterSurfaceSpeedY : AbstractExtendedVariant {
        public const float VanillaSpeed = 80f;

        public override object ConvertLegacyVariantValue(int value) {
            return (float)value;
        }

        public WaterSurfaceSpeedY() : base(variantType: typeof(float), defaultVariantValue: VanillaSpeed) { }

        // hooks handled in UnderwaterSpeedX
    }
}
