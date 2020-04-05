using Celeste;
using System.Reflection;

namespace ExtendedVariants.Variants {
    class ScreenShakeIntensity : AbstractExtendedVariant {
        private PropertyInfo shakeVectorInfo = typeof(Level).GetProperty("ShakeVector",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private FieldInfo rumbleInfo = typeof(RumbleTrigger).GetField("rumble",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.ScreenShakeIntensity;
        }

        public override void SetValue(int value) {
            Settings.ScreenShakeIntensity = value;
        }

        public override void Load() {
            On.Celeste.Level.BeforeRender += onLevelBeforeRender;
            On.Celeste.RumbleTrigger.RenderDisplacement += onRumbleTriggerRenderDisplacement;
        }

        public override void Unload() {
            On.Celeste.Level.BeforeRender -= onLevelBeforeRender;
            On.Celeste.RumbleTrigger.RenderDisplacement -= onRumbleTriggerRenderDisplacement;
        }

        private void onLevelBeforeRender(On.Celeste.Level.orig_BeforeRender orig, Level self) {
            if (Settings.ScreenShakeIntensity == 10) {
                orig(self);
                return;
            }

            shakeVectorInfo.SetValue(self, self.ShakeVector * Settings.ScreenShakeIntensity / 10f, null);
            orig(self);
        }

        private void onRumbleTriggerRenderDisplacement(On.Celeste.RumbleTrigger.orig_RenderDisplacement orig, RumbleTrigger self) {
            if (Settings.ScreenShakeIntensity == 10) {
                orig(self);
                return;
            }

            float tempRumble = (float) rumbleInfo.GetValue(self);
            rumbleInfo.SetValue(self, tempRumble * Settings.ScreenShakeIntensity / 10f);
            orig(self);
            rumbleInfo.SetValue(self, tempRumble);
        }
    }
}
