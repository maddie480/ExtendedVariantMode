using System;
using YamlDotNet.Serialization;

namespace Celeste.Mod.ExtendedVariants {
    public class ExtendedVariantsSettings : EverestModuleSettings {
        public int Gravity { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float GravityFactor => Gravity / 10f;

        /// <summary>
        /// Create a selector displaying a factor instead of the actual value.
        /// </summary>
        /// <param name="menu">The menu to add the option in</param>
        /// <param name="inGame">true if in-game menu, false otherwise</param>
        public void CreateGravityEntry(TextMenu menu, bool inGame) {
            menu.Add(new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_GRAVITY"),
                i => $"{i / 10f:f1}x", 1, 30, Gravity).Change(i => Gravity = i));
        }

        // ======================================

        public int JumpHeight { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float JumpHeightFactor => JumpHeight / 10f;

        /// <summary>
        /// Create a selector displaying a factor instead of the actual value.
        /// </summary>
        /// <param name="menu">The menu to add the option in</param>
        /// <param name="inGame">true if in-game menu, false otherwise</param>
        public void CreateJumpHeightEntry(TextMenu menu, bool inGame) {
            menu.Add(new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_JUMPHEIGHT"),
                i => $"{i / 10f:f1}x", 0, 30, JumpHeight).Change(i => JumpHeight = i));
        }

        // ======================================

        public int SpeedX { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float SpeedXFactor => SpeedX / 10f;

        /// <summary>
        /// Create a selector displaying a factor instead of the actual value.
        /// </summary>
        /// <param name="menu">The menu to add the option in</param>
        /// <param name="inGame">true if in-game menu, false otherwise</param>
        public void CreateSpeedXEntry(TextMenu menu, bool inGame) {
            menu.Add(new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SPEEDX"),
                i => $"{i / 10f:f1}x", 1, 30, SpeedX).Change(i => SpeedX = i));
        }

        // ======================================

        public int Stamina { get; set; } = 11;

        /// <summary>
        /// Create a selector displaying a value multiplied by 10 (to match the internal value).
        /// </summary>
        /// <param name="menu">The menu to add the option in</param>
        /// <param name="inGame">true if in-game menu, false otherwise</param>
        public void CreateStaminaEntry(TextMenu menu, bool inGame) {
            menu.Add(new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_STAMINA"),
                i => $"{i * 10}", 0, 30, Stamina).Change(i => Stamina = i));
        }

        // ======================================

        public int DashSpeed { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float DashSpeedFactor => DashSpeed / 10f;

        /// <summary>
        /// Create a selector displaying a factor instead of the actual value.
        /// </summary>
        /// <param name="menu">The menu to add the option in</param>
        /// <param name="inGame">true if in-game menu, false otherwise</param>
        public void CreateDashSpeedEntry(TextMenu menu, bool inGame) {
            menu.Add(new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHSPEED"),
                i => $"{i / 10f:f1}x", 0, 30, DashSpeed).Change(i => DashSpeed = i));
        }

        // ======================================

        public int DashCount { get; set; } = -1;

        /// <summary>
        /// Create a selector with -1 displayed as "Default".
        /// </summary>
        /// <param name="menu">The menu to add the option in</param>
        /// <param name="inGame">true if in-game menu, false otherwise</param>
        public void CreateDashCountEntry(TextMenu menu, bool inGame) {
            menu.Add(new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHCOUNT"), i => {
                if (i == -1) {
                    return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                }
                return i.ToString();
            }, -1, 5, DashCount).Change(i => DashCount = i));
        }

        // ======================================

        public int Friction { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float FrictionFactor {
            get {
                switch (Friction) {
                    case -1: return 0f;
                    case 0: return 0.05f ;
                    default: return Friction / 10f;
                }
            }
        }

        /// <summary>
        /// Create a selector displaying a factor instead of the actual value.
        /// </summary>
        /// <param name="menu">The menu to add the option in</param>
        /// <param name="inGame">true if in-game menu, false otherwise</param>
        public void CreateFrictionEntry(TextMenu menu, bool inGame) {
            menu.Add(new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FRICTION"),
                i => {
                    switch(i) {
                        case -1: return "0.0x";
                        case 0: return "0.05x";
                        default: return $"{i / 10f:f1}x";
                    }
                }, -1, 30, Friction).Change(i => Friction = i));
        }
    }
}
