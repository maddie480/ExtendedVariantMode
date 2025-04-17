using Celeste.Mod;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class WallSlidingSpeed : AbstractExtendedVariant {
        public WallSlidingSpeed() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= modPlayerNormalUpdate;
        }

        private void modPlayerNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(20f))) {
                Logger.Log("ExtendedVariantMode/WallSlidingSpeed", $"Modding wall sliding speed at {cursor.Index} in IL for Player.NormalUpdate");
                cursor.EmitDelegate<Func<float, float>>(getWallSlidingSpeed);
            }
        }

        private float getWallSlidingSpeed(float orig) {
            return orig * GetVariantValue<float>(Variant.WallSlidingSpeed);
        }
    }
}
