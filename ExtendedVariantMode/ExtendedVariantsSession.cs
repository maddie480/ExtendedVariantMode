using System.Collections.Generic;

namespace Celeste.Mod.ExtendedVariants {
    public class ExtendedVariantsSession : EverestModuleSession {
        public Dictionary<Variant, int> VariantsEnabledViaTrigger = new Dictionary<Variant, int>();
    }
}
