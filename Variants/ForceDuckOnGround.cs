using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class ForceDuckOnGround : AbstractExtendedVariant {
        public ForceDuckOnGround() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
        }

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.)
        /// to handle the "force duck on ground" variant.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to "if(this.Ducking)" => that's a brfalse
            // (or, in the FNA version, "bool ducking = this.Ducking;" => that's a stloc.s)
            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchCallvirt<Player>("get_Ducking"),
                instr => (instr.OpCode == OpCodes.Brfalse || instr.OpCode == OpCodes.Stloc_S))
                // in the XNA version, we get after the brfalse. In the FNA version, we have a ldloc.s and a brfalse after the cursor
                && (cursor.Prev.OpCode == OpCodes.Brfalse || cursor.Next.Next.OpCode == OpCodes.Brfalse)) {

                if (cursor.Prev.OpCode == OpCodes.Stloc_S) {
                    // get after the brfalse in order to line up with the XNA version
                    cursor.Index += 2;
                }

                Logger.Log("ExtendedVariantMode/ForceDuckOnGround", $"Inserting condition to enforce Force Duck On Ground at {cursor.Index} in CIL code for NormalUpdate");

                ILLabel target = (ILLabel) cursor.Prev.Operand;

                // basically, this turns the if into "if(this.Ducking && !GetVariantValue(Variant.ForceDuckOnGround))": this prevents unducking
                cursor.EmitDelegate<Func<bool>>(ForceDuckOnGroundEnabled);
                cursor.Emit(OpCodes.Brtrue, target);

                // jump to the "else" to modify this one too
                cursor.GotoLabel(target);

                // set ourselves just before the condition we want to mod
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdsfld(typeof(Input), "MoveY"))) {
                    ILCursor cursorAfterCondition = cursor.Clone();

                    if (cursorAfterCondition.TryGotoNext(MoveType.After, instr => (instr.OpCode == OpCodes.Bne_Un || instr.OpCode == OpCodes.Bne_Un_S))) {
                        Logger.Log("ExtendedVariantMode/ForceDuckOnGround", $"Inserting condition to enforce Force Duck On Ground at {cursor.Index} in CIL code for NormalUpdate");

                        // so this is basically "if (this.onGround && (GetVariantValue(Variant.ForceDuckOnGround) || Input.MoveY == 1) && this.Speed.Y >= 0f)"
                        // by telling IL "if GetVariantValue(Variant.ForceDuckOnGround) is true, jump over the Input.MoveY check"
                        cursor.EmitDelegate<Func<bool>>(ForceDuckOnGroundEnabled);
                        cursor.Emit(OpCodes.Brtrue, cursorAfterCondition.Next);
                    }
                }
            }
        }

        private bool ForceDuckOnGroundEnabled() => GetVariantValue<bool>(Variant.ForceDuckOnGround);
    }
}
