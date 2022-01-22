using Celeste;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using System;

namespace ExtendedVariants.Variants {
    public class FriendlyBadelineFollower : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(bool);
        }
        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.FriendlyBadelineFollower;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.FriendlyBadelineFollower = (bool) value;
        }

        public override void SetLegacyVariantValue(int value) {
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
