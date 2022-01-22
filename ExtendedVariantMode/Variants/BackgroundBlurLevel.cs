using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    // that's actually the same as BlurLevel except not at the same place in Render. :p
    // we are blurring the Level buffer after only the BG stylegrounds were rendered.
    public class BackgroundBlurLevel : AbstractExtendedVariant {
        private VirtualRenderTarget tempBuffer;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 0f;
        }

        public override object GetVariantValue() {
            return Settings.BackgroundBlurLevel;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.BackgroundBlurLevel = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.BackgroundBlurLevel = value / 10f;
        }

        public override void Load() {
            IL.Celeste.Level.Render += modLevelRender;
            On.Celeste.GameplayBuffers.Create += onGameplayBuffersCreate;
            On.Celeste.GameplayBuffers.Unload += onGameplayBuffersUnload;

            if (Engine.Scene is Level) {
                // we are already in a map, aaaaa, create the blur temp buffer real quick
                tempBuffer = VirtualContent.CreateRenderTarget("extended-variants-temp-bg-blur-buffer", 320, 180);
            }
        }

        public override void Unload() {
            IL.Celeste.Level.Render -= modLevelRender;
            On.Celeste.GameplayBuffers.Create -= onGameplayBuffersCreate;
            On.Celeste.GameplayBuffers.Unload -= onGameplayBuffersUnload;

            // dispose the blur temp buffer, or it will just be laying around for no reason.
            tempBuffer?.Dispose();
            tempBuffer = null;
        }

        private void modLevelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdfld<Level>("Background"),
                instr => instr.MatchLdarg(0),
                instr => instr.MatchCallvirt<Renderer>("Render"))) {

                Logger.Log("ExtendedVariantMode/BackgroundBlurLevel", $"Injecting call for BG blur at {cursor.Index} in IL for Level.Render");

                cursor.EmitDelegate<Action>(BackgroundBlurLevelBuffer);
            }
        }

        private void BackgroundBlurLevelBuffer() {
            if (Settings.BackgroundBlurLevel > 0) {
                // what if... I just gaussian blur the level buffer
                GaussianBlur.Blur(GameplayBuffers.Level.Target, tempBuffer, GameplayBuffers.Level, 0, true, GaussianBlur.Samples.Nine, Settings.BackgroundBlurLevel);
            }
        }

        private void onGameplayBuffersCreate(On.Celeste.GameplayBuffers.orig_Create orig) {
            orig();

            // create the blur temp buffer as well.
            tempBuffer = VirtualContent.CreateRenderTarget("extended-variants-temp-bg-blur-buffer", 320, 180);
        }

        private void onGameplayBuffersUnload(On.Celeste.GameplayBuffers.orig_Unload orig) {
            orig();

            // dispose the blur temp buffer as well.
            tempBuffer?.Dispose();
            tempBuffer = null;
        }
    }
}
