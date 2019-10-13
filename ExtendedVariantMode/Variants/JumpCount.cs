using Celeste;
using Celeste.Mod;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class JumpCount : AbstractExtendedVariant {

        private static int jumpBuffer = 0;

        public JumpCount(DashCount dashCount) {
            dashCount.OnDashRefill += dashRefilled;
        }

        public override int GetDefaultValue() {
            return 1;
        }

        public override int GetValue() {
            return Settings.JumpCount;
        }

        public override void SetValue(int value) {
            Settings.JumpCount = value;
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += patchJumpGraceTimer;
            IL.Celeste.Player.DashUpdate += patchJumpGraceTimer;
            On.Celeste.Player.UseRefill += modUseRefill;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= patchJumpGraceTimer;
            IL.Celeste.Player.DashUpdate -= patchJumpGraceTimer;
            On.Celeste.Player.UseRefill -= modUseRefill;
        }

        private void patchJumpGraceTimer(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            MethodReference wallJumpCheck = seekReferenceToMethod(il, "WallJumpCheck");

            // jump to whenever jumpGraceTimer is retrieved
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Player>("jumpGraceTimer"))) {
                Logger.Log("ExtendedVariantsModule", $"Patching jump count in at {cursor.Index} in CIL code");

                // store a reference to it
                FieldReference refToJumpGraceTimer = ((FieldReference)cursor.Prev.Operand);

                // call this.WallJumpCheck(1)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldc_I4_1);
                cursor.Emit(OpCodes.Callvirt, wallJumpCheck);

                // call this.WallJumpCheck(-1)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldc_I4_M1);
                cursor.Emit(OpCodes.Callvirt, wallJumpCheck);

                // replace the jumpGraceTimer with the modded value
                cursor.EmitDelegate<Func<float, bool, bool, float>>(canJump);

                // go back to the beginning of the method
                cursor.Index = 0;
                // and add a call to RefillJumpBuffer so that we can reset the jumpBuffer even if we cannot access jumpGraceTimer (being private)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, refToJumpGraceTimer);
                cursor.EmitDelegate<Action<float>>(refillJumpBuffer);
            }
        }
        
        /// <summary>
        /// Seeks any reference to a named method (callvirt) in IL code.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        /// <param name="methodName">name of the method</param>
        /// <returns>A reference to the method</returns>
        private MethodReference seekReferenceToMethod(ILContext il, string methodName) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Callvirt && ((MethodReference)instr.Operand).Name.Contains(methodName))) {
                return (MethodReference)cursor.Next.Operand;
            }
            return null;
        }

        /// <summary>
        /// Wraps the UseRefill method, so that it returns true when crystals refill jumps.
        /// </summary>
        /// <param name="orig">The original method</param>
        /// <param name="self">The Player entity</param>
        /// <param name="twoDashes">unused</param>
        /// <returns>true if the original method returned true OR the refill also refilled dashes, false otherwise</returns>
        private bool modUseRefill(On.Celeste.Player.orig_UseRefill orig, Player self, bool twoDashes) {
            int jumpBufferBefore = jumpBuffer;

            bool origResult = orig(self, twoDashes);
            if(Settings.RefillJumpsOnDashRefill && jumpBuffer > jumpBufferBefore) {
                // break the crystal because it refilled jumps
                return true;
            }
            return origResult;
        }

        private void dashRefilled() {
            if (Settings.RefillJumpsOnDashRefill && Settings.JumpCount >= 2) {
                refillJumpBuffer(1f);
            }
        }

        private void refillJumpBuffer(float jumpGraceTimer) {
            // JumpCount - 1 because the first jump is from vanilla Celeste
            if (jumpGraceTimer > 0f) jumpBuffer = Settings.JumpCount - 1;
        }

        /// <summary>
        /// Detour the WallJump method in order to disable it if we want.
        /// </summary>
        private float canJump(float initialJumpGraceTimer, bool canWallJumpRight, bool canWallJumpLeft) {
            if(Settings.JumpCount == 0) {
                // we disabled jumping, so let's pretend the grace timer has run out
                return 0f;
            }
            if(canWallJumpLeft || canWallJumpRight) {
                // no matter what, don't touch vanilla behavior if a wall jump is possible
                // because inserting extra jumps would kill wall jumping
                return initialJumpGraceTimer;
            }
            if(Settings.JumpCount == 6) {
                // infinite jumping, yay
                return 1f;
            }
            if(Settings.JumpCount == 1 || initialJumpGraceTimer > 0f || jumpBuffer <= 0) {
                // return the default value because we don't want to change anything 
                // (we are disabled, our jump buffer ran out, or vanilla Celeste allows jumping anyway)
                return initialJumpGraceTimer;
            }
            // consume an Extended Variant Jump(TM)
            jumpBuffer--;
            return 1f;
        }
    }
}
