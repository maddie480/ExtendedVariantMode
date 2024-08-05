using System;

namespace ExtendedVariants.Variants {
    public class LiftboostCapX : AbstractExtendedVariant {
        public const float Default = 250.0f; // Player.LiftXCap

        public override Type GetVariantType() => typeof(float);
        public override object GetDefaultVariantValue() => Default;
        public override object ConvertLegacyVariantValue(int value) => value / 10.0f;

        // hooks handled in BoostMultiplier
    }

    public class LiftboostCapUp : AbstractExtendedVariant {
        public const float Default = -130.0f; // Player.LiftYCap

        public override Type GetVariantType() => typeof(float);
        public override object GetDefaultVariantValue() => Default;
        public override object ConvertLegacyVariantValue(int value) => value / 10.0f;

        // hooks handled in BoostMultiplier
    }

    public class LiftboostCapDown : AbstractExtendedVariant {
        public const float Default = 0.0f;

        public override Type GetVariantType() => typeof(float);
        public override object GetDefaultVariantValue() => Default;
        public override object ConvertLegacyVariantValue(int value) => value / 10.0f;

        // hooks handled in BoostMultiplier
    }
}