using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;

namespace ExtendedVariants.Variants {
    public class CoyoteTime : AbstractExtendedVariant {
        private ILHook hookOrigUpdate;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object GetVariantValue() {
            return Settings.CoyoteTime;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.CoyoteTime = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.CoyoteTime = (value / 10f);
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
                    if (Settings.CoyoteTime != 1f) {
                        // default is 0.1
                        new DynData<Player>(p)["jumpGraceTimer"] = Settings.CoyoteTime * 0.1f;
                    }
                });
            }
        }

        private void modCoyoteTime(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're hooking usages for StartJumpGraceTime instead of hooking it because there's no way hooking a 4-instruction method works without inlining issues.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.1f))) {
                Logger.Log("ExtendedVariantMode/CoyoteTime", $"Modding coyote time at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<float, float>>(orig => orig * Settings.CoyoteTime);
            }
        }
    }
}
