using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class InvertDashes : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
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
                Logger.Log("ExtendedVariantMode/InvertDashes", $"Adding code to apply Invert Dashes at index {cursor.Index} in CIL code for CallDashEvents");

                // just add a call to ModifyDashSpeed (arg 0 = this)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Player>>(invertDashSpeed);
            }
        }

        /// <summary>
        /// Inverts the dash direction of the player.
        /// </summary>
        /// <param name="self">A reference to the player</param>
        private void invertDashSpeed(Player self) {
            if (GetVariantValue<bool>(Variant.InvertDashes)) {
                self.Speed *= -1;
                self.DashDir *= -1;
            }
        }
    }
}
