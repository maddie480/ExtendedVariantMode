using Celeste.Mod;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedVariants.Variants {
    class WallSlidingSpeed : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object GetVariantValue() {
            return Settings.WallSlidingSpeed;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.WallSlidingSpeed = value / 10f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.WallSlidingSpeed = (float) value;
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
            return orig * Settings.WallSlidingSpeed;
        }
    }
}
