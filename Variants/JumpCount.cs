using Celeste;
using Celeste.Mod;
using Celeste.Mod.Helpers;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class JumpCount : AbstractExtendedVariant {

        private static FieldInfo playerDreamJump = typeof(Player).GetField("dreamJump", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo playerJumpGraceTimer = typeof(Player).GetField("jumpGraceTimer", BindingFlags.NonPublic | BindingFlags.Instance);

        private static int jumpBuffer = 0;

        private static JumpCooldown jumpCooldown;

        private static JumpCount instance;

        public JumpCount(JumpCooldown jumpCooldown) : base(variantType: typeof(int), defaultVariantValue: 1) {
            instance = this;

            DashCount.OnDashRefill += dashRefilled;
            JumpCount.jumpCooldown = jumpCooldown;
        }

        public override object ConvertLegacyVariantValue(int value) {
            if (value == 6) {
                // having "infinite jumps" be int.MaxValue makes so much more sense than having it be 6...
                return int.MaxValue;
            } else {
                return value;
            }
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += patchJumpGraceTimer;
            IL.Celeste.Player.DashUpdate += patchJumpGraceTimer;
            IL.Celeste.Player.UseRefill += modUseRefill;
            On.Celeste.Player.DreamDashEnd += modDreamDashEnd;
            On.Celeste.Player.ClimbHopBlockedCheck += modClimbHopBlockedCheck;
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
            On.Celeste.Player.ClimbHopBlockedCheck -= modClimbHopBlockedCheck;
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            IL.Celeste.Player.DreamDashUpdate -= preventDreamJumping;
        }

        private static void preventDreamJumping(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // the strategy here is "let's pretend the player didn't press Jump if their Jump Count is 0".
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Input), "Jump"), instr => instr.MatchCallvirt<VirtualButton>("get_Pressed"))) {
                Logger.Log("ExtendedVariantMode/JumpCount", $"Preventing dream jumping with 0 jumps at {cursor.Index} in IL for Player.DreamDashUpdate");

                cursor.EmitDelegate<Func<bool, bool>>(disableDreamJumps);
            }
        }
        private static bool disableDreamJumps(bool orig) {
            if (GetVariantValue<int>(Variant.JumpCount) == 0) {
                // no dream jumping!
                return false;
            }
            // we have at least 1 jump so don't change the value.
            return orig;
        }

        private static void patchJumpGraceTimer(ILContext il) {
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
                cursor.EmitDelegate<Func<float, Player, bool, bool, float>>(canJumpStatic);
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
        private static MethodReference seekReferenceToMethod(ILContext il, string methodName) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Callvirt && ((MethodReference) instr.Operand).Name.Contains(methodName))) {
                return (MethodReference) cursor.Next.Operand;
            }
            return null;
        }

        /// <summary>
        /// This hook makes dash crystals activate when refilling jumps is required.
        /// </summary>
        private static void modUseRefill(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // let's jump to if (this.Dashes < num)
            if (cursor.TryGotoNextBestFit(MoveType.After,
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

        private static bool jumpNeedsRefilling() {
            // JumpCount - 1 because the first jump is from vanilla Celeste
            return GetVariantValue<bool>(Variant.RefillJumpsOnDashRefill) && GetVariantValue<int>(Variant.JumpCount) >= 2 && jumpBuffer < GetVariantValue<int>(Variant.JumpCount) - 1;
        }

        private static void dashRefilled() {
            if (GetVariantValue<bool>(Variant.RefillJumpsOnDashRefill) && GetVariantValue<int>(Variant.JumpCount) >= 2) {
                RefillJumpBuffer();
            }
        }

        private static void refillJumpBuffer(Player player) {
            float jumpGraceTimer = (float) playerJumpGraceTimer.GetValue(player);

            if (jumpGraceTimer > 0f && GetVariantValue<bool>(Variant.ResetJumpCountOnGround)) {
                // JumpCount - 1 because the first jump is from vanilla Celeste
                jumpBuffer = GetVariantValue<int>(Variant.JumpCount) - 1;
            }
        }

        /// <summary>
        /// Refills the jump buffer, giving the maximum amount of extra jumps possible.
        /// </summary>
        /// <returns>Whether extra jumps were refilled or not.</returns>
        public static bool RefillJumpBuffer() {
            int oldJumpBuffer = jumpBuffer;
            jumpBuffer = GetVariantValue<int>(Variant.JumpCount) - 1;
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
                jumpBuffer = Math.Min(jumpBuffer, cap == -1 ? GetVariantValue<int>(Variant.JumpCount) - 1 : cap);
            }

            return oldJumpBuffer != jumpBuffer;
        }

        /// <summary>
        /// Sets or caps the jump count.
        /// </summary>
        /// <param name="jumpCount">The jump count</param>
        /// <param name="cap">If true, the jump count will be capped to the given value; otherwise, it will be set to the given value</param>
        public static void SetJumpCount(int jumpCount, bool cap) {
            if (cap) {
                jumpBuffer = Math.Min(jumpBuffer, jumpCount);
            } else {
                jumpBuffer = jumpCount;
            }
        }

        private static float canJumpStatic(float initialJumpGraceTimer, Player self, bool canWallJumpRight, bool canWallJumpLeft) {
            // Frogeline Project relies on this method being non-static
            return instance.canJump(initialJumpGraceTimer, self, canWallJumpRight, canWallJumpLeft);
        }

        /// <summary>
        /// Detour the WallJump method in order to disable it if we want.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private float canJump(float initialJumpGraceTimer, Player self, bool canWallJumpRight, bool canWallJumpLeft) {
            if (GetVariantValue<int>(Variant.JumpCount) == 0 && jumpBuffer <= 0) {
                // we disabled jumping, so let's pretend the grace timer has run out
                return 0f;
            }
            if (self.CanUnDuck && (canWallJumpLeft || canWallJumpRight || self.CollideCheck<Water>(self.Position + Vector2.UnitY * 2f))) {
                // no matter what, don't touch vanilla behavior if a wall jump or water jump is possible
                // because inserting extra jumps would kill wall jumping
                return initialJumpGraceTimer;
            }
            if (initialJumpGraceTimer > 0f || (GetVariantValue<int>(Variant.JumpCount) != int.MaxValue && jumpBuffer <= 0) || jumpCooldown.CheckCooldown()) {
                // return the default value because we don't want to change anything
                // (our jump buffer ran out, we're in cooldown, or vanilla Celeste allows jumping anyway)
                return initialJumpGraceTimer;
            }

            // consume an Extended Variant Jump(TM)
            jumpBuffer--;
            jumpCooldown.ArmCooldown();

            // be sure that the sound played is not the dream jump one.
            playerDreamJump.SetValue(self, false);

            return 1f;
        }

        private static void modDreamDashEnd(On.Celeste.Player.orig_DreamDashEnd orig, Player self) {
            orig(self);

            if (GetVariantValue<bool>(Variant.ResetJumpCountOnGround)) {
                // consistently refill jumps, whichever direction the dream dash was in.
                // without this, jumps are only refilled when the coyote jump timer is filled: it only happens on horizontal dream dashes.
                RefillJumpBuffer();
            }
        }

        private static bool modClimbHopBlockedCheck(On.Celeste.Player.orig_ClimbHopBlockedCheck orig, Player self) {
            if (orig(self)) {
                return true;
            }

            // if landing on the ground would reduce the number of jumps we have, prevent an automatic climb hop
            if (GetVariantValue<bool>(Variant.ResetJumpCountOnGround) && jumpBuffer > GetVariantValue<int>(Variant.JumpCount) - 1) {
                return true;
            }

            return false;
        }

        private static void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                // always reset the jump count when the player enters a new level (respawn, new map, etc... everything but a transition)
                RefillJumpBuffer();
            }

            if (self.Tracker.CountEntities<JumpIndicator>() == 0) {
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
