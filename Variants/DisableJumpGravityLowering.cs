using Celeste.Mod;
using ExtendedVariants.Module;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class DisableJumpGravityLowering : AbstractExtendedVariant {
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
            IL.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
        }

        private void modPlayerNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchCall(typeof(Math), "Abs"),
                instr => instr.MatchLdcR4(40f)
            )) {
                Logger.Log("ExtendedVariantMode/DisableJumpGravityLowering", $"Disabling jump gravity lowering at {cursor.Index} in IL for Player.NormalUpdate");

                // turn the Math.Abs(...) < 40f check into Math.Abs(...) < -1f, making it always false.
                cursor.EmitDelegate<Func<float, float>>(orig =>
                    GetVariantValue<bool>(ExtendedVariantsModule.Variant.DisableJumpGravityLowering) ? -1f : orig
                );
            }
        }
    }
}
