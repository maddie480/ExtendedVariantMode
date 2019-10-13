using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class JumpHeight : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.JumpHeight;
        }

        public override void SetValue(int value) {
            Settings.JumpHeight = value;
        }

        public override void Load() {
            IL.Celeste.Player.Jump += modJump;
            IL.Celeste.Player.SuperJump += modSuperJump;
            IL.Celeste.Player.SuperWallJump += modSuperWallJump;
            IL.Celeste.Player.WallJump += modWallJump;
        }

        public override void Unload() {
            IL.Celeste.Player.Jump -= modJump;
            IL.Celeste.Player.SuperJump -= modSuperJump;
            IL.Celeste.Player.SuperWallJump -= modSuperWallJump;
            IL.Celeste.Player.WallJump -= modWallJump;
        }
        
        /// <summary>
        /// Edits the Jump method in Player (called when jumping, simply.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // the speed applied to jumping is simply -105f (negative = up). Let's multiply this with our jump height factor.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f))) {
                Logger.Log("ExtendedVariantsModule", $"Modding constant at {cursor.Index} in CIL code for Jump to make jump height editable");

                // add two instructions to multiply -105f with the "jump height factor"
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Edits the SuperJump method in Player (called when super/hyperdashing.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modSuperJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply -105f (height given by a superdash) with the jump height factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f))) {
                Logger.Log("ExtendedVariantsModule", $"Applying jump height to constant at {cursor.Index} in CIL code for SuperJump");
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Edits the WallJump method in Player (called when walljumping, obviously.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modWallJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply -105f (height given by a superdash) with the jump height factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f))) {
                Logger.Log("ExtendedVariantsModule", $"Applying jump height to constant at {cursor.Index} in CIL code for WallJump");
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Edits the SuperWallJump method in Player (called when super/hyperdashing on a wall.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modSuperWallJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply -160f (height given by a superdash) with the jump height factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-160f))) {
                Logger.Log("ExtendedVariantsModule", $"Applying jump height to constant at {cursor.Index} in CIL code for SuperWallJump");
                cursor.EmitDelegate<Func<float>>(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Returns the currently configured jump height factor.
        /// </summary>
        /// <returns>The jump height factor (1 = default jump height)</returns>
        private float determineJumpHeightFactor() {
            return Settings.JumpHeightFactor;
        }
    }
}
