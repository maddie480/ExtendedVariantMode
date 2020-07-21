using Celeste;
using Monocle;
using MonoMod.Utils;
using System.Linq;

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
            PlayerDeadBody deadPlayer = null;
            if (player == null) {
                deadPlayer = scene?.Entities?.OfType<PlayerDeadBody>().FirstOrDefault();
            }

            if (Settings.DisableMadelineSpotlight) {
                // save the lighting alpha, then replace it.
                if (player != null) {
                    origSpotlightAlpha = player.Light.Alpha;
                    player.Light.Alpha = 0f;
                } else if (deadPlayer != null) {
                    VertexLight light = new DynData<PlayerDeadBody>(deadPlayer).Get<VertexLight>("light");
                    origSpotlightAlpha = light.Alpha;
                    light.Alpha = 0f;
                }
            }

            orig(self, scene);

            if (Settings.DisableMadelineSpotlight) {
                // restore the spotlight
                if (player != null) {
                    player.Light.Alpha = origSpotlightAlpha;
                } else if (deadPlayer != null) {
                    VertexLight light = new DynData<PlayerDeadBody>(deadPlayer).Get<VertexLight>("light");
                    light.Alpha = origSpotlightAlpha;
                }
            }
        }
    }
}
