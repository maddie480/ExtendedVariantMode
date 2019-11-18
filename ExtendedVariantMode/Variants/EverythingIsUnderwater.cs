using Celeste;
using Microsoft.Xna.Framework;

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

            LevelData levelData = self.Session.LevelData;
            if (Settings.EverythingIsUnderwater && !levelData.Underwater) {
                // reproduce the vanilla behavior of the Underwater flag: ... simply cover the level with water.
                // (we make the water go 10 pixels above the screen to avoid having a weird "coming out from water" sound effect on upwards transitions.)
                self.Add(new Water(new Vector2(levelData.Bounds.Left, levelData.Bounds.Top - 10), 
                    false, false, levelData.Bounds.Width, levelData.Bounds.Height + 10));
            }
        }
    }
}
