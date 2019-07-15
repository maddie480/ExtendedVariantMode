using YamlDotNet.Serialization;

namespace Celeste.Mod.ExtendedVariants {
    public class ExtendedVariantsSettings : EverestModuleSettings {
        [SettingRange(1, 30)]
        public int Gravity { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float GravityFactor => Gravity / 10f;

        [SettingRange(0, 30)]
        public int Stamina { get; set; } = 11;
    }
}
