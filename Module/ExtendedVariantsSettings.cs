using Celeste.Mod;
using System.Collections.Generic;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsSettings : EverestModuleSettings {
        [SettingIgnore]
        public bool MasterSwitch { get; set; } = false;

        [SettingIgnore]
        public Dictionary<ExtendedVariantsModule.Variant, object> EnabledVariants { get; set; } = new Dictionary<ExtendedVariantsModule.Variant, object>();

        // ======================================

        [SettingIgnore]
        public bool ChangeVariantsRandomly { get; set; } = false;

        [SettingIgnore]
        public int VariantSet { get; set; } = 3;

        [SettingIgnore]
        public int ChangeVariantsInterval { get; set; } = 0;

        [SettingIgnore]
        public Dictionary<string, bool> RandomizerEnabledVariants { get; set; } = new Dictionary<string, bool>();

        [SettingIgnore]
        public bool RerollMode { get; set; } = false;

        [SettingIgnore]
        public int MaxEnabledVariants { get; set; } = 10;

        [SettingIgnore]
        public string RandoSetSeed { get; set; } = null;

        [SettingIgnore]
        public int Vanillafy { get; set; } = 0;

        [SettingIgnore]
        public bool DisplayEnabledVariantsToScreen { get; set; } = false;
    }
}
