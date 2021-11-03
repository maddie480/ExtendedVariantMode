using Celeste.Mod;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class SwimmingSpeed : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.SwimmingSpeed;
        }

        public override void SetValue(int value) {
            Settings.SwimmingSpeed = value;
        }

        public override void Load() {
            IL.Celeste.Player.SwimUpdate += patchSwimUpdate;
        }

        public override void Unload() {
            IL.Celeste.Player.SwimUpdate -= patchSwimUpdate;
        }

        private void patchSwimUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(60f) || instr.MatchLdcR4(80f) || instr.MatchLdcR4(-60f))) {
                Logger.Log("ExtendedVariantMode/SwimmingSpeed", $"Patching swimming speed at {cursor.Index} in IL for Player.SwimUpdate");
                cursor.EmitDelegate<Func<float, float>>(speed => speed * Settings.SwimmingSpeed / 10);
            }
        }
    }
}
