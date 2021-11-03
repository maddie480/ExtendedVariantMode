using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class GlitchEffect : AbstractExtendedVariant {

        public override int GetDefaultValue() {
            return -1;
        }

        public override int GetValue() {
            return Settings.GlitchEffect;
        }

        public override void SetValue(int value) {
            Settings.GlitchEffect = value;
        }

        public override void Load() {
            IL.Celeste.Glitch.Apply += modGlitchApply;
        }

        public override void Unload() {
            IL.Celeste.Glitch.Apply -= modGlitchApply;
        }

        private void modGlitchApply(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // instead of messing with Glitch.Value, we replace it in the actual rendering method.
            // this way, it has no impact on the rest of the game, and will revert to the intended value when disabled.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Glitch), "Value"))) {
                Logger.Log("ExtendedVariantMode/GlitchEffect", $"Patching glitch value at {cursor.Index} in IL code for Glitch.Apply");
                cursor.EmitDelegate<Func<float, float>>(modGlitchValue);
            }
        }

        private float modGlitchValue(float vanilla) {
            if (Settings.GlitchEffect == -1) return vanilla;
            return Settings.GlitchEffect / 20f;
        }
    }
}
