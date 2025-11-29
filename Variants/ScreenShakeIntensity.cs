using Celeste;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class ScreenShakeIntensity : AbstractExtendedVariant {
        public ScreenShakeIntensity() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

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

        private static void onLevelBeforeRender(On.Celeste.Level.orig_BeforeRender orig, Level self) {
            if (GetVariantValue<float>(Variant.ScreenShakeIntensity) == 1f) {
                orig(self);
                return;
            }

            self.ShakeVector *= GetVariantValue<float>(Variant.ScreenShakeIntensity);
            orig(self);
        }

        private static void onRumbleTriggerRenderDisplacement(On.Celeste.RumbleTrigger.orig_RenderDisplacement orig, RumbleTrigger self) {
            if (GetVariantValue<float>(Variant.ScreenShakeIntensity) == 1f) {
                orig(self);
                return;
            }

            float tempRumble = self.rumble;
            self.rumble = tempRumble * GetVariantValue<float>(Variant.ScreenShakeIntensity);
            orig(self);
            self.rumble = tempRumble;
        }
    }
}
