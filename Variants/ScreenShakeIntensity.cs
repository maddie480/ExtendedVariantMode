using Celeste;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

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

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
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
            if (GetVariantValue<float>(Variant.ScreenShakeIntensity) == 1f) {
                orig(self);
                return;
            }

            shakeVectorInfo.SetValue(self, self.ShakeVector * GetVariantValue<float>(Variant.ScreenShakeIntensity), null);
            orig(self);
        }

        private void onRumbleTriggerRenderDisplacement(On.Celeste.RumbleTrigger.orig_RenderDisplacement orig, RumbleTrigger self) {
            if (GetVariantValue<float>(Variant.ScreenShakeIntensity) == 1f) {
                orig(self);
                return;
            }

            float tempRumble = (float) rumbleInfo.GetValue(self);
            rumbleInfo.SetValue(self, tempRumble * GetVariantValue<float>(Variant.ScreenShakeIntensity));
            orig(self);
            rumbleInfo.SetValue(self, tempRumble);
        }
    }
}
