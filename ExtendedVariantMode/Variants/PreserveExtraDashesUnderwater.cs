using Celeste;

namespace ExtendedVariants.Variants {
    public class PreserveExtraDashesUnderwater : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 1;
        }

        public override int GetValue() {
            return Settings.PreserveExtraDashesUnderwater ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.PreserveExtraDashesUnderwater = (value != 0);
        }

        public override void Load() {
            On.Celeste.Player.SwimUpdate += onSwimUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.SwimUpdate -= onSwimUpdate;
        }

        private int onSwimUpdate(On.Celeste.Player.orig_SwimUpdate orig, Player self) {
            int result = orig(self);

            if (!Settings.PreserveExtraDashesUnderwater && result == Player.StDash && self.Dashes > self.MaxDashes) {
                // if we're dashing and have more than our dash count, consume a dash!
                self.Dashes--;
            }

            return result;
        }
    }
}
