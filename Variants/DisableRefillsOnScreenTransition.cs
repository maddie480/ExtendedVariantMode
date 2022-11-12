
using System;

namespace ExtendedVariants.Variants {
    public class DisableRefillsOnScreenTransition : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.DisableRefillsOnScreenTransition;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DisableRefillsOnScreenTransition = (bool) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.DisableRefillsOnScreenTransition = (value != 0);
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

            if (Settings.DisableRefillsOnScreenTransition) {
                self.Dashes = bakDashes;
                self.Stamina = bakStamina;
            }
        }
    }
}
