using Celeste.Mod;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsSettings : EverestModuleSettings {

        public bool MasterSwitch = false;

        // ======================================

        public bool OptionsOutOfModOptionsMenuEnabled = true;

        public bool SubmenusForEachCategoryEnabled = true;

        public bool AutomaticallyResetVariants = true;

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

        [SettingIgnore]
        public bool LegacyDashSpeedBehavior { get; set; } = false;

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
        public bool DisableClimbJumping { get; set; } = false;

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
        public bool InvertGrab { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool DisableNeutralJumping { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public int ExplodeLaunchSpeed { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public bool BadelineChasersEverywhere { get; set; } = false;

        [SettingIgnore]
        public int ChaserCount { get; set; } = 1;

        [SettingIgnore]
        public bool AffectExistingChasers { get; set; } = false;

        [SettingIgnore]
        public int BadelineLag { get; set; } = 0;

        [SettingIgnore]
        public int DelayBetweenBadelines { get; set; } = 4;

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
        public int RoomBloom { get; set; } = -1;

        // ======================================

        [SettingIgnore]
        public bool OshiroEverywhere { get; set; } = false;

        [SettingIgnore]
        public int OshiroCount { get; set; } = 1;

        [SettingIgnore]
        public int ReverseOshiroCount { get; set; } = 0;

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

        [SettingIgnore]
        public bool AllowThrowingTheoOffscreen { get; set; } = false;

        [SettingIgnore]
        public bool AllowLeavingTheoBehind { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool HeldDash { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool AllStrawberriesAreGoldens { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public int DashRefillOnGroundState { get; set; } = 0;

        // ======================================

        [SettingIgnore]
        public bool EverythingIsUnderwater { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool BadelineBossesEverywhere { get; set; } = false;

        [SettingIgnore]
        public int BadelineAttackPattern { get; set; } = 0;

        [SettingIgnore]
        public bool ChangePatternsOfExistingBosses { get; set; } = false;

        [SettingIgnore]
        public bool FirstBadelineSpawnRandom { get; set; } = false;

        [SettingIgnore]
        public int BadelineBossCount { get; set; } = 1;

        [SettingIgnore]
        public int BadelineBossNodeCount { get; set; } = 1;

        // ======================================

        [SettingIgnore]
        public int GameSpeed { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int ColorGrading { get; set; } = -1;

        // ======================================

        [SettingIgnore]
        public int JellyfishEverywhere { get; set; } = 0;

        // ======================================

        [SettingIgnore]
        public bool RisingLavaEverywhere { get; set; } = false;

        [SettingIgnore]
        public int RisingLavaSpeed { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public bool InvertHorizontalControls { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool BounceEverywhere { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public int GlitchEffect { get; set; } = -1;

        // ======================================

        [SettingIgnore]
        public int SuperdashSteeringSpeed { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int ScreenShakeIntensity { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int AnxietyEffect { get; set; } = -1;

        // ======================================

        [SettingIgnore]
        public int BlurLevel { get; set; } = 0;

        // ======================================

        [SettingIgnore]
        public int ZoomLevel { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int DashDirection { get; set; } = 0;

        // ======================================

        [SettingIgnore]
        public int BackgroundBrightness { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int ForegroundEffectOpacity { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public bool DisableMadelineSpotlight { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool MadelineIsSilhouette { get; set; } = false;

        [SettingIgnore]
        public bool DashTrailAllTheTime { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool DisableClimbingUpOrDown { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public int SwimmingSpeed { get; set; } = 10;

        // ======================================

        [SettingIgnore]
        public int BoostMultiplier { get; set; } = 10;

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
        public bool DisplayEnabledVariantsToScreen { get; set; } = false;
    }
}
