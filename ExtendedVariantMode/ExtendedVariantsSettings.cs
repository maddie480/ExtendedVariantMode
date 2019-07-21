using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Celeste.Mod.ExtendedVariants {
    public class ExtendedVariantsSettings : EverestModuleSettings {

        public bool MasterSwitch = false;

        // ======================================

        [SettingIgnore]
        public int Gravity { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float GravityFactor => Gravity / 10f;

        // ======================================

        [SettingIgnore]
        public int JumpHeight { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float JumpHeightFactor => JumpHeight / 10f;

        // ======================================

        [SettingIgnore]
        public int SpeedX { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float SpeedXFactor => SpeedX / 10f;

        // ======================================

        [SettingIgnore]
        public int Stamina { get; set; } = 11;

        // ======================================

        [SettingIgnore]
        public int DashSpeed { get; set; } = 10;

        [YamlIgnore]
        [SettingIgnore]
        public float DashSpeedFactor => DashSpeed / 10f;

        // ======================================

        [SettingIgnore]
        public int DashCount { get; set; } = -1;

        // ======================================

        [SettingIgnore]
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

        // ======================================

        public Dictionary<int, Dictionary<Variant, int>> OverrideValuesInSave = new Dictionary<int, Dictionary<Variant, int>>();
    }
}
