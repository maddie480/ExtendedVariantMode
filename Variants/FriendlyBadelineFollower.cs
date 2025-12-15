using Celeste;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class FriendlyBadelineFollower : AbstractExtendedVariant {

        public FriendlyBadelineFollower() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
            On.Celeste.Player.Update += onPlayerUpdate;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
            On.Celeste.Player.Update -= onPlayerUpdate;
        }

        private static bool wasActiveOnLastFrame = false;

        private static void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            Player player = self.Tracker.GetEntity<Player>();

            if (GetVariantValue<bool>(Variant.FriendlyBadelineFollower) && player != null && self.Tracker.CountEntities<FriendlyBaddy>() == 0) {
                self.Add(new FriendlyBaddy(player.Center + new Vector2(-16f * (int) player.Facing, -16f)));
            }

            wasActiveOnLastFrame = GetVariantValue<bool>(Variant.FriendlyBadelineFollower);
        }

        private static void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            Level level = Engine.Scene as Level;

            if (!wasActiveOnLastFrame && GetVariantValue<bool>(Variant.FriendlyBadelineFollower) && level.Tracker.CountEntities<FriendlyBaddy>() == 0) {
                // Friendly Badeline Follower was just activated (by trigger or mod options), so make her appear right now!
                FriendlyBaddy baddy = new FriendlyBaddy(self.Center);
                level.Add(baddy);
                baddy.Appear(level, silent: true);
                Audio.Play("event:/char/badeline/maddy_split", self.Center);
            }

            wasActiveOnLastFrame = GetVariantValue<bool>(Variant.FriendlyBadelineFollower);
        }
    }
}
