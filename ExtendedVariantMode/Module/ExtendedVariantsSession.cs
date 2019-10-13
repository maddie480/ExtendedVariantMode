using Celeste.Mod;
using System.Collections.Generic;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsSession : EverestModuleSession {
        public Dictionary<ExtendedVariantsModule.Variant, int> VariantsEnabledViaTrigger = new Dictionary<ExtendedVariantsModule.Variant, int>();
    }
}
