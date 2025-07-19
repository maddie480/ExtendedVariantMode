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
    "LiftboostCapDown",
    "LiftboostCapUp",
    "LiftboostCapX",
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
    "UltraSpeedMultiplier",
    "UnderwaterSpeedX",
    "UnderwaterSpeedY",
    "WallBouncingSpeed",
    "WallSlidingSpeed",
    "WaterSurfaceSpeedX",
    "WaterSurfaceSpeedY",
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
    "AutoDash",
    "AutoJump",
    "BadelineBossesEverywhere",
    "BadelineChasersEverywhere",
    "BounceEverywhere",
    "BufferableGrab",
    "ChangePatternsOfExistingBosses",
    "ConsistentThrowing",
    "CornerboostProtection",
    "CorrectedMirrorMode",
    "CrouchDashFix",
    "DashBeforePickup",
    "DashTrailAllTheTime",
    "DisableClimbJumping",
    "DisableDashCooldown",
    "DisableJumpGravityLowering",
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
    "MidairTech",
    "NoFreezeFrames",
    "NoFreezeFramesAdvanceCassetteBlocks",
    "OshiroEverywhere",
    "PermanentBinoStorage",
    "PermanentDashAttack",
    "PreserveExtraDashesUnderwater",
    "PreserveWallbounceSpeed",
    "RefillJumpsOnDashRefill",
    "ResetJumpCountOnGround",
    "RestoreDashesOnRespawn",
    "RisingLavaEverywhere",
    "SaferDiagonalSmuggle",
    "SnowballsEverywhere",
    "StretchUpDashes",
    "TheoCrystalsEverywhere",
    "ThrowIgnoresForcedMove",
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
