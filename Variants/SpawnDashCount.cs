namespace ExtendedVariants.Variants {
    public class SpawnDashCount : AbstractExtendedVariant {
        /// <summary>
        /// Grants Madeline additional dashes on spawn.
        /// You can also start with 0 dashes, which can be useful in combination with disabling dash refills on ground & screen transitions.
        /// </summary>
        /// <remarks>
        /// See <see cref="DashCount.modAdded"/> for the code that sets the player's dashes on spawn.
        /// </remarks>

        public SpawnDashCount() : base(variantType: typeof(int), defaultVariantValue: -1) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }
    }
}
