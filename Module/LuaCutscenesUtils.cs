using ExtendedVariants.Variants;
using MonoMod.ModInterop;
using System;

namespace ExtendedVariants.Module {
    // A class providing some methods that can be called from Lua cutscenes, by doing:
    // local luaCutscenesUtils = require("#ExtendedVariants.Module.LuaCutscenesUtils")
    // luaCutscenesUtils.TriggerIntegerVariant(...)
    [ModExportName("ExtendedVariantMode")]
    public static class LuaCutscenesUtils {
        public static object GetCurrentVariantValue(string variantString) {
            ExtendedVariantsModule.Variant variant = (ExtendedVariantsModule.Variant) Enum.Parse(typeof(ExtendedVariantsModule.Variant), variantString);
            return ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(variant);
        }

        public static void TriggerIntegerVariant(string variant, int newValue, bool revertOnDeath) {
            TriggerVariant(variant, newValue, revertOnDeath);
        }

        public static void TriggerBooleanVariant(string variant, bool newValue, bool revertOnDeath) {
            TriggerVariant(variant, newValue, revertOnDeath);
        }

        public static void TriggerFloatVariant(string variant, float newValue, bool revertOnDeath) {
            TriggerVariant(variant, newValue, revertOnDeath);
        }

        public static void TriggerVariant(string variantString, object newValue, bool revertOnDeath) {
            ExtendedVariantsModule.Variant variant = (ExtendedVariantsModule.Variant) Enum.Parse(typeof(ExtendedVariantsModule.Variant), variantString);
            ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(variant, newValue, revertOnLeave: false, isFade: false, revertOnDeath, legacy: false);
        }

        public static void SetJumpCount(int jumpCount) {
            JumpCount.SetJumpCount(jumpCount, cap: false);
        }

        public static void CapJumpCount(int jumpCount) {
            JumpCount.SetJumpCount(jumpCount, cap: true);
        }

        public static int GetJumpCount() {
            return JumpCount.GetJumpBuffer();
        }
    }
}
