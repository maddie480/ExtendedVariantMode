using Celeste;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Variants {
    public class FriendlyBadelineFollower : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.FriendlyBadelineFollower ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.FriendlyBadelineFollower = (value != 0);
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            Player player = self.Tracker.GetEntity<Player>();

            if (Settings.FriendlyBadelineFollower && player != null && self.Tracker.CountEntities<FriendlyBaddy>() == 0) {
                self.Add(new FriendlyBaddy(player.Center + new Vector2(-16f * (int) player.Facing, -16f)));
            }
        }
    }
}
