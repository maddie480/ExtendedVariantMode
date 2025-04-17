using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class RoomBloom : AbstractExtendedVariant {

        public RoomBloom() : base(variantType: typeof(float), defaultVariantValue: -1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            if (value == -1) {
                return -1f;
            } else if (value < 10f) {
                // 8 means 80% bloom...
                return value / 10f;
            } else {
                // ... but 14 means 500% bloom, not 140%. Legacy values ftw
                return value - 9f;
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
            if (GetVariantValue<float>(Variant.RoomBloom) == -1f) return vanilla;
            return Math.Min(GetVariantValue<float>(Variant.RoomBloom), 1f);
        }

        private float modBloomStrength(float vanilla) {
            if (GetVariantValue<float>(Variant.RoomBloom) == -1f) return vanilla;
            return Math.Max(1, GetVariantValue<float>(Variant.RoomBloom));
        }
    }
}
