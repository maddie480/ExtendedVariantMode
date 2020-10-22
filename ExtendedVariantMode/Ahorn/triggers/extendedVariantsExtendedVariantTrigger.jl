module ExtendedVariantModeExtendedVariantTrigger

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantMode/ExtendedVariantTrigger" ExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", enable::Bool=true, newValue::Integer=10, revertOnLeave::Bool=false)

const variants = String[
	"AddSeekers",
	"AffectExistingChasers",
	"AirFriction",
	"AllStrawberriesAreGoldens",
	"AnxietyEffect",
	"BackgroundBrightness",
	"BadelineAttackPattern",
	"BadelineBossCount",
	"BadelineBossNodeCount",
	"BadelineBossesEverywhere",
	"BadelineChasersEverywhere",
	"BadelineLag",
	"BlurLevel",
	"BounceEverywhere",
	"ChangePatternsOfExistingBosses",
	"ChangeVariantsInterval",
	"ChangeVariantsRandomly",
	"ChaserCount",
	"ColorGrading",
	"DashCount",
	"DashDirection",
	"DashLength",
	"DashSpeed",
	"DashTrailAllTheTime",
	"DelayBetweenBadelines",
	"DisableClimbJumping",
	"DisableClimbingUpOrDown",
	"DisableMadelineSpotlight",
	"DisableNeutralJumping",
	"DisableOshiroSlowdown",
	"DisableSeekerSlowdown",
	"DisableWallJumping",
	"DontRefillDashOnGround",
	"EverythingIsUnderwater",
	"ExplodeLaunchSpeed",
	"FallSpeed",
	"FirstBadelineSpawnRandom",
	"ForceDuckOnGround",
	"ForegroundEffectOpacity",
	"Friction",
	"GameSpeed",
	"GlitchEffect",
	"Gravity",
	"HeldDash",
	"HiccupStrength",
	"HyperdashSpeed",
	"InvertDashes",
	"InvertGrab",
	"InvertHorizontalControls",
	"JellyfishEverywhere",
	"JumpCount",
	"JumpHeight",
	"OshiroCount",
	"OshiroEverywhere",
	"RegularHiccups",
	"ReverseOshiroCount",
	"RisingLavaEverywhere",
	"RisingLavaSpeed",
	"RoomBloom",
	"RoomLighting",
	"ScreenShakeIntensity",
	"SnowballDelay",
	"SnowballsEverywhere",
	"SpeedX",
	"Stamina",
	"SuperdashSteeringSpeed",
	"TheoCrystalsEverywhere",
	"UpsideDown",
	"WallBouncingSpeed",
	"WindEverywhere",
	"ZoomLevel"
]

const placements = Ahorn.PlacementDict(
	"Extended Variant Trigger (Extended Variant Mode)" => Ahorn.EntityPlacement(
		ExtendedVariantTrigger,
		"rectangle"
	)
)

Ahorn.editingOptions(trigger::ExtendedVariantTrigger) = Dict{String, Any}(
	"variantChange" => variants
)

end