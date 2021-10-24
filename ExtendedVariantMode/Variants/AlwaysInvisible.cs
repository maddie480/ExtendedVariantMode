
namespace ExtendedVariants.Variants {
    class AlwaysInvisible : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.AlwaysInvisible ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.AlwaysInvisible = (value != 0);
        }

        public override void Load() {
            On.Celeste.PlayerSeeker.Render += modPlayerSeekerRender;
            On.Celeste.Player.Render += modPlayerRender;
        }

        public override void Unload() {
            On.Celeste.PlayerSeeker.Render -= modPlayerSeekerRender;
            On.Celeste.Player.Render -= modPlayerRender;
        }

        // vanilla implements Invisible Motion by skipping the Render method entirely when the player is moving... so we're just going to do the same. :p

        private void modPlayerSeekerRender(On.Celeste.PlayerSeeker.orig_Render orig, Celeste.PlayerSeeker self) {
            if (!Settings.AlwaysInvisible) {
                orig(self);
            }
        }

        private void modPlayerRender(On.Celeste.Player.orig_Render orig, Celeste.Player self) {
            if (!Settings.AlwaysInvisible) {
                orig(self);
            }
        }
    }
}
