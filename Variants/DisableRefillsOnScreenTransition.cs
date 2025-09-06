namespace ExtendedVariants.Variants {
    public class DisableRefillsOnScreenTransition : AbstractExtendedVariant {
        /// <summary>
        /// Prevents you from getting your dash and stamina back with screen transitions.
        /// </summary>
        /// <remarks>
        /// See <see cref="ScreenTransitionDashCount.onPlayerTransition(On.Celeste.Player.orig_OnTransition, Celeste.Player)"/> for the code that sets the dash count on screen transition.
        /// </remarks>
        public DisableRefillsOnScreenTransition() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }
    }
}
