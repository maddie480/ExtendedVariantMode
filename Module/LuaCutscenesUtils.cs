using System;

namespace ExtendedVariants.Module {
    // A class providing some methods that can be called from Lua cutscenes, by doing:
    // local luaCutscenesUtils = require("#ExtendedVariants.Module.LuaCutscenesUtils")
    // luaCutscenesUtils.TriggerIntegerVariant(...)
    public static class LuaCutscenesUtils {
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
            ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(variant, newValue, false, false, revertOnDeath, false);
        }
    }
}
