using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class DelayBetweenBadelines : AbstractExtendedVariant {
        public DelayBetweenBadelines() : base(variantType: typeof(float), defaultVariantValue: 0.4f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void VariantValueChanged() {
            BadelineChasersEverywhere.ChangeValueRightAway(Variant.DelayBetweenBadelines,
                (baddy, value) => baddy.followBehindIndexDelay = value * baddy.index);
        }
    }
}
