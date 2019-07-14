using YamlDotNet.Serialization;

namespace Celeste.Mod.ExtendedVariants
{
    [SettingName("MODOPTIONS_EXTENDEDVARIANTSMODULE_TITLE")]
    public class ExtendedVariantsSettings : EverestModuleSettings
    {
        [SettingName("MODOPTIONS_EXTENDEDVARIANTSMODULE_GRAVITY")]
        [SettingRange(1, 30)]
        public int Gravity { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float GravityFactor => Gravity / 10f;
    }
}
