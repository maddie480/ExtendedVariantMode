using YamlDotNet.Serialization;

namespace Celeste.Mod.ExtendedVariants {
    public class ExtendedVariantsSettings : EverestModuleSettings {
        [SettingRange(1, 30)]
        public int Gravity { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float GravityFactor => Gravity / 10f;

        [SettingRange(1, 30)]
        public int SpeedX { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float SpeedXFactor => SpeedX / 10f;

        [SettingRange(0, 30)]
        public int Stamina { get; set; } = 11;

        public int DashCount { get; set; } = -1;

        /// <summary>
        /// Create a selector with -1 displayed as "Default".
        /// </summary>
        /// <param name="menu">The menu to add the option in</param>
        /// <param name="inGame">true if in-game menu, false otherwise</param>
        public void CreateDashCountEntry(TextMenu menu, bool inGame) {
            menu.Add(new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHCOUNT"), i => {
                if(i == -1) {
                    return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                }
                return i.ToString();
            }, -1, 5, DashCount).Change(i => DashCount = i));
        }
    }
}
