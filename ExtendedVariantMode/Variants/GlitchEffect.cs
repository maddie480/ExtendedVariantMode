using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class GlitchEffect : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return -1f;
        }

        public override object GetVariantValue() {
            return Settings.GlitchEffect;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.GlitchEffect = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            if (value == -1) {
                Settings.GlitchEffect = -1f;
            } else {
                Settings.GlitchEffect = (value / 20f);
            }
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
            if (Settings.GlitchEffect == -1f) return vanilla;
            return Settings.GlitchEffect;
        }
    }
}
