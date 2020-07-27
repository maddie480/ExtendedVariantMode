using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    class ForegroundEffectOpacity : AbstractExtendedVariant {
        private VirtualRenderTarget foregroundEffectBuffer;

        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.ForegroundEffectOpacity;
        }

        public override void SetValue(int value) {
            Settings.ForegroundEffectOpacity = value;
        }

        public override void Load() {
            IL.Celeste.Level.Render += modLevelRender;
            On.Celeste.GameplayBuffers.Create += onGameplayBuffersCreate;
            On.Celeste.GameplayBuffers.Unload += onGameplayBuffersUnload;
        }

        public override void Unload() {
            IL.Celeste.Level.Render -= modLevelRender;
            On.Celeste.GameplayBuffers.Create -= onGameplayBuffersCreate;
            On.Celeste.GameplayBuffers.Unload -= onGameplayBuffersUnload;
        }

        private void modLevelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdfld<Level>("Foreground"),
                instr => instr.MatchLdarg(0),
                instr => instr.MatchCallvirt<Renderer>("Render"))) {

                Logger.Log("ExtendedVariantMode/ForegroundEffectOpacity", $"Modding foreground rendering at {cursor.Index} in IL for Level.Render");

                cursor.EmitDelegate<Action>(prepareForegroundOpacityEffect);
                cursor.Index += 4;
                cursor.EmitDelegate<Action>(finishForegroundOpacityEffect);
            }
        }

        private void prepareForegroundOpacityEffect() {
            if (Settings.ForegroundEffectOpacity < 10) {
                // redirect the foreground rendering to our render target.
                Engine.Graphics.GraphicsDevice.SetRenderTarget(foregroundEffectBuffer);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            }
        }

        private void finishForegroundOpacityEffect() {
            if (Settings.ForegroundEffectOpacity < 10) {
                // redirect the rendering back to the level buffer.
                Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);

                // render the foreground ourselves with the opacity we want.
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null);
                Draw.SpriteBatch.Draw(foregroundEffectBuffer, Vector2.Zero, Color.White * (Settings.ForegroundEffectOpacity / 10f));
                Draw.SpriteBatch.End();
            }
        }

        private void onGameplayBuffersCreate(On.Celeste.GameplayBuffers.orig_Create orig) {
            orig();

            // create the foreground effect buffer as well.
            foregroundEffectBuffer = VirtualContent.CreateRenderTarget("foreground-effect-buffer", 320, 180);
        }

        private void onGameplayBuffersUnload(On.Celeste.GameplayBuffers.orig_Unload orig) {
            orig();

            // dispose the black mask as well.
            foregroundEffectBuffer?.Dispose();
            foregroundEffectBuffer = null;
        }
    }
}
