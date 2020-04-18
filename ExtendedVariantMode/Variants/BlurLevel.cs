using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class BlurLevel : AbstractExtendedVariant {
        private VirtualRenderTarget tempBuffer;

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.BlurLevel;
        }

        public override void SetValue(int value) {
            Settings.BlurLevel = value;
        }

        public override void Load() {
            tempBuffer = VirtualContent.CreateRenderTarget("extended-variants-temp-blur-buffer", 320, 180);

            IL.Celeste.Level.Render += modLevelRender;
        }

        public override void Unload() {
            IL.Celeste.Level.Render -= modLevelRender;

            tempBuffer.Dispose();
            tempBuffer = null;
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
            if (Settings.BlurLevel > 0) {
                // what if... I just gaussian blur the level buffer
                GaussianBlur.Blur(GameplayBuffers.Level.Target, tempBuffer, GameplayBuffers.Level, 0, true, GaussianBlur.Samples.Nine, Settings.BlurLevel / 10f);
            }
        }
    }
}
