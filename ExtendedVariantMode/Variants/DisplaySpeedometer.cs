using ExtendedVariants.Entities;
using System.Linq;

namespace ExtendedVariants.Variants {
    class DisplaySpeedometer : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.DisplaySpeedometer;
        }

        public override void SetValue(int value) {
            Settings.DisplaySpeedometer = value;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Celeste.Level self, Celeste.Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (!self.Entities.Any(entity => entity is Speedometer)) {
                // add the entity showing the speedometer (it will be invisible unless the option is enabled)
                self.Add(new Speedometer());
                self.Entities.UpdateLists();
            }
        }
    }
}
