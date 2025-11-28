using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class BackgroundBrightness : AbstractExtendedVariant {
        private static VirtualRenderTarget blackMask;

        public BackgroundBrightness() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Level.Render += modLevelRender;
            On.Celeste.Level.BeforeRender += onLevelBeforeRender;
            On.Celeste.GameplayBuffers.Create += onGameplayBuffersCreate;
            On.Celeste.GameplayBuffers.Unload += onGameplayBuffersUnload;

            if (Engine.Scene is Level) {
                // we are already in a map, aaaaa, create the black mask real quick
                ensureBufferIsCorrect();
            }
        }

        public override void Unload() {
            IL.Celeste.Level.Render -= modLevelRender;
            On.Celeste.Level.BeforeRender -= onLevelBeforeRender;
            On.Celeste.GameplayBuffers.Create -= onGameplayBuffersCreate;
            On.Celeste.GameplayBuffers.Unload -= onGameplayBuffersUnload;

            // dispose the black mask, or it will just be laying around for no reason.
            blackMask?.Dispose();
            blackMask = null;
        }

        private static void ensureBufferIsCorrect() {
            if (blackMask == null || blackMask.Width != GameplayWidth || blackMask.Height != GameplayHeight) {
                blackMask?.Dispose();
                blackMask = VirtualContent.CreateRenderTarget("extended-variants-black-mask", GameplayWidth, GameplayHeight);
            }
        }

        private static void modLevelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdfld<Level>("Background"),
                instr => instr.MatchLdarg(0),
                instr => instr.MatchCallvirt<Renderer>("Render"))) {

                // sneak between the background rendering and the gameplay rendering.
                // we want to slap the lighting here!
                Logger.Log("ExtendedVariantMode/BackgroundBrightness", $"Adding lighting rendering at {cursor.Index} in IL for Level.Render");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Level>>(renderBackgroundLighting);
            }
        }

        private static void onLevelBeforeRender(On.Celeste.Level.orig_BeforeRender orig, Level self) {
            orig(self);

            if (blackMask != null) {
                // ensure the black mask is... well, black.
                ensureBufferIsCorrect();
                Engine.Graphics.GraphicsDevice.SetRenderTarget(blackMask);
                Engine.Graphics.GraphicsDevice.Clear(Color.Black);
            }
        }

        private static void renderBackgroundLighting(Level self) {
            if (GetVariantValue<float>(Variant.BackgroundBrightness) < 1f) {
                // Apply a mask over the background layer, but behind the gameplay layer.
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, GFX.DestinationTransparencySubtract, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GFX.FxDither, Matrix.Identity);
                Draw.SpriteBatch.Draw(blackMask, Vector2.Zero, Color.White * MathHelper.Clamp(1 - GetVariantValue<float>(Variant.BackgroundBrightness), 0f, 1f));
                Draw.SpriteBatch.End();
            }
        }

        private static void onGameplayBuffersCreate(On.Celeste.GameplayBuffers.orig_Create orig) {
            orig();

            // create the black mask as well.
            ensureBufferIsCorrect();
        }

        private static void onGameplayBuffersUnload(On.Celeste.GameplayBuffers.orig_Unload orig) {
            orig();

            // dispose the black mask as well.
            blackMask?.Dispose();
            blackMask = null;
        }
    }
}
