using Celeste;
using Monocle;

namespace ExtendedVariants.Variants {
    class DisableMadelineSpotlight : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.DisableMadelineSpotlight ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.DisableMadelineSpotlight = (value != 0);
        }

        public override void Load() {
            On.Celeste.LightingRenderer.BeforeRender += onLightingRender;
        }

        public override void Unload() {
            On.Celeste.LightingRenderer.BeforeRender -= onLightingRender;
        }

        private void onLightingRender(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene) {
            float origSpotlightAlpha = 0f;

            Player player = scene?.Tracker.GetEntity<Player>();
            if (Settings.DisableMadelineSpotlight && player != null) {
                // save the lighting alpha, then replace it.
                origSpotlightAlpha = player.Light.Alpha;
                player.Light.Alpha = 0f;
            }

            orig(self, scene);

            if (Settings.DisableMadelineSpotlight && player != null) {
                // restore the spotlight
                player.Light.Alpha = origSpotlightAlpha;
            }
        }
    }
}
