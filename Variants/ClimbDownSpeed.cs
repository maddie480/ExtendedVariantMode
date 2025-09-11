using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class ClimbDownSpeed : AbstractExtendedVariant {
        public ClimbDownSpeed() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Player.ClimbUpdate += onPlayerClimbUpdate;
        }

        public override void Unload() {
            IL.Celeste.Player.ClimbUpdate -= onPlayerClimbUpdate;
        }

        private void onPlayerClimbUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(80f))) {
                Logger.Log("ExtendedVariantMode/ClimbDownSpeed", $"Modifying num to set the downwards climbing speed @ {cursor.Index} in IL for Player.ClimbUpdate");
                cursor.EmitDelegate<Func<float>>(() => {
                    return GetVariantValue<float>(Variant.ClimbDownSpeed);
                });
                cursor.Emit(OpCodes.Mul);
            }
        }
    }
}
