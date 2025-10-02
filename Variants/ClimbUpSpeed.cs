using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class ClimbUpSpeed : AbstractExtendedVariant {
        public ClimbUpSpeed() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

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

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-45f))) {
                Logger.Log("ExtendedVariantMode/ClimbUpSpeed", $"Modifying num to set the upwards climbing speed @ {cursor.Index} in IL for Player.ClimbUpdate");
                cursor.EmitDelegate<Func<float>>(() => GetVariantValue<float>(Variant.ClimbUpSpeed));
                cursor.Emit(OpCodes.Mul);
            }
        }
    }
}
