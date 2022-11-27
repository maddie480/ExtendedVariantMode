using Celeste.Mod;
using ExtendedVariants.Variants;
using System.Collections.Generic;
using static ExtendedVariants.Variants.DisplaySpeedometer;
using static ExtendedVariants.Variants.DontRefillDashOnGround;
using static ExtendedVariants.Variants.JungleSpidersEverywhere;
using static ExtendedVariants.Variants.MadelineBackpackMode;
using static ExtendedVariants.Variants.WindEverywhere;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsSettings : EverestModuleSettings {

        public bool MasterSwitch = false;

        // ======================================

        public bool AutoResetVariantsOnFirstStartup = true;

        // ======================================

        public bool OptionsOutOfModOptionsMenuEnabled = true;

        public bool SubmenusForEachCategoryEnabled = true;

        public bool AutomaticallyResetVariants = true;

        // ======================================

        [SettingIgnore]
        public float Gravity { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float FallSpeed { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float JumpHeight { get; set; } = 1f;

        [SettingIgnore]
        public float JumpDuration { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float SpeedX { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public int Stamina { get; set; } = 110;

        // ======================================

        [SettingIgnore]
        public float DashSpeed { get; set; } = 1f;

        [SettingIgnore]
        public bool LegacyDashSpeedBehavior { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public int DashCount { get; set; } = -1;

        // ======================================

        [SettingIgnore]
        public int CornerCorrection { get; set; } = 4;

        // ======================================

        [SettingIgnore]
        public bool DisableDashCooldown { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public float Friction { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float AirFriction { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public bool DisableWallJumping { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool DisableClimbJumping { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool DisableJumpingOutOfWater { get; set; } = false;

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
        public float HyperdashSpeed { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float WallBouncingSpeed { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float WallSlidingSpeed { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float DashLength { get; set; } = 1f;

        [SettingIgnore]
        public float DashTimerMultiplier { get; set; } = 1f;

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
        public float ExplodeLaunchSpeed { get; set; } = 1f;

        [SettingIgnore]
        public bool DisableSuperBoosts { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public float HorizontalSpringBounceDuration { get; set; } = 1f;

        [SettingIgnore]
        public float HorizontalWallJumpDuration { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public bool BadelineChasersEverywhere { get; set; } = false;

        [SettingIgnore]
        public int ChaserCount { get; set; } = 1;

        [SettingIgnore]
        public bool AffectExistingChasers { get; set; } = false;

        [SettingIgnore]
        public float BadelineLag { get; set; } = 1.55f;

        [SettingIgnore]
        public float DelayBetweenBadelines { get; set; } = 0.4f;

        // ======================================

        [SettingIgnore]
        public float RegularHiccups { get; set; } = 0f;

        [SettingIgnore]
        public float HiccupStrength { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float RoomLighting { get; set; } = -1f;

        // ======================================

        [SettingIgnore]
        public float RoomBloom { get; set; } = -1f;

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
        public WindPattern WindEverywhere { get; set; } = WindPattern.Default;


        // ======================================

        [SettingIgnore]
        public bool SnowballsEverywhere { get; set; } = false;

        [SettingIgnore]
        public float SnowballDelay { get; set; } = 0.8f;


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
        public DashRefillOnGroundConfiguration DashRefillOnGroundState { get; set; } = DashRefillOnGroundConfiguration.DEFAULT;
        [SettingIgnore]
        public bool DontRefillStaminaOnGround { get; set; } = false;

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
        public float GameSpeed { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public string ColorGrading { get; set; } = "";

        // ======================================

        [SettingIgnore]
        public int JellyfishEverywhere { get; set; } = 0;

        // ======================================

        [SettingIgnore]
        public bool RisingLavaEverywhere { get; set; } = false;

        [SettingIgnore]
        public float RisingLavaSpeed { get; set; } = 1;

        // ======================================

        [SettingIgnore]
        public bool InvertHorizontalControls { get; set; } = false;
        [SettingIgnore]
        public bool InvertVerticalControls { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool BounceEverywhere { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public SpiderType JungleSpidersEverywhere { get; set; } = SpiderType.Disabled;

        // ======================================

        [SettingIgnore]
        public float GlitchEffect { get; set; } = -1f;

        // ======================================

        [SettingIgnore]
        public float SuperdashSteeringSpeed { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float ScreenShakeIntensity { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float AnxietyEffect { get; set; } = -1;

        // ======================================

        [SettingIgnore]
        public float BlurLevel { get; set; } = 0;

        [SettingIgnore]
        public float BackgroundBlurLevel { get; set; } = 0;

        // ======================================

        [SettingIgnore]
        public float ZoomLevel { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public bool[][] AllowedDashDirections { get; set; } = new bool[][] { new bool[] { true, true, true }, new bool[] { true, true, true }, new bool[] { true, true, true } };

        // ======================================

        [SettingIgnore]
        public float BackgroundBrightness { get; set; } = 1;

        // ======================================

        [SettingIgnore]
        public float ForegroundEffectOpacity { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public bool DisableMadelineSpotlight { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool DisableKeysSpotlight { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool MadelineIsSilhouette { get; set; } = false;
        [SettingIgnore]
        public bool MadelineHasPonytail { get; set; } = false;

        [SettingIgnore]
        public bool DashTrailAllTheTime { get; set; } = false;

        [SettingIgnore]
        public MadelineBackpackModes MadelineBackpackMode { get; set; } = MadelineBackpackModes.Default;

        // ======================================

        [SettingIgnore]
        public DisableClimbingUpOrDown.ClimbUpOrDownOptions DisableClimbingUpOrDown { get; set; } = Variants.DisableClimbingUpOrDown.ClimbUpOrDownOptions.Disabled;

        // ======================================

        [SettingIgnore]
        public float PickupDuration { get; set; } = 1f;

        [SettingIgnore]
        public float MinimumDelayBeforeThrowing { get; set; } = 1f;

        [SettingIgnore]
        public float DelayBeforeRegrabbing { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float SwimmingSpeed { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public float BoostMultiplier { get; set; } = 1;

        // ======================================

        [SettingIgnore]
        public bool FriendlyBadelineFollower { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool DisableRefillsOnScreenTransition { get; set; } = false;

        [SettingIgnore]
        public bool RestoreDashesOnRespawn { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool DisplayDashCount { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool EveryJumpIsUltra { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public float CoyoteTime { get; set; } = 1f;

        // ======================================

        [SettingIgnore]
        public bool NoFreezeFrames { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public bool PreserveExtraDashesUnderwater { get; set; } = true;

        // ======================================

        [SettingIgnore]
        public bool AlwaysInvisible { get; set; } = false;

        // ======================================

        [SettingIgnore]
        public SpeedometerConfiguration DisplaySpeedometer { get; set; } = SpeedometerConfiguration.DISABLED;

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
