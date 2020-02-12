using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class Gravity : AbstractExtendedVariant {

        private float climbJumpGrabCooldown = -1f;

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

            On.Celeste.Player.Update += modUpdate;
            On.Celeste.Player.ClimbJump += modClimbJump;
            Everest.Events.Level.OnExit += onLevelExit;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;

            On.Celeste.Player.Update -= modUpdate;
            On.Celeste.Player.ClimbJump -= modClimbJump;
            Everest.Events.Level.OnExit -= onLevelExit;

            // reset the cooldown as the OnExit hook would.
            climbJumpGrabCooldown = -1f;
        }

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // find out where the constant 900 (downward acceleration) is loaded into the stack
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(900f))) {
                Logger.Log("ExtendedVariantMode/Gravity", $"Applying gravity to constant at {cursor.Index} in CIL code for NormalUpdate");

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

                Logger.Log("ExtendedVariantMode/Gravity", $"Injecting code to be able to grab when going up on 0-gravity at {cursor.Index} in IL code for Player.NormalUpdate");

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

        private void modClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            orig(self);

            // trigger the cooldown
            climbJumpGrabCooldown = 0.25f;
        }

        private void modUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            // deplete the cooldown
            if (climbJumpGrabCooldown >= 0f)
                climbJumpGrabCooldown -= Engine.DeltaTime;
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            // reset the cooldown
            climbJumpGrabCooldown = -1f;
        }

        private bool canGrabEvenWhenGoingUp() {
            return Settings.Gravity == 0 && climbJumpGrabCooldown <= 0f;
        }

        // NOTE: Gravity also comes in play in the UpdateSprite patch of FallSpeed.
    }
}
