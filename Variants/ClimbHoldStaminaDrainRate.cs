using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class ClimbHoldStaminaDrainRate : AbstractExtendedVariant {
        public ClimbHoldStaminaDrainRate() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

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

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(10f))) {
                Logger.Log("ExtendedVariantMode/ClimbHoldStaminaDrainRate", $"Applying multiplier to Stamina @ {cursor.Index} in IL for Player.ClimbUpdate");
                cursor.EmitDelegate<Func<float>>(() => GetVariantValue<float>(Variant.ClimbHoldStaminaDrainRate));
                cursor.Emit(OpCodes.Mul);
            }
        }
    }
}
