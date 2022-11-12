using Celeste.Mod;
using System.Collections.Generic;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsSession : EverestModuleSession {
        public Dictionary<ExtendedVariantsModule.Variant, object> VariantsEnabledViaTrigger = new Dictionary<ExtendedVariantsModule.Variant, object>();

        public bool ExtendedVariantsWereUsed { get; set; } = false;

        public int DashCountOnLatestRespawn { get; set; } = -1;

        public bool ExtendedVariantsDisplayedOnScreenViaTrigger { get; set; } = false;
    }
}
