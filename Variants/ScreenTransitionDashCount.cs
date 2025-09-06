using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class ScreenTransitionDashCount : AbstractExtendedVariant {

        public ScreenTransitionDashCount() : base(variantType: typeof(int), defaultVariantValue: -1) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }

        public override void Load() {
            On.Celeste.Player.OnTransition += onPlayerTransition;
        }

        public override void Unload() {
            On.Celeste.Player.OnTransition -= onPlayerTransition;
        }

        private void onPlayerTransition(On.Celeste.Player.orig_OnTransition orig, Celeste.Player self) {
            int bakDashes = self.Dashes;
            float bakStamina = self.Stamina;

            orig(self);

            var dashCount = GetVariantValue<int>(Variant.ScreenTransitionDashCount);
            if (GetVariantValue<bool>(Variant.DisableRefillsOnScreenTransition)) {
                self.Dashes = bakDashes;
                self.Stamina = bakStamina;
            } else if (dashCount != -1) {
                self.Dashes = dashCount;
            }
        }
    }
}
