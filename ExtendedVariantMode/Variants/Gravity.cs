using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class Gravity : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.Gravity;
        }

        public override void SetValue(int value) {
            Settings.Gravity = value;
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
        }

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // find out where the constant 900 (downward acceleration) is loaded into the stack
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(900f))) {
                Logger.Log("ExtendedVariantsModule", $"Applying gravity to constant at {cursor.Index} in CIL code for NormalUpdate");

                // add two instructions to multiply those constants with the "gravity factor"
                cursor.EmitDelegate<Func<float>>(determineGravityFactor);
                cursor.Emit(OpCodes.Mul);
            }

            cursor.Index = 0;

            // let's jump to if (this.Speed.Y < 0f) => "is the player going up? if so, they can't grab!"
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Input), "Grab")) &&
                cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdarg(0), // this
                instr => instr.MatchLdflda<Player>("Speed"),
                instr => instr.MatchLdfld<Vector2>("Y"),
                instr => instr.MatchLdcR4(0f),
                instr => instr.OpCode == OpCodes.Blt_Un || instr.OpCode == OpCodes.Blt_Un_S)) {

                Instruction afterCheck = cursor.Next;

                // step back before the "Speed.Y < 0f" check (more specifically, inside it. it would be skipped otherwise)
                cursor.Index -= 4;

                Logger.Log("ExtendedVariantsModule", $"Injecting code to be able to grab when going up on 0-gravity at {cursor.Index} in IL code for Player.NormalUpdate");

                // pop this, inject ourselves to jump over the "Speed.Y < 0f" check, and put this back
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate<Func<bool>>(canGrabEvenWhenGoingUp);
                cursor.Emit(OpCodes.Brtrue, afterCheck);
                cursor.Emit(OpCodes.Ldarg_0);
            }
        }

        /// <summary>
        /// Returns the currently configured gravity factor.
        /// </summary>
        /// <returns>The gravity factor (1 = default gravity)</returns>
        private float determineGravityFactor() {
            return Settings.Gravity / 10f;
        }

        private bool canGrabEvenWhenGoingUp() {
            return Settings.Gravity == 0;
        }

        // NOTE: Gravity also comes in play in the UpdateSprite patch of FallSpeed.
    }
}
