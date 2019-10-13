using Celeste;
using Monocle;

namespace ExtendedVariants.Variants {
    public class RegularHiccups : AbstractExtendedVariant {

        private float regularHiccupTimer = 0f;

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.RegularHiccups;
        }

        public override void SetValue(int value) {
            Settings.RegularHiccups = value;
        }

        public override void Load() {
            On.Celeste.Player.Update += modUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.Update -= modUpdate;
        }

        public void UpdateTimerFromSettings() {
            regularHiccupTimer = Settings.RegularHiccups / 10f;
        }

        private void modUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            if(Settings.RegularHiccups != 0 && !SaveData.Instance.Assists.Hiccups) {
                regularHiccupTimer -= Engine.DeltaTime;

                if (regularHiccupTimer > Settings.RegularHiccups / 10f) {
                    regularHiccupTimer = Settings.RegularHiccups / 10f;
                }
                if (regularHiccupTimer <= 0) {
                    regularHiccupTimer = Settings.RegularHiccups / 10f;
                    self.HiccupJump();
                }
            }
        }
    }
}
