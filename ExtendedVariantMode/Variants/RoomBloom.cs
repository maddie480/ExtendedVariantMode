using Celeste;
using Celeste.Mod;

namespace ExtendedVariants.Variants {
    public class RoomBloom : AbstractExtendedVariant {

        bool moddedRoomBloom = false;

        public override int GetDefaultValue() {
            return -1;
        }

        public override int GetValue() {
            return Settings.RoomBloom;
        }

        public override void SetValue(int value) {
            Settings.RoomBloom = value;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.BloomFadeTrigger.OnStay += modBloomFadeTriggerOnStay;
            Everest.Events.Level.OnExit += onLevelExit;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.BloomFadeTrigger.OnStay -= modBloomFadeTriggerOnStay;
            Everest.Events.Level.OnExit -= onLevelExit;
        }

        /// <summary>
        /// Mods the bloom of a new room being loaded.
        /// </summary>
        /// <param name="self">The level we are in</param>
        /// <param name="introType">How the player enters the level</param>
        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (Settings.RoomBloom != -1) {
                // mod the room bloom
                self.Bloom.Base = Settings.RoomBloom / 10f;
                moddedRoomBloom = true;
            } else if(moddedRoomBloom) {
                // variant is disabled => restore the bloom according to session
                self.Bloom.Base = AreaData.Get(self).BloomBase + self.Session.BloomBaseAdd;
                moddedRoomBloom = false;
            }
        }

        /// <summary>
        /// Locks the bloom to the value set by the user even when they hit a Bloom Fade Trigger.
        /// (The Bloom Fade Trigger will still update the BloomBaseAdd in the session, so we can use it to revert the changes if the variant is disabled.)
        /// </summary>
        /// <param name="orig">The original method</param>
        /// <param name="self">The trigger itself</param>
        /// <param name="player">The player hitting the trigger</param>
        private void modBloomFadeTriggerOnStay(On.Celeste.BloomFadeTrigger.orig_OnStay orig, BloomFadeTrigger self, Player player) {
            orig(self, player);

            if (Settings.RoomBloom != -1) {
                // be sure to lock the bloom to the value set by the player
                self.SceneAs<Level>().Bloom.Base = Settings.RoomBloom / 10f;
            }
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            // reset value to default
            moddedRoomBloom = false;
        }
    }
}
