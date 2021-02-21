
namespace ExtendedVariants.Variants {
    class DisableRefillsOnScreenTransition : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.DisableRefillsOnScreenTransition ? 1 : 0;
        }

        public override void SetValue(int value) {
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
