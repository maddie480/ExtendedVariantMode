using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;

namespace ExtendedVariants.Variants {
    class CoyoteTime : AbstractExtendedVariant {
        private ILHook hookOrigUpdate;

        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.CoyoteTime;
        }

        public override void SetValue(int value) {
            Settings.CoyoteTime = value;
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

        private void onJumpGraceTimerReset(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're hooking usages for StartJumpGraceTime instead of hooking it because there's no way hooking a 4-instruction method works without inlining issues.
            while (cursor.TryGotoNext(instr => instr.MatchCallvirt<Player>("StartJumpGraceTime"))) {
                Logger.Log("ExtendedVariantMode/CoyoteTime", $"Modding coyote time at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.EmitDelegate<Action<Player>>(p => {
                    if (Settings.CoyoteTime != 10) {
                        // default is 0.1, and 10 / 100 = 0.1
                        new DynData<Player>(p)["jumpGraceTimer"] = Settings.CoyoteTime / 100f;
                    }
                });
            }
        }

        private void modCoyoteTime(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're hooking usages for StartJumpGraceTime instead of hooking it because there's no way hooking a 4-instruction method works without inlining issues.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.1f))) {
                Logger.Log("ExtendedVariantMode/CoyoteTime", $"Modding coyote time at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<float, float>>(orig => orig * Settings.CoyoteTime / 10f);
            }
        }
    }
}
