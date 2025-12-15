using Celeste;
using Celeste.Mod;
using Celeste.Mod.Helpers;
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

        private static void Player_DashCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            Func<Instruction, bool>[] ilSequence = {
                static instr => instr.MatchLdloca(3),
                static instr => instr.MatchLdloc(1),
                static instr => instr.MatchLdflda<Player>("beforeDashSpeed"),
                static instr => instr.MatchLdfld<Vector2>("X"),
                static instr => instr.MatchStfld<Vector2>("X")
            };

            if (!cursor.TryGotoNextBestFit(MoveType.After, ilSequence)) {
                Logger.Log(LogLevel.Error, "ExtendedVariantMode/StretchUpDashes",
                    $"Could not find IL sequence to hook in {il.Method.FullName}!");
                return;
            }
            cursor.MoveAfterLabels();

            Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/StretchUpDashes",
                $"Emitting dash stretch delegate in {il.Method.FullName} @ {cursor.Instrs[cursor.Index]}");

            // dashSpeed = doUpDashStretch(this.beforeDashSpeed, dashSpeed);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit<Player>(OpCodes.Ldfld, "beforeDashSpeed");
            cursor.Emit(OpCodes.Ldloc_S, (byte) 3);
            cursor.EmitDelegate<Func<Vector2, Vector2, Vector2>>(doUpDashStretch);
            cursor.Emit(OpCodes.Stloc_S, (byte)3);
        }

        private static Vector2 doUpDashStretch(Vector2 beforeDashSpeed, Vector2 dashSpeed) {
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

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public StretchUpDashes() : base(variantType: typeof(bool), defaultVariantValue: false) { }
    }
}
