using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class UpsideDown : AbstractExtendedVariant {
        private static ZoomLevel zoomLevelVariant;

        public UpsideDown(ZoomLevel zoomLevel) {
            zoomLevelVariant = zoomLevel;
        }

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.UpsideDown ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.UpsideDown = (value != 0);
        }

        public override void Load() {
            IL.Celeste.Level.Render += modLevelRender;
        }

        public override void Unload() {
            IL.Celeste.Level.Render -= modLevelRender;

            // be sure the controls are not upside down anymore
            Input.Aim.InvertedY = (Input.GliderMoveY.Inverted = (Input.MoveY.Inverted = false));
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
            Input.Aim.InvertedY = (Input.GliderMoveY.Inverted = (Input.MoveY.Inverted = ExtendedVariantsModule.Settings.UpsideDown));

            paddingVector = zoomLevelVariant.getScreenPosition(paddingVector);

            if (ExtendedVariantsModule.Settings.UpsideDown) {
                paddingVector.Y = -paddingVector.Y;
                positionVector.Y = 90f - (positionVector.Y - 90f);
            }
        }

        private SpriteEffects applyUpsideDownEffectToSprites() {
            SpriteEffects effects = SpriteEffects.None;
            if (Settings.UpsideDown) effects |= SpriteEffects.FlipVertically;
            if (SaveData.Instance.Assists.MirrorMode) effects |= SpriteEffects.FlipHorizontally;
            return effects;
        }
    }
}
