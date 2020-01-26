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
            IL.Celeste.Player.UseRefill += modUseRefill;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= patchJumpGraceTimer;
            IL.Celeste.Player.DashUpdate -= patchJumpGraceTimer;
            IL.Celeste.Player.UseRefill -= modUseRefill;
        }

        private void patchJumpGraceTimer(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            MethodReference wallJumpCheck = seekReferenceToMethod(il, "WallJumpCheck");

            // jump to whenever jumpGraceTimer is retrieved
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Player>("jumpGraceTimer"))) {
                Logger.Log("ExtendedVariantMode/JumpCount", $"Patching jump count in at {cursor.Index} in CIL code");

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
        /// This hook makes dash crystals activate when refilling jumps is required.
        /// </summary>
        private void modUseRefill(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // let's jump to if (this.Dashes < num)
            if(cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchLdarg(0), // this
                instr => instr.MatchLdfld<Player>("Dashes"), // this.Dashes
                instr => instr.MatchLdloc(0), // num
                instr => instr.OpCode == OpCodes.Blt_S)) { // jump if lower than

                object branchTarget = cursor.Prev.Operand;

                Logger.Log("ExtendedVariantMode/JumpCount", $"Injecting jump refill check in dash refill check at {cursor.Index} in IL code for UseRefill");

                cursor.EmitDelegate<Func<bool>>(jumpNeedsRefilling);
                cursor.Emit(OpCodes.Brtrue_S, branchTarget);
            }
        }

        private bool jumpNeedsRefilling() {
            // JumpCount - 1 because the first jump is from vanilla Celeste
            return Settings.RefillJumpsOnDashRefill && Settings.JumpCount >= 2 && jumpBuffer < Settings.JumpCount - 1;
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

        public bool RefillJumpBuffer() {
            int oldJumpBuffer = jumpBuffer;
            jumpBuffer = Settings.JumpCount - 1;
            return oldJumpBuffer != jumpBuffer;
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
