using Celeste;
using System;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class ScreenShakeIntensity : AbstractExtendedVariant {
        private PropertyInfo shakeVectorInfo = typeof(Level).GetProperty("ShakeVector",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private FieldInfo rumbleInfo = typeof(RumbleTrigger).GetField("rumble",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object GetVariantValue() {
            return Settings.ScreenShakeIntensity;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.ScreenShakeIntensity = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.ScreenShakeIntensity = (value / 10f);
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
            if (Settings.ScreenShakeIntensity == 1f) {
                orig(self);
                return;
            }

            shakeVectorInfo.SetValue(self, self.ShakeVector * Settings.ScreenShakeIntensity, null);
            orig(self);
        }

        private void onRumbleTriggerRenderDisplacement(On.Celeste.RumbleTrigger.orig_RenderDisplacement orig, RumbleTrigger self) {
            if (Settings.ScreenShakeIntensity == 1f) {
                orig(self);
                return;
            }

            float tempRumble = (float) rumbleInfo.GetValue(self);
            rumbleInfo.SetValue(self, tempRumble * Settings.ScreenShakeIntensity);
            orig(self);
            rumbleInfo.SetValue(self, tempRumble);
        }
    }
}
