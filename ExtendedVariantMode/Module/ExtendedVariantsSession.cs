using Celeste.Mod;
using System.Collections.Generic;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsSession : EverestModuleSession {
        public Dictionary<Variant, int> VariantsEnabledViaTrigger = new Dictionary<Variant, int>();
    }
}
