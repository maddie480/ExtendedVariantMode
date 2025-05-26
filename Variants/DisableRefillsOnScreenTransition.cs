
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class DisableRefillsOnScreenTransition : AbstractExtendedVariant {

        public DisableRefillsOnScreenTransition() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
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

            if (GetVariantValue<bool>(Variant.DisableRefillsOnScreenTransition)) {
                self.Dashes = bakDashes;
                self.Stamina = bakStamina;
            }
        }
    }
}
