using Celeste.Mod;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsSession : EverestModuleSession {
        public Dictionary<ExtendedVariantsModule.Variant, object> VariantsEnabledViaTrigger = new Dictionary<ExtendedVariantsModule.Variant, object>();
        public HashSet<ExtendedVariantsModule.Variant> VariantsOverridenByUser = new HashSet<ExtendedVariantsModule.Variant>();

        public bool ExtendedVariantsWereUsed { get; set; } = false;

        public int DashCountOnLatestRespawn { get; set; } = -1;

        public bool ExtendedVariantsDisplayedOnScreenViaTrigger { get; set; } = false;

        // Those are extended variant trigger manager variables that should not be saved, but should be reset with the session.
        public Dictionary<ExtendedVariantsModule.Variant, object> OverriddenVariantsInRoom { get; set; } = new Dictionary<ExtendedVariantsModule.Variant, object>();
        public Dictionary<ExtendedVariantsModule.Variant, object> OverriddenVariantsInRoomRevertOnLeave { get; set; } = new Dictionary<ExtendedVariantsModule.Variant, object>();
    }
}
