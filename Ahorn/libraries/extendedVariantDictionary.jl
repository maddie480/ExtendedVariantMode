module ExtendedVariantDictionary

export IntegerVariants, FloatVariants, BooleanVariants

const IntegerVariants = [
    "AddSeekers",
    "BadelineBossCount",
    "BadelineBossNodeCount",
    "ChaserCount",
    "CornerCorrection",
    "DashCount",
    "JellyfishEverywhere",
	"MultiBuffering",
    "OshiroCount",
    "ReverseOshiroCount",
    "Stamina",
    "WallBounceDistance",
    "WallJumpDistance"
]

const FloatVariants = [
    "AirFriction",
    "AnxietyEffect",
    "BackgroundBlurLevel",
    "BackgroundBrightness",
    "BadelineLag",
    "BlurLevel",
    "BoostMultiplier",
    "CoyoteTime",
    "DashLength",
    "DashSpeed",
    "DashTimerMultiplier",
    "DelayBeforeRegrabbing",
    "DelayBetweenBadelines",
    "ExplodeLaunchSpeed",
    "FallSpeed",
    "FastFallAcceleration",
    "ForegroundEffectOpacity",
    "Friction",
    "GameSpeed",
    "GlitchEffect",
    "Gravity",
    "HiccupStrength",
    "HorizontalSpringBounceDuration",
    "HorizontalWallJumpDuration",
    "HyperdashSpeed",
    "JumpCooldown",
    "JumpDuration",
    "JumpHeight",
    "MinimumDelayBeforeThrowing",
    "PickupDuration",
    "RegularHiccups",
    "RisingLavaSpeed",
    "RoomLighting",
    "RoomBloom",
    "ScreenShakeIntensity",
    "SnowballDelay",
    "SpeedX",
    "SuperdashSteeringSpeed",
    "SwimmingSpeed",
    "UltraSpeedMultiplier",
    "WallBouncingSpeed",
    "WallSlidingSpeed",
    "ZoomLevel"
]

const BooleanVariants = [
    "AffectExistingChasers",
    "AllStrawberriesAreGoldens",
    "AllowLeavingTheoBehind",
    "AllowThrowingTheoOffscreen",
    "AlternativeBuffering",
    "AlwaysFeather",
    "AlwaysInvisible",
    "BadelineBossesEverywhere",
    "BadelineChasersEverywhere",
    "BounceEverywhere",
    "BufferableGrab",
    "ChangePatternsOfExistingBosses",
    "CornerboostProtection",
    "CorrectedMirrorMode",
    "DashBeforePickup",
    "DashTrailAllTheTime",
    "DisableClimbJumping",
    "DisableDashCooldown",
    "DisableJumpingOutOfWater",
    "DisableKeysSpotlight",
    "DisableMadelineSpotlight",
    "DisableNeutralJumping",
    "DisableOshiroSlowdown",
    "DisableRefillsOnScreenTransition",
    "DisableSeekerSlowdown",
    "DisableSuperBoosts",
    "DisableWallJumping",
    "DisplayDashCount",
    "DontRefillStaminaOnGround",
    "EveryJumpIsUltra",
    "EverythingIsUnderwater",
    "FirstBadelineSpawnRandom",
    "ForceDuckOnGround",
    "FriendlyBadelineFollower",
    "HeldDash",
    "InvertDashes",
    "InvertGrab",
    "InvertHorizontalControls",
    "InvertVerticalControls",
    "LegacyDashSpeedBehavior",
    "LiftboostProtection",
    "NoFreezeFrames",
    "OshiroEverywhere",
    "PermanentBinoStorage",
    "PermanentDashAttack",
    "PreserveExtraDashesUnderwater",
    "RefillJumpsOnDashRefill",
    "ResetJumpCountOnGround",
    "RestoreDashesOnRespawn",
    "RisingLavaEverywhere",
    "SaferDiagonalSmuggle",
    "SnowballsEverywhere",
    "TheoCrystalsEverywhere",
    "TrueNoGrabbing",
    "UltraProtection",
    "UpsideDown",
    "WalllessWallbounce"
]

const SpecialHandlingVariants = [
    "BadelineAttackPattern", # technically integer, but not easy to use
    "DashDirection", # bool[][]
    "ColorGrading", # string
    "DisableClimbingUpOrDown", # enum
    "DisplaySpeedometer", # enum
    "DontRefillDashOnGround", # enum
    "JumpCount", # integer but with special "infinite" value
    "JungleSpidersEverywhere", # enum
    "MadelineBackpackMode", # enum
    "SpinnerColor", # enum
    "WindEverywhere", # enum
    "DashRestriction" # enum
]

const UnavailableVariants = [
    "MadelineIsSilhouette", # use Madeline Silhouette Trigger instead
    "MadelineHasPonytail" # use Madeline Ponytail Trigger instead
]

end