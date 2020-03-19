using Celeste;
using ExtendedVariants.Entities;

namespace ExtendedVariants.Variants {
    class EverythingIsUnderwater : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.EverythingIsUnderwater ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.EverythingIsUnderwater = (value != 0);
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (!self.Session?.LevelData?.Underwater ?? false) {
                // inject a controller that will spawn/despawn water depending on the extended variant setting.
                self.Add(new UnderwaterSwitchController(Settings));
                self.Entities.UpdateLists();
            }
        }
    }
}
