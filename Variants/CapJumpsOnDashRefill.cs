namespace ExtendedVariants.Variants {
    public class CapJumpsOnDashRefill : AbstractExtendedVariant {
        public CapJumpsOnDashRefill() : base(variantType: typeof(bool), defaultVariantValue: true) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}