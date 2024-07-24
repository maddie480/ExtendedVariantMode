using Celeste.Mod;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class SwimmingSpeed : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            //IL.Celeste.Player.SwimUpdate += patchSwimUpdate;
        }

        public override void Unload() {
            //IL.Celeste.Player.SwimUpdate -= patchSwimUpdate;
        }

        private void patchSwimUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(60f) || instr.MatchLdcR4(80f) || instr.MatchLdcR4(-60f))) {
                Logger.Log("ExtendedVariantMode/SwimmingSpeed", $"Patching swimming speed at {cursor.Index} in IL for Player.SwimUpdate");
                cursor.EmitDelegate<Func<float, float>>(speed => speed * GetVariantValue<float>(Variant.SwimmingSpeed));
            }
        }
    }
}
