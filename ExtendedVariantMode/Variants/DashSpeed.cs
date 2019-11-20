using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class DashSpeed : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.DashSpeed;
        }

        public override void SetValue(int value) {
            Settings.DashSpeed = value;
        }

        public override void Load() {
            IL.Celeste.Player.CallDashEvents += modCallDashEvents;
        }

        public override void Unload() {
            IL.Celeste.Player.CallDashEvents -= modCallDashEvents;
        }
        
        /// <summary>
        /// Edits the CallDashEvents method in Player (called multiple times when the player dashes).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modCallDashEvents(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // enter the if in the method (the "if" checks if dash events were already called) and inject ourselves in there
            // (those are actually brtrue in the XNA version and brfalse in the FNA version. Seriously?)
            if (cursor.TryGotoNext(MoveType.After, instr => (instr.OpCode == OpCodes.Brtrue || instr.OpCode == OpCodes.Brfalse))) {
                Logger.Log("ExtendedVariantMode/DashSpeed", $"Adding code to mod dash speed at index {cursor.Index} in CIL code for CallDashEvents");

                // just add a call to ModifyDashSpeed (arg 0 = this)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Player>>(modifyDashSpeed);
            }
        }

        /// <summary>
        /// Modifies the dash speed of the player.
        /// </summary>
        /// <param name="self">A reference to the player</param>
        private void modifyDashSpeed(Player self) {
            self.Speed *= Settings.DashSpeed / 10f;
        }
    }
}
