using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class UpsideDown : AbstractExtendedVariant {
        private static FieldInfo inputFeather = typeof(Input).GetField("Feather"); // only exists on the beta! will be null on 1.3.1.2

        private static ZoomLevel zoomLevelVariant;

        private static ILHook hdParallaxHook;

        public UpsideDown(ZoomLevel zoomLevel) {
            zoomLevelVariant = zoomLevel;
        }

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            IL.Celeste.Level.Render += modLevelRender;
            On.Celeste.Level.Update += onLevelUpdate;
        }

        public override void Unload() {
            IL.Celeste.Level.Render -= modLevelRender;
            On.Celeste.Level.Update -= onLevelUpdate;

            // be sure the controls are not upside down anymore
            Input.Aim.InvertedY = (Input.GliderMoveY.Inverted = (Input.MoveY.Inverted = false));
            if (inputFeather != null) {
                (inputFeather.GetValue(null) as VirtualJoystick).InvertedY = false;
            }

            hdParallaxHook?.Dispose();
            hdParallaxHook = null;
        }

        public static void Initialize() {
            if (ExtendedVariantsModule.Instance.MaxHelpingHandInstalled) {
                // flip HD stylegrounds upside-down
                MethodInfo renderMethod = Everest.Modules.Where(m => m.Metadata?.Name == "MaxHelpingHand").First().GetType().Assembly
                    .GetType("Celeste.Mod.MaxHelpingHand.Effects.HdParallax")
                    .GetMethod("renderForReal", BindingFlags.NonPublic | BindingFlags.Instance);

                hdParallaxHook = new ILHook(renderMethod, patchHDStylegroundsRendering);
            }
        }

        /// <summary>
        /// Edits the Render method in Level (handling the whole level rendering).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modLevelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump right where Mirror Mode is handled
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdfld(typeof(Assists), "MirrorMode"))) {
                // move back 2 steps (we are between Instance and MirrorMode in "SaveData.Instance.MirrorMode" and we want to be before that)
                cursor.Index -= 2;

                VariableDefinition positionVector = seekReferenceTo(il, cursor.Index, 5);
                VariableDefinition paddingVector = seekReferenceTo(il, cursor.Index, 9);

                if (positionVector == null || paddingVector == null) {
                    positionVector = seekReferenceTo(il, cursor.Index, 10);
                    paddingVector = seekReferenceTo(il, cursor.Index, 14);
                }

                if (positionVector != null && paddingVector != null) {
                    // insert our delegates to do about the same thing as vanilla Celeste at about the same time
                    Logger.Log("ExtendedVariantMode/UpsideDown", $"Adding upside down delegate call at {cursor.Index} in CIL code for LevelRender");

                    cursor.Emit(OpCodes.Ldloca_S, paddingVector);
                    cursor.Emit(OpCodes.Ldloca_S, positionVector);
                    cursor.EmitDelegate<TwoRefVectorParameters>(applyUpsideDownEffect);
                }
            }

            // move forward a bit to get after the MirrorMode loading
            cursor.Index += 3;

            // jump to the next MirrorMode usage again
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldfld && ((FieldReference) instr.Operand).Name.Contains("MirrorMode"))) {
                // jump back 2 steps
                cursor.Index -= 2;

                Logger.Log("ExtendedVariantMode/UpsideDown", $"Adding upside down delegate call at {cursor.Index} in CIL code for LevelRender for sprite effects");

                // erase "SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None"
                // that's 3 instructions to load MirrorMode, and 4 assigning either 1 or 0 to it
                cursor.RemoveRange(7);

                // and replace it with a delegate call
                cursor.EmitDelegate<Func<SpriteEffects>>(applyUpsideDownEffectToSprites);
            }
        }

        /// <summary>
        /// Seeks any reference to a numbered variable in IL code.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        /// <param name="variableIndex">Index of the variable</param>
        /// <returns>A reference to the variable</returns>
        private VariableDefinition seekReferenceTo(ILContext il, int startingPoint, int variableIndex) {
            ILCursor cursor = new ILCursor(il);
            cursor.Index = startingPoint;
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldloc_S && ((VariableDefinition) instr.Operand).Index == variableIndex)) {
                return (VariableDefinition) cursor.Next.Operand;
            }
            return null;
        }

        private delegate void TwoRefVectorParameters(ref Vector2 one, ref Vector2 two);

        private static void applyUpsideDownEffect(ref Vector2 paddingVector, ref Vector2 positionVector) {
            paddingVector = zoomLevelVariant.getScreenPosition(paddingVector);

            if (isUpsideDown()) {
                paddingVector.Y = -paddingVector.Y;
                positionVector.Y = 90f - (positionVector.Y - 90f);
            }
        }

        private void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            Input.Aim.InvertedY = (Input.GliderMoveY.Inverted = (Input.MoveY.Inverted = isUpsideDown()));
            if (inputFeather != null) {
                (inputFeather.GetValue(null) as VirtualJoystick).InvertedY = isUpsideDown();
            }

            orig(self);
        }

        private SpriteEffects applyUpsideDownEffectToSprites() {
            SpriteEffects effects = SpriteEffects.None;
            if (GetVariantValue<bool>(Variant.UpsideDown)) effects |= SpriteEffects.FlipVertically;
            if (SaveData.Instance.Assists.MirrorMode) effects |= SpriteEffects.FlipHorizontally;
            return effects;
        }

        private static void patchHDStylegroundsRendering(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld<Engine>("ScreenMatrix"))) {
                Logger.Log("ExtendedVariantMode/UpsideDown", $"Flipping HD stylegrounds upside down at {cursor.Index} in IL for HdParallax.renderForReal");

                cursor.EmitDelegate<Func<Matrix, Matrix>>(orig => {
                    if (isUpsideDown()) {
                        orig *= Matrix.CreateTranslation(0f, -Engine.Viewport.Height, 0f);
                        orig *= Matrix.CreateScale(1f, -1f, 1f);
                    }

                    return orig;
                });
            }
        }
        private static bool isUpsideDown() {
            return (bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(Variant.UpsideDown);
        }
    }
}
