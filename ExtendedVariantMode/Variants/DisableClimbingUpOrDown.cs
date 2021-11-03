using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class DisableClimbingUpOrDown : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.DisableClimbingUpOrDown ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.DisableClimbingUpOrDown = (value != 0);
        }

        public override void Load() {
            IL.Celeste.Player.ClimbUpdate += onPlayerClimbUpdate;
        }

        public override void Unload() {
            IL.Celeste.Player.ClimbUpdate -= onPlayerClimbUpdate;
        }

        private void onPlayerClimbUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdsfld(typeof(Input), "MoveY"),
                instr => instr.MatchLdfld<VirtualIntegerAxis>("Value"))) {

                Logger.Log("ExtendedVariantMode/DisableClimbingUpOrDown", $"Modifying MoveY to prevent player from moving @ {cursor.Index} in IL for Player.ClimbUpdate");
                cursor.EmitDelegate<Func<int, int>>(orig => {
                    if (Settings.DisableClimbingUpOrDown) {
                        return 0; // player isn't holding up or down...
                    }
                    return orig;
                });
            }
        }
    }
}
