using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class RoomBloom : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return -1f;
        }

        public override object GetVariantValue() {
            return Settings.RoomBloom;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.RoomBloom = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            if (value == -1) {
                Settings.RoomBloom = -1f;
            } else if (value < 10f) {
                // 8 means 80% bloom...
                Settings.RoomBloom = (value / 10f);
            } else {
                // ... but 14 means 500% bloom, not 140%. Legacy values ftw
                Settings.RoomBloom = (value - 9f);
            }
        }

        public override void Load() {
            IL.Celeste.BloomRenderer.Apply += onBloomRendererApply;
        }

        public override void Unload() {
            IL.Celeste.BloomRenderer.Apply -= onBloomRendererApply;
        }

        private void onBloomRendererApply(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<BloomRenderer>("Base"))) {
                Logger.Log("ExtendedVariantMode/RoomBloom", $"Modding bloom base at {cursor.Index} in IL code for BloomRenderer.Apply");

                cursor.EmitDelegate<Func<float, float>>(modBloomBase);
            }

            cursor.Index = 0;

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<BloomRenderer>("Strength"))) {
                Logger.Log("ExtendedVariantMode/RoomBloom", $"Modding bloom strength at {cursor.Index} in IL code for BloomRenderer.Apply");

                cursor.EmitDelegate<Func<float, float>>(modBloomStrength);
            }
        }

        private float modBloomBase(float vanilla) {
            if (Settings.RoomBloom == -1f) return vanilla;
            return Math.Min(Settings.RoomBloom, 1f);
        }

        private float modBloomStrength(float vanilla) {
            if (Settings.RoomBloom == -1f) return vanilla;
            return Math.Max(1, Settings.RoomBloom);
        }
    }
}
