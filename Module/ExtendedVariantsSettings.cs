using Celeste.Mod;
using ExtendedVariants.UI;
using static ExtendedVariants.Module.ExtendedVariantsModule;
using System.Collections.Generic;
using ExtendedVariants.Variants;
using YamlDotNet.Serialization;

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

        // === CelesteTAS Set-Command Support === //

        private object GetVariant(Variant variant) => Instance.TriggerManager.GetCurrentVariantValue(variant);
        private void SetVariant(Variant variant, object value) => ModOptionsEntries.SetVariantValue(variant, value);

        // Movement - Vertical speed
        [YamlIgnore]
        [SettingIgnore]
        public float Gravity {
            get => (float)GetVariant(Variant.Gravity);
            set => SetVariant(Variant.Gravity, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float FallSpeed {
            get => (float)GetVariant(Variant.FallSpeed);
            set => SetVariant(Variant.FallSpeed, value);
        }

        // Movement - Jumping
        [YamlIgnore]
        [SettingIgnore]
        public float JumpHeight {
            get => (float)GetVariant(Variant.JumpHeight);
            set => SetVariant(Variant.JumpHeight, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float JumpDuration {
            get => (float)GetVariant(Variant.JumpDuration);
            set => SetVariant(Variant.JumpDuration, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float WallBouncingSpeed {
            get => (float)GetVariant(Variant.WallBouncingSpeed);
            set => SetVariant(Variant.WallBouncingSpeed, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableWallJumping {
            get => (bool)GetVariant(Variant.DisableWallJumping);
            set => SetVariant(Variant.DisableWallJumping, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableClimbJumping {
            get => (bool)GetVariant(Variant.DisableClimbJumping);
            set => SetVariant(Variant.DisableClimbJumping, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableJumpingOutOfWater {
            get => (bool)GetVariant(Variant.DisableJumpingOutOfWater);
            set => SetVariant(Variant.DisableJumpingOutOfWater, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableNeutralJumping {
            get => (bool)GetVariant(Variant.DisableNeutralJumping);
            set => SetVariant(Variant.DisableNeutralJumping, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int WallJumpDistance {
            get => (int)GetVariant(Variant.WallJumpDistance);
            set => SetVariant(Variant.WallJumpDistance, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int WallBounceDistance {
            get => (int)GetVariant(Variant.WallBounceDistance);
            set => SetVariant(Variant.WallBounceDistance, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float HorizontalWallJumpDuration {
            get => (float)GetVariant(Variant.HorizontalWallJumpDuration);
            set => SetVariant(Variant.HorizontalWallJumpDuration, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int JumpCount {
            get => (int)GetVariant(Variant.JumpCount);
            set => SetVariant(Variant.JumpCount, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool RefillJumpsOnDashRefill {
            get => (bool)GetVariant(Variant.RefillJumpsOnDashRefill);
            set => SetVariant(Variant.RefillJumpsOnDashRefill, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool ResetJumpCountOnGround {
            get => (bool)GetVariant(Variant.ResetJumpCountOnGround);
            set => SetVariant(Variant.ResetJumpCountOnGround, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float JumpCooldown {
            get => (float)GetVariant(Variant.JumpCooldown);
            set => SetVariant(Variant.JumpCooldown, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool EveryJumpIsUltra {
            get => (bool)GetVariant(Variant.EveryJumpIsUltra);
            set => SetVariant(Variant.EveryJumpIsUltra, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float CoyoteTime {
            get => (float)GetVariant(Variant.CoyoteTime);
            set => SetVariant(Variant.CoyoteTime, value);
        }

        // Movement - Dashing
        [YamlIgnore]
        [SettingIgnore]
        public float DashSpeed {
            get => (float)GetVariant(Variant.DashSpeed);
            set => SetVariant(Variant.DashSpeed, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool LegacyDashSpeedBehavior {
            get => (bool)GetVariant(Variant.LegacyDashSpeedBehavior);
            set => SetVariant(Variant.LegacyDashSpeedBehavior, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float DashLength {
            get => (float)GetVariant(Variant.DashLength);
            set => SetVariant(Variant.DashLength, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float DashTimerMultiplier {
            get => (float)GetVariant(Variant.DashTimerMultiplier);
            set => SetVariant(Variant.DashTimerMultiplier, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool[][] DashDirection {
            get => (bool[][])GetVariant(Variant.DashDirection);
            set => SetVariant(Variant.DashDirection, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float HyperdashSpeed {
            get => (float)GetVariant(Variant.HyperdashSpeed);
            set => SetVariant(Variant.HyperdashSpeed, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float SuperdashSteeringSpeed {
            get => (float)GetVariant(Variant.SuperdashSteeringSpeed);
            set => SetVariant(Variant.SuperdashSteeringSpeed, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float UltraSpeedMultiplier {
            get => (float)GetVariant(Variant.UltraSpeedMultiplier);
            set => SetVariant(Variant.UltraSpeedMultiplier, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int DashCount {
            get => (int)GetVariant(Variant.DashCount);
            set => SetVariant(Variant.DashCount, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool HeldDash {
            get => (bool)GetVariant(Variant.HeldDash);
            set => SetVariant(Variant.HeldDash, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public DontRefillDashOnGround.DashRefillOnGroundConfiguration DontRefillDashOnGround {
            get => (DontRefillDashOnGround.DashRefillOnGroundConfiguration)GetVariant(Variant.DontRefillDashOnGround);
            set => SetVariant(Variant.DontRefillDashOnGround, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public DashRestriction.DashRestrictionType DashRestriction {
            get => (DashRestriction.DashRestrictionType)GetVariant(Variant.DashRestriction);
            set => SetVariant(Variant.DashRestriction, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableRefillsOnScreenTransition {
            get => (bool)GetVariant(Variant.DisableRefillsOnScreenTransition);
            set => SetVariant(Variant.DisableRefillsOnScreenTransition, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DontRefillStaminaOnGround {
            get => (bool)GetVariant(Variant.DontRefillStaminaOnGround);
            set => SetVariant(Variant.DontRefillStaminaOnGround, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool RestoreDashesOnRespawn {
            get => (bool)GetVariant(Variant.RestoreDashesOnRespawn);
            set => SetVariant(Variant.RestoreDashesOnRespawn, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool PreserveExtraDashesUnderwater {
            get => (bool)GetVariant(Variant.PreserveExtraDashesUnderwater);
            set => SetVariant(Variant.PreserveExtraDashesUnderwater, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableDashCooldown {
            get => (bool)GetVariant(Variant.DisableDashCooldown);
            set => SetVariant(Variant.DisableDashCooldown, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int CornerCorrection {
            get => (int)GetVariant(Variant.CornerCorrection);
            set => SetVariant(Variant.CornerCorrection, value);
        }

        // Movement - Moving around
        [YamlIgnore]
        [SettingIgnore]
        public float SpeedX {
            get => (float)GetVariant(Variant.SpeedX);
            set => SetVariant(Variant.SpeedX, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float SwimmingSpeed {
            get => (float)GetVariant(Variant.SwimmingSpeed);
            set => SetVariant(Variant.SwimmingSpeed, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float Friction {
            get => (float)GetVariant(Variant.Friction);
            set => SetVariant(Variant.Friction, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float AirFriction {
            get => (float)GetVariant(Variant.AirFriction);
            set => SetVariant(Variant.AirFriction, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float ExplodeLaunchSpeed {
            get => (float)GetVariant(Variant.ExplodeLaunchSpeed);
            set => SetVariant(Variant.ExplodeLaunchSpeed, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float WallSlidingSpeed {
            get => (float)GetVariant(Variant.WallSlidingSpeed);
            set => SetVariant(Variant.WallSlidingSpeed, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableSuperBoosts {
            get => (bool)GetVariant(Variant.DisableSuperBoosts);
            set => SetVariant(Variant.DisableSuperBoosts, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float BoostMultiplier {
            get => (float)GetVariant(Variant.BoostMultiplier);
            set => SetVariant(Variant.BoostMultiplier, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public DisableClimbingUpOrDown.ClimbUpOrDownOptions DisableClimbingUpOrDown {
            get => (DisableClimbingUpOrDown.ClimbUpOrDownOptions)GetVariant(Variant.DisableClimbingUpOrDown);
            set => SetVariant(Variant.DisableClimbingUpOrDown, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float HorizontalSpringBounceDuration {
            get => (float)GetVariant(Variant.HorizontalSpringBounceDuration);
            set => SetVariant(Variant.HorizontalSpringBounceDuration, value);
        }
        
        // Movement - Holdable items
        [YamlIgnore]
        [SettingIgnore]
        public float PickupDuration {
            get => (float)GetVariant(Variant.PickupDuration);
            set => SetVariant(Variant.PickupDuration, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float MinimumDelayBeforeThrowing {
            get => (float)GetVariant(Variant.MinimumDelayBeforeThrowing);
            set => SetVariant(Variant.MinimumDelayBeforeThrowing, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float DelayBeforeRegrabbing {
            get => (float)GetVariant(Variant.DelayBeforeRegrabbing);
            set => SetVariant(Variant.DelayBeforeRegrabbing, value);
        }

        // Game Elements - Badeline Chasers
        [YamlIgnore]
        [SettingIgnore]
        public bool BadelineChasersEverywhere {
            get => (bool)GetVariant(Variant.BadelineChasersEverywhere);
            set => SetVariant(Variant.BadelineChasersEverywhere, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int ChaserCount {
            get => (int)GetVariant(Variant.ChaserCount);
            set => SetVariant(Variant.ChaserCount, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool AffectExistingChasers {
            get => (bool)GetVariant(Variant.AffectExistingChasers);
            set => SetVariant(Variant.AffectExistingChasers, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float BadelineLag {
            get => (float)GetVariant(Variant.BadelineLag);
            set => SetVariant(Variant.BadelineLag, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float DelayBetweenBadelines {
            get => (float)GetVariant(Variant.DelayBetweenBadelines);
            set => SetVariant(Variant.DelayBetweenBadelines, value);
        }

        // Game Elements - Badeline Bosses
        [YamlIgnore]
        [SettingIgnore]
        public bool BadelineBossesEverywhere {
            get => (bool)GetVariant(Variant.BadelineBossesEverywhere);
            set => SetVariant(Variant.BadelineBossesEverywhere, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int BadelineAttackPattern {
            get => (int)GetVariant(Variant.BadelineAttackPattern);
            set => SetVariant(Variant.BadelineAttackPattern, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool ChangePatternsOfExistingBosses {
            get => (bool)GetVariant(Variant.ChangePatternsOfExistingBosses);
            set => SetVariant(Variant.ChangePatternsOfExistingBosses, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool FirstBadelineSpawnRandom {
            get => (bool)GetVariant(Variant.FirstBadelineSpawnRandom);
            set => SetVariant(Variant.FirstBadelineSpawnRandom, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int BadelineBossCount {
            get => (int)GetVariant(Variant.BadelineBossCount);
            set => SetVariant(Variant.BadelineBossCount, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int BadelineBossNodeCount {
            get => (int)GetVariant(Variant.BadelineBossNodeCount);
            set => SetVariant(Variant.BadelineBossNodeCount, value);
        }

        // Game Elements - Oshiro
        [YamlIgnore]
        [SettingIgnore]
        public bool OshiroEverywhere {
            get => (bool)GetVariant(Variant.OshiroEverywhere);
            set => SetVariant(Variant.OshiroEverywhere, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int OshiroCount {
            get => (int)GetVariant(Variant.OshiroCount);
            set => SetVariant(Variant.OshiroCount, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableOshiroSlowdown {
            get => (bool)GetVariant(Variant.DisableOshiroSlowdown);
            set => SetVariant(Variant.DisableOshiroSlowdown, value);
        }

        // Game Elements - Theo Crystals
        [YamlIgnore]
        [SettingIgnore]
        public bool TheoCrystalsEverywhere {
            get => (bool)GetVariant(Variant.TheoCrystalsEverywhere);
            set => SetVariant(Variant.TheoCrystalsEverywhere, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool AllowThrowingTheoOffscreen {
            get => (bool)GetVariant(Variant.AllowThrowingTheoOffscreen);
            set => SetVariant(Variant.AllowThrowingTheoOffscreen, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool AllowLeavingTheoBehind {
            get => (bool)GetVariant(Variant.AllowLeavingTheoBehind);
            set => SetVariant(Variant.AllowLeavingTheoBehind, value);
        }

        // Game Elements - Other
        [YamlIgnore]
        [SettingIgnore]
        public WindEverywhere.WindPattern   WindEverywhere {
            get => (WindEverywhere.WindPattern)GetVariant(Variant.WindEverywhere);
            set => SetVariant(Variant.WindEverywhere, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool SnowballsEverywhere {
            get => (bool)GetVariant(Variant.SnowballsEverywhere);
            set => SetVariant(Variant.SnowballsEverywhere, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float SnowballDelay {
            get => (float)GetVariant(Variant.SnowballDelay);
            set => SetVariant(Variant.SnowballDelay, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int AddSeekers {
            get => (int)GetVariant(Variant.AddSeekers);
            set => SetVariant(Variant.AddSeekers, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableSeekerSlowdown {
            get => (bool)GetVariant(Variant.DisableSeekerSlowdown);
            set => SetVariant(Variant.DisableSeekerSlowdown, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int JellyfishEverywhere {
            get => (int)GetVariant(Variant.JellyfishEverywhere);
            set => SetVariant(Variant.JellyfishEverywhere, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool RisingLavaEverywhere {
            get => (bool)GetVariant(Variant.RisingLavaEverywhere);
            set => SetVariant(Variant.RisingLavaEverywhere, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float RisingLavaSpeed {
            get => (float)GetVariant(Variant.RisingLavaSpeed);
            set => SetVariant(Variant.RisingLavaSpeed, value);
        }

        // Visual - Madeline
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableMadelineSpotlight {
            get => (bool)GetVariant(Variant.DisableMadelineSpotlight);
            set => SetVariant(Variant.DisableMadelineSpotlight, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DashTrailAllTheTime {
            get => (bool)GetVariant(Variant.DashTrailAllTheTime);
            set => SetVariant(Variant.DashTrailAllTheTime, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisplayDashCount {
            get => (bool)GetVariant(Variant.DisplayDashCount);
            set => SetVariant(Variant.DisplayDashCount, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public DisplaySpeedometer.SpeedometerConfiguration DisplaySpeedometer {
            get => (DisplaySpeedometer.SpeedometerConfiguration)GetVariant(Variant.DisplaySpeedometer);
            set => SetVariant(Variant.DisplaySpeedometer, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public MadelineBackpackMode.MadelineBackpackModes MadelineBackpackMode {
            get => (MadelineBackpackMode.MadelineBackpackModes)GetVariant(Variant.MadelineBackpackMode);
            set => SetVariant(Variant.MadelineBackpackMode, value);
        }

        // Visual - Level
        [YamlIgnore]
        [SettingIgnore]
        public bool UpsideDown {
            get => (bool)GetVariant(Variant.UpsideDown);
            set => SetVariant(Variant.UpsideDown, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float RoomLighting {
            get => (float)GetVariant(Variant.RoomLighting);
            set => SetVariant(Variant.RoomLighting, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float BackgroundBrightness {
            get => (float)GetVariant(Variant.BackgroundBrightness);
            set => SetVariant(Variant.BackgroundBrightness, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float ForegroundEffectOpacity {
            get => (float)GetVariant(Variant.ForegroundEffectOpacity);
            set => SetVariant(Variant.ForegroundEffectOpacity, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float RoomBloom {
            get => (float)GetVariant(Variant.RoomBloom);
            set => SetVariant(Variant.RoomBloom, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float GlitchEffect {
            get => (float)GetVariant(Variant.GlitchEffect);
            set => SetVariant(Variant.GlitchEffect, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float AnxietyEffect {
            get => (float)GetVariant(Variant.AnxietyEffect);
            set => SetVariant(Variant.AnxietyEffect, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float BlurLevel {
            get => (float)GetVariant(Variant.BlurLevel);
            set => SetVariant(Variant.BlurLevel, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float BackgroundBlurLevel {
            get => (float)GetVariant(Variant.BackgroundBlurLevel);
            set => SetVariant(Variant.BackgroundBlurLevel, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float ZoomLevel {
            get => (float)GetVariant(Variant.ZoomLevel);
            set => SetVariant(Variant.ZoomLevel, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public string ColorGrading {
            get => (string)GetVariant(Variant.ColorGrading);
            set => SetVariant(Variant.ColorGrading, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool DisableKeysSpotlight {
            get => (bool)GetVariant(Variant.DisableKeysSpotlight);
            set => SetVariant(Variant.DisableKeysSpotlight, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public SpinnerColor.Color SpinnerColor {
            get => (SpinnerColor.Color)GetVariant(Variant.SpinnerColor);
            set => SetVariant(Variant.SpinnerColor, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float ScreenShakeIntensity {
            get => (float)GetVariant(Variant.ScreenShakeIntensity);
            set => SetVariant(Variant.ScreenShakeIntensity, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool FriendlyBadelineFollower {
            get => (bool)GetVariant(Variant.FriendlyBadelineFollower);
            set => SetVariant(Variant.FriendlyBadelineFollower, value);
        }

        // Gameplay Tweaks
        [YamlIgnore]
        [SettingIgnore]
        public float GameSpeed {
            get => (float)GetVariant(Variant.GameSpeed);
            set => SetVariant(Variant.GameSpeed, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool NoFreezeFrames {
            get => (bool)GetVariant(Variant.NoFreezeFrames);
            set => SetVariant(Variant.NoFreezeFrames, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool EverythingIsUnderwater {
            get => (bool)GetVariant(Variant.EverythingIsUnderwater);
            set => SetVariant(Variant.EverythingIsUnderwater, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public int Stamina {
            get => (int)GetVariant(Variant.Stamina);
            set => SetVariant(Variant.Stamina, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float RegularHiccups {
            get => (float)GetVariant(Variant.RegularHiccups);
            set => SetVariant(Variant.RegularHiccups, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public float HiccupStrength {
            get => (float)GetVariant(Variant.HiccupStrength);
            set => SetVariant(Variant.HiccupStrength, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool AllStrawberriesAreGoldens {
            get => (bool)GetVariant(Variant.AllStrawberriesAreGoldens);
            set => SetVariant(Variant.AllStrawberriesAreGoldens, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool AlwaysInvisible {
            get => (bool)GetVariant(Variant.AlwaysInvisible);
            set => SetVariant(Variant.AlwaysInvisible, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool CorrectedMirrorMode {
            get => (bool)GetVariant(Variant.CorrectedMirrorMode);
            set => SetVariant(Variant.CorrectedMirrorMode, value);
        }

        // Gameplay Tweaks - Trolls
        [YamlIgnore]
        [SettingIgnore]
        public bool ForceDuckOnGround {
            get => (bool)GetVariant(Variant.ForceDuckOnGround);
            set => SetVariant(Variant.ForceDuckOnGround, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool InvertDashes {
            get => (bool)GetVariant(Variant.InvertDashes);
            set => SetVariant(Variant.InvertDashes, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool InvertGrab {
            get => (bool)GetVariant(Variant.InvertGrab);
            set => SetVariant(Variant.InvertGrab, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool InvertHorizontalControls {
            get => (bool)GetVariant(Variant.InvertHorizontalControls);
            set => SetVariant(Variant.InvertHorizontalControls, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool InvertVerticalControls {
            get => (bool)GetVariant(Variant.InvertVerticalControls);
            set => SetVariant(Variant.InvertVerticalControls, value);
        }
        [YamlIgnore]
        [SettingIgnore]
        public bool BounceEverywhere {
            get => (bool)GetVariant(Variant.BounceEverywhere);
            set => SetVariant(Variant.BounceEverywhere, value);
        }
    }
}
