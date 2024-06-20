using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class StretchUpDashes : AbstractExtendedVariant {
        private static ILHook dashCoroutineHook;

        public override void Load() {
            dashCoroutineHook = new ILHook(
                typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                Player_DashCoroutine
            );
        }

        public override void Unload() {
            dashCoroutineHook?.Dispose();
        }

        private void Player_DashCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (!foundILSequence(cursor)) {
                Logger.Log(LogLevel.Error, "ExtendedVariantMode/StretchUpDashes",
                    $"Could not find IL sequence to hook in {il.Method.FullName}!");
                return;
            }

            Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/StretchUpDashes",
                $"Emitting dash stretch delegate in {il.Method.FullName} @ {cursor.Instrs[cursor.Index]}");

            // dashSpeed = doUpDashStretch(this.beforeDashSpeed, dashSpeed);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit<Player>(OpCodes.Ldfld, "beforeDashSpeed");
            cursor.Emit(OpCodes.Ldloc_S, (byte) 3);
            cursor.EmitDelegate<Func<Vector2, Vector2, Vector2>>(doUpDashStretch);
            cursor.Emit(OpCodes.Stloc_S, (byte)3);
        }

        private Vector2 doUpDashStretch(Vector2 beforeDashSpeed, Vector2 dashSpeed) {
            // the part which stretches dashes in the x direction based on velocity looks like this:
            //
            // if (Math.Sign(this.beforeDashSpeed.X) == Math.Sign(dashSpeed.X)
            //         && Math.Abs(this.beforeDashSpeed.X) > Math.Abs(dashSpeed.X)) {
            //     dashSpeed.X = this.beforeDashSpeed.X;
            // }
            //
            // we want to do basically the same thing in the Y axis
            // don't apply this to downdiags though; it'd mess up ultras and stuff (downdiags have vectors (-1, 1) or (1, 1))
            // also make sure we only do this when enabled

            if (GetVariantValue<bool>(Variant.StretchUpDashes)
                    && !(Math.Sign(dashSpeed.Y) == 1 && Math.Sign(dashSpeed.X) != 0)
                    && Math.Sign(beforeDashSpeed.Y) == Math.Sign(dashSpeed.Y)
                    && Math.Abs(beforeDashSpeed.Y) > Math.Abs(dashSpeed.Y))
                dashSpeed.Y = beforeDashSpeed.Y;
            return dashSpeed;
        }

        private static bool foundILSequence(ILCursor cursor) {
            // try to find the most fitting match in a sequence of IL
            //
            // is this probably overkill? yes.
            // did I have fun writing this? yes.

            const int maxIndexDiff = 0x10;

            Func<Instruction, bool>[] ilSequence = {
                static instr => instr.MatchLdloca(3),
                static instr => instr.MatchLdloc(1),
                static instr => instr.MatchLdflda<Player>("beforeDashSpeed"),
                static instr => instr.MatchLdfld<Vector2>("X"),
                static instr => instr.MatchStfld<Vector2>("X")
            };

            // go to each instance of the first predicate
            while (cursor.TryGotoNext(MoveType.After, ilSequence[0])) {
                int savedCursorPosition = cursor.Index;

                // then try to match the rest of the predicates, making sure
                // the cursor has not gone further than MaxIndexDiff instructions
                for (int i = 1; i < ilSequence.Length; i++) {
                    Func<Instruction, bool> matcher = ilSequence[i];
                    int beforeMoveIndex = cursor.Index;

                    if (!cursor.TryGotoNext(MoveType.After, matcher))
                        goto FailedToMatch;

                    if (cursor.Index - beforeMoveIndex > maxIndexDiff)
                        goto FailedToMatch;
                }
                // we found a match!
                cursor.MoveAfterLabels();
                return true;

            FailedToMatch:
                // we go again
                cursor.Index = savedCursorPosition;
            }

            // no match :c
            return false;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override Type GetVariantType() {
            return typeof(bool);
        }
    }
}
