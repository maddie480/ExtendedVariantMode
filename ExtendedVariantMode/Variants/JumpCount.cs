using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Linq;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class JumpCount : AbstractExtendedVariant {

        private static FieldInfo playerDreamJump = typeof(Player).GetField("dreamJump", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo playerJumpGraceTimer = typeof(Player).GetField("jumpGraceTimer", BindingFlags.NonPublic | BindingFlags.Instance);

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
            On.Celeste.Player.DreamDashEnd += modDreamDashEnd;
            On.Celeste.Level.LoadLevel += modLoadLevel;
            IL.Celeste.Player.DreamDashUpdate += preventDreamJumping;

            // if already in a map, add the jump indicator right away.
            if (Engine.Scene is Level level) {
                level.Add(new JumpIndicator());
                level.Entities.UpdateLists();
            }
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= patchJumpGraceTimer;
            IL.Celeste.Player.DashUpdate -= patchJumpGraceTimer;
            IL.Celeste.Player.UseRefill -= modUseRefill;
            On.Celeste.Player.DreamDashEnd -= modDreamDashEnd;
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            IL.Celeste.Player.DreamDashUpdate -= preventDreamJumping;
        }

        private void preventDreamJumping(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // the strategy here is "let's pretend the player didn't press Jump if their Jump Count is 0".
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Input), "Jump"), instr => instr.MatchCallvirt<VirtualButton>("get_Pressed"))) {
                Logger.Log("ExtendedVariantMode/JumpCount", $"Preventing dream jumping with 0 jumps at {cursor.Index} in IL for Player.DreamDashUpdate");

                cursor.EmitDelegate<Func<bool, bool>>(orig => {
                    if (Settings.JumpCount == 0) {
                        // no dream jumping!
                        return false;
                    }
                    // we have at least 1 jump so don't change the value.
                    return true;
                });
            }
        }

        private void patchJumpGraceTimer(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            MethodReference wallJumpCheck = seekReferenceToMethod(il, "WallJumpCheck");

            // jump to whenever jumpGraceTimer is retrieved
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Player>("jumpGraceTimer"))) {
                Logger.Log("ExtendedVariantMode/JumpCount", $"Patching jump count in at {cursor.Index} in CIL code");

                // get "this"
                cursor.Emit(OpCodes.Ldarg_0);

                // call this.WallJumpCheck(1)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldc_I4_1);
                cursor.Emit(OpCodes.Callvirt, wallJumpCheck);

                // call this.WallJumpCheck(-1)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldc_I4_M1);
                cursor.Emit(OpCodes.Callvirt, wallJumpCheck);

                // replace the jumpGraceTimer with the modded value
                cursor.EmitDelegate<Func<float, Player, bool, bool, float>>(canJump);
            }

            // go back to the beginning of the method
            cursor.Index = 0;
            // and add a call to RefillJumpBuffer so that we can reset the jumpBuffer if normal jumps are available.
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Player>>(refillJumpBuffer);
        }

        /// <summary>
        /// Seeks any reference to a named method (callvirt) in IL code.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        /// <param name="methodName">name of the method</param>
        /// <returns>A reference to the method</returns>
        private MethodReference seekReferenceToMethod(ILContext il, string methodName) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Callvirt && ((MethodReference) instr.Operand).Name.Contains(methodName))) {
                return (MethodReference) cursor.Next.Operand;
            }
            return null;
        }

        /// <summary>
        /// This hook makes dash crystals activate when refilling jumps is required.
        /// </summary>
        private void modUseRefill(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // let's jump to if (this.Dashes < num)
            if (cursor.TryGotoNext(MoveType.After,
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
                RefillJumpBuffer();
            }
        }

        private void refillJumpBuffer(Player player) {
            float jumpGraceTimer = (float) playerJumpGraceTimer.GetValue(player);
            // JumpCount - 1 because the first jump is from vanilla Celeste
            if (jumpGraceTimer > 0f) jumpBuffer = Settings.JumpCount - 1;
        }

        /// <summary>
        /// Refills the jump buffer, giving the maximum amount of extra jumps possible.
        /// </summary>
        /// <returns>Whether extra jumps were refilled or not.</returns>
        public bool RefillJumpBuffer() {
            int oldJumpBuffer = jumpBuffer;
            jumpBuffer = Settings.JumpCount - 1;
            return oldJumpBuffer != jumpBuffer;
        }

        /// <summary>
        /// Adds more jumps to the jump counter.
        /// </summary>
        /// <param name="jumpsToAdd">The number of extra jumps to add</param>
        /// <param name="capped">true if the jump count should not exceed the cap, false if we don't care</param>
        /// <param name="cap">the maximum number of jumps to apply, -1 to use the current extended variant setting</param>
        /// <returns>Whether the jump count changed or not.</returns>
        public bool AddJumps(int jumpsToAdd, bool capped, int cap) {
            int oldJumpBuffer = jumpBuffer;

            // even if jumps are set to 0, 2-jump extra refills give back 2 extra jumps.
            jumpBuffer = Math.Max(0, jumpBuffer);
            jumpBuffer += jumpsToAdd;

            if (capped) {
                // cap the extra jump count.
                jumpBuffer = Math.Min(jumpBuffer, cap == -1 ? Settings.JumpCount - 1 : cap);
            }

            return oldJumpBuffer != jumpBuffer;
        }

        /// <summary>
        /// Detour the WallJump method in order to disable it if we want.
        /// </summary>
        private float canJump(float initialJumpGraceTimer, Player self, bool canWallJumpRight, bool canWallJumpLeft) {
            if (Settings.JumpCount == 0 && jumpBuffer <= 0) {
                // we disabled jumping, so let's pretend the grace timer has run out
                return 0f;
            }
            if (self.CanUnDuck && (canWallJumpLeft || canWallJumpRight || self.CollideCheck<Water>(self.Position + Vector2.UnitY * 2f))) {
                // no matter what, don't touch vanilla behavior if a wall jump or water jump is possible
                // because inserting extra jumps would kill wall jumping
                return initialJumpGraceTimer;
            }
            if (initialJumpGraceTimer > 0f || (Settings.JumpCount != 6 && jumpBuffer <= 0)) {
                // return the default value because we don't want to change anything 
                // (our jump buffer ran out, or vanilla Celeste allows jumping anyway)
                return initialJumpGraceTimer;
            }

            // consume an Extended Variant Jump(TM)
            jumpBuffer--;

            // be sure that the sound played is not the dream jump one.
            playerDreamJump.SetValue(self, false);

            return 1f;
        }

        private void modDreamDashEnd(On.Celeste.Player.orig_DreamDashEnd orig, Player self) {
            orig(self);

            // consistently refill jumps, whichever direction the dream dash was in.
            // without this, jumps are only refilled when the coyote jump timer is filled: it only happens on horizontal dream dashes.
            RefillJumpBuffer();
        }

        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                // always reset the jump count when the player enters a new level (respawn, new map, etc... everything but a transition)
                RefillJumpBuffer();
            }

            if (!self.Entities.Any(entity => entity is JumpIndicator)) {
                // add the entity showing the jump count
                self.Add(new JumpIndicator());
                self.Entities.UpdateLists();
            }
        }

        public static int GetJumpBuffer() {
            return jumpBuffer;
        }
    }
}
