using System;

namespace ExtendedVariants.Variants {
    public class LiftboostCapX : AbstractExtendedVariant {
        public const float Default = 250.0f; // Player.LiftXCap

        public LiftboostCapX() : base(variantType: typeof(float), defaultVariantValue: Default) { }
        public override object ConvertLegacyVariantValue(int value) => value / 10.0f;

        // hooks handled in BoostMultiplier
    }

    public class LiftboostCapUp : AbstractExtendedVariant {
        public const float Default = -130.0f; // Player.LiftYCap

        public LiftboostCapUp() : base(variantType: typeof(float), defaultVariantValue: Default) { }
        public override object ConvertLegacyVariantValue(int value) => value / 10.0f;

        // hooks handled in BoostMultiplier
    }

    public class LiftboostCapDown : AbstractExtendedVariant {
        public const float Default = 0.0f;

        public LiftboostCapDown() : base(variantType: typeof(float), defaultVariantValue: Default) { }
        public override object ConvertLegacyVariantValue(int value) => value / 10.0f;

        // hooks handled in BoostMultiplier
    }
}