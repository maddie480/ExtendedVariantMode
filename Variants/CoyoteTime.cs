using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class CoyoteTime : AbstractExtendedVariant {
        private static ILHook hookOrigUpdate;

        public CoyoteTime() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.BounceBlock.ShakeOffPlayer += onJumpGraceTimerReset;
            IL.Celeste.Player.DreamDashEnd += modCoyoteTime;

            hookOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update"), modCoyoteTime);
        }

        public override void Unload() {
            IL.Celeste.BounceBlock.ShakeOffPlayer -= onJumpGraceTimerReset;
            IL.Celeste.Player.DreamDashEnd -= modCoyoteTime;

            hookOrigUpdate?.Dispose();
            hookOrigUpdate = null;
        }

        private static void onJumpGraceTimerReset(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're hooking usages for StartJumpGraceTime instead of hooking it because there's no way hooking a 4-instruction method works without inlining issues.
            while (cursor.TryGotoNext(instr => instr.MatchCallvirt<Player>("StartJumpGraceTime"))) {
                Logger.Log("ExtendedVariantMode/CoyoteTime", $"Modding coyote time at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.EmitDelegate<Action<Player>>(modJumpGraceTimer);
            }
        }

        private static void modJumpGraceTimer(Player p) {
            if (GetVariantValue<float>(Variant.CoyoteTime) != 1f) {
                // default is 0.1
                p.jumpGraceTimer = GetVariantValue<float>(Variant.CoyoteTime) * 0.1f;
            }
        }

        private static void modCoyoteTime(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're hooking usages for StartJumpGraceTime instead of hooking it because there's no way hooking a 4-instruction method works without inlining issues.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.1f))) {
                Logger.Log("ExtendedVariantMode/CoyoteTime", $"Modding coyote time at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<float, float>>(modCoyoteTimeInner);
            }
        }

        private static float modCoyoteTimeInner(float orig) {
            return orig * GetVariantValue<float>(Variant.CoyoteTime);
        }
    }
}
