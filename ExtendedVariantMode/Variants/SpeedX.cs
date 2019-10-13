using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class SpeedX : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.SpeedX;
        }

        public override void SetValue(int value) {
            Settings.SpeedX = value;
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
            IL.Celeste.Player.SuperJump += modSuperJump;
            IL.Celeste.Player.SuperWallJump += modSuperWallJump;
            IL.Celeste.Player.WallJump += modWallJump;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
            IL.Celeste.Player.SuperJump -= modSuperJump;
            IL.Celeste.Player.SuperWallJump -= modSuperWallJump;
            IL.Celeste.Player.WallJump -= modWallJump;
        }
        
        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.)
        /// to handle the X speed part.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we use 90 as an anchor (an "if" before the instruction we want to mod loads 90 in the stack)
            // then we jump to the next usage of V_6 to get the reference to it (no idea how to build it otherwise)
            // (actually, this is V_31 in the FNA version)
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(90f))
                && cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Stloc_S
                    && (((VariableDefinition)instr.Operand).Index == 6 || ((VariableDefinition)instr.Operand).Index == 31))) {

                VariableDefinition variable = (VariableDefinition) cursor.Next.Operand;

                // we jump before the next ldflda, which is between the "if (this.level.InSpace)" and the next one
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldflda)) {
                    Logger.Log("ExtendedVariantsModule", $"Applying X speed modding to variable {variable.ToString()} at {cursor.Index} in CIL code for NormalUpdate");

                    // pop ldarg.0
                    cursor.Emit(OpCodes.Pop);

                    // modify variable 6 to apply X factor
                    cursor.Emit(OpCodes.Ldloc_S, variable);
                    cursor.EmitDelegate<Func<float>>(determineSpeedXFactor);
                    cursor.Emit(OpCodes.Mul);
                    cursor.Emit(OpCodes.Stloc_S, variable);

                    // execute ldarg.0 again
                    cursor.Emit(OpCodes.Ldarg_0);
                }
            }
        }

        /// <summary>
        /// Edits the SuperJump method in Player (called when super/hyperdashing.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modSuperJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply 260f (speed given by a superdash) with the X speed factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(260f))) {
                Logger.Log("ExtendedVariantsModule", $"Applying X speed to constant at {cursor.Index} in CIL code for SuperJump");
                cursor.EmitDelegate<Func<float>>(determineSpeedXFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }
        
        /// <summary>
        /// Edits the SuperJump method in Player (called when super/hyperdashing on a wall.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modSuperWallJump(ILContext il)  {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply 170f (X speed given by a superdash) with the X speed factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(170f))) {
                Logger.Log("ExtendedVariantsModule", $"Applying X speed to constant at {cursor.Index} in CIL code for SuperWallJump");
                cursor.EmitDelegate<Func<float>>(determineSpeedXFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Edits the WallJump method in Player (called when walljumping, obviously.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modWallJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply 130f (X speed given by a walljump) with the X speed factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f))) {
                Logger.Log("ExtendedVariantsModule", $"Applying X speed to constant at {cursor.Index} in CIL code for WallJump");
                cursor.EmitDelegate<Func<float>>(determineSpeedXFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Returns the currently configured X speed factor.
        /// </summary>
        /// <returns>The speed factor (1 = default speed)</returns>
        private float determineSpeedXFactor() {
            return Settings.SpeedXFactor;
        }

        // NOTE: X speed also comes in play for the UpdateSprite patch in Friction.
    }
}
