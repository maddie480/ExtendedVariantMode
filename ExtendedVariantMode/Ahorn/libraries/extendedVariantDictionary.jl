module ExtendedVariantDictionary

export IntegerVariants, FloatVariants, BooleanVariants

const IntegerVariants = [
	"AddSeekers",
	"BadelineBossCount",
	"BadelineBossNodeCount",
	"ChaserCount",
	"DashCount",
	"JellyfishEverywhere",
	"OshiroCount",
	"ReverseOshiroCount",
	"Stamina"
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
	"DelayBetweenBadelines",
	"ExplodeLaunchSpeed",
	"FallSpeed",
	"ForegroundEffectOpacity",
	"Friction",
	"GameSpeed",
	"GlitchEffect",
	"Gravity",
	"HiccupStrength",
	"HyperdashSpeed",
	"JumpHeight",
	"RegularHiccups",
	"RisingLavaSpeed",
	"RoomLighting",
	"RoomBloom",
	"ScreenShakeIntensity",
	"SnowballDelay",
	"SpeedX",
	"SuperdashSteeringSpeed",
	"SwimmingSpeed",
	"WallBouncingSpeed",
	"WallSlidingSpeed",
	"ZoomLevel"
]

const BooleanVariants = [
	"AffectExistingChasers",
	"AllStrawberriesAreGoldens",
	"AllowLeavingTheoBehind",
	"AllowThrowingTheoOffscreen",
	"AlwaysInvisible",
	"BadelineBossesEverywhere",
	"BadelineChasersEverywhere",
	"BounceEverywhere",
	"ChangePatternsOfExistingBosses",
	"DashTrailAllTheTime",
	"DisableClimbJumping",
	"DisableClimbingUpOrDown",
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
	"NoFreezeFrames",
	"OshiroEverywhere",
	"PreserveExtraDashesUnderwater",
	"RefillJumpsOnDashRefill",
	"RestoreDashesOnRespawn",
	"RisingLavaEverywhere",
	"SnowballsEverywhere",
	"TheoCrystalsEverywhere",
	"UpsideDown"
]

const SpecialHandlingVariants = [
	"BadelineAttackPattern", # technically integer, but not easy to use
	"DashDirection", # bool[][]
	"ColorGrading", # string
	"DisplaySpeedometer", # enum
	"DontRefillDashOnGround", # enum
	"JumpCount", # integer but with special "infinite" value
	"JungleSpidersEverywhere", # enum
	"MadelineBackpackMode", # enum
	"WindEverywhere" # enum
]

const UnavailableVariants = [
	"MadelineIsSilhouette", # use Madeline Silhouette Trigger instead
	"MadelineHasPonytail" # use Madeline Ponytail Trigger instead
]

end