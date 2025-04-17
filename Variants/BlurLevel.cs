using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class BlurLevel : AbstractExtendedVariant {
        private VirtualRenderTarget tempBuffer;

        public BlurLevel() : base(variantType: typeof(float), defaultVariantValue: 0f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Level.Render += modLevelRender;
            On.Celeste.GameplayBuffers.Create += onGameplayBuffersCreate;
            On.Celeste.GameplayBuffers.Unload += onGameplayBuffersUnload;

            if (Engine.Scene is Level) {
                // we are already in a map, aaaaa, create the blur temp buffer real quick
                ensureBufferIsCorrect();
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

        private void ensureBufferIsCorrect() {
            if (tempBuffer == null || tempBuffer.Width != GameplayWidth || tempBuffer.Height != GameplayHeight) {
                tempBuffer?.Dispose();
                tempBuffer = VirtualContent.CreateRenderTarget("extended-variants-temp-blur-buffer", GameplayWidth, GameplayHeight);
            }
        }

        private void modLevelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                instr => instr.MatchLdnull(),
                instr => instr.MatchCallvirt<GraphicsDevice>("SetRenderTarget"))) {

                Logger.Log("ExtendedVariantMode/BlurLevel", $"Injecting call for blur at {cursor.Index} in IL for Level.Render");

                cursor.EmitDelegate<Action>(blurLevelBuffer);
            }
        }

        private void blurLevelBuffer() {
            if (GetVariantValue<float>(Variant.BlurLevel) > 0) {
                // what if... I just gaussian blur the level buffer
                ensureBufferIsCorrect();
                GaussianBlur.Blur(GameplayBuffers.Level.Target, tempBuffer, GameplayBuffers.Level, 0, true, GaussianBlur.Samples.Nine, GetVariantValue<float>(Variant.BlurLevel));
            }
        }

        private void onGameplayBuffersCreate(On.Celeste.GameplayBuffers.orig_Create orig) {
            orig();

            // create the blur temp buffer as well.
            ensureBufferIsCorrect();
        }

        private void onGameplayBuffersUnload(On.Celeste.GameplayBuffers.orig_Unload orig) {
            orig();

            // dispose the blur temp buffer as well.
            tempBuffer?.Dispose();
            tempBuffer = null;
        }
    }
}
