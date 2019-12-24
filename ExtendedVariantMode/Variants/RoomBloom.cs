using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class RoomBloom : AbstractExtendedVariant {

        public override int GetDefaultValue() {
            return -1;
        }

        public override int GetValue() {
            return Settings.RoomBloom;
        }

        public override void SetValue(int value) {
            Settings.RoomBloom = value;
        }

        public override void Load() {
            IL.Celeste.BloomRenderer.Apply += onBloomRendererApply;
        }

        public override void Unload() {
            IL.Celeste.BloomRenderer.Apply -= onBloomRendererApply;
        }

        private void onBloomRendererApply(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<BloomRenderer>("Base"))) {
                Logger.Log("ExtendedVariantMode/RoomBloom", $"Modding bloom base at {cursor.Index} in IL code for BloomRenderer.Apply");

                cursor.EmitDelegate<Func<float, float>>(modBloomBase);
            }

            cursor.Index = 0;

            while(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<BloomRenderer>("Strength"))) {
                Logger.Log("ExtendedVariantMode/RoomBloom", $"Modding bloom strength at {cursor.Index} in IL code for BloomRenderer.Apply");

                cursor.EmitDelegate<Func<float, float>>(modBloomStrength);
            }
        }

        private float modBloomBase(float vanilla) {
            if (Settings.RoomBloom == -1) return vanilla;
            return Math.Min(Settings.RoomBloom, 10) / 10f;
        }

        private float modBloomStrength(float vanilla) {
            if (Settings.RoomBloom == -1) return vanilla;
            return Math.Max(1, Settings.RoomBloom - 9);
        }
    }
}
