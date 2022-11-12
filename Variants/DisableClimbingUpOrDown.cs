using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class DisableClimbingUpOrDown : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.DisableClimbingUpOrDown;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DisableClimbingUpOrDown = (bool) value;
        }

        public override void SetLegacyVariantValue(int value) {
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
