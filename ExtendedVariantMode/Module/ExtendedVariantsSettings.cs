using Celeste.Mod;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsSettings : EverestModuleSettings {

        public bool MasterSwitch = false;

        // ======================================

        [SettingIgnore]
        public int Gravity { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int FallSpeed { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int JumpHeight { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int SpeedX { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int Stamina { get; set; } = 11;

        // ======================================

        [SettingIgnore]
        public int DashSpeed { get; set; } = 10;
        
        // ======================================

        [SettingIgnore]
        public int DashCount { get; set; } = -1;

        // ======================================

        [SettingIgnore]
        public int Friction { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int AirFriction { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public bool DisableWallJumping { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public int JumpCount { get; set; } = 1;

        [SettingIgnore]
        public bool RefillJumpsOnDashRefill { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool UpsideDown { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public int HyperdashSpeed { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int WallBouncingSpeed { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int DashLength { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public bool ForceDuckOnGround { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool InvertDashes { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool DisableNeutralJumping { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool BadelineChasersEverywhere { get; set; } = false;

        [SettingIgnore]
        public int ChaserCount { get; set; } = 1;

        [SettingIgnore]
        public bool AffectExistingChasers { get; set; } = false;

        [SettingIgnore]
        public int BadelineLag { get; set; } = 0;

        // ======================================

        [SettingIgnore]
        public int RegularHiccups { get; set; } = 0;

        [SettingIgnore]
        public int HiccupStrength { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int RoomLighting { get; set; } = -1;

        // ======================================

        [SettingIgnore]
        public bool OshiroEverywhere { get; set; } = false;

        [SettingIgnore]
        public bool DisableOshiroSlowdown { get; set; } = false;


        // ======================================

        [SettingIgnore]
        public int WindEverywhere { get; set; } = 0;


        // ======================================

        [SettingIgnore]
        public bool SnowballsEverywhere { get; set; } = false;

        [SettingIgnore]
        public int SnowballDelay { get; set; } = 8;


        // ======================================

        [SettingIgnore]
        public int AddSeekers { get; set; } = 0;

        [SettingIgnore]
        public bool DisableSeekerSlowdown { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool TheoCrystalsEverywhere { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool HeldDash { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool AllStrawberriesAreGoldens { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool DontRefillDashOnGround { get; set; } = false;


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
        public int Vanillafy { get; set; } = 0;

        [SettingIgnore]
        public bool FileOutput { get; set; } = false;
    }
}
