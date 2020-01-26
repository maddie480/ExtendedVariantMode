module ExtendedVariantModeExtendedVariantTrigger

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantTrigger" ExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", enable::Bool=true, newValue::Integer=10, revertOnLeave::Bool=false)

const variants = String[
	"Gravity",
	"FallSpeed",
	"JumpHeight",
	"SpeedX",
	"Stamina",
	"DashSpeed",
	"DashCount",
	"Friction",
	"AirFriction",
	"DisableWallJumping",
	"DisableClimbJumping",
	"JumpCount",
	"UpsideDown",
	"HyperdashSpeed",
	"WallBouncingSpeed",
	"DashLength",
	"ForceDuckOnGround",
	"InvertDashes",
	"InvertGrab",
	"DisableNeutralJumping",
	"ChangeVariantsRandomly",
	"ChangeVariantsInterval",
	"BadelineChasersEverywhere",
	"ChaserCount",
	"AffectExistingChasers",
	"BadelineLag",
	"DelayBetweenBadelines",
	"RegularHiccups",
	"HiccupStrength",
	"OshiroEverywhere",
	"OshiroCount",
	"ReverseOshiroCount",
	"DisableOshiroSlowdown",
	"WindEverywhere",
	"SnowballsEverywhere",
	"SnowballDelay",
	"AddSeekers",
	"DisableSeekerSlowdown",
	"TheoCrystalsEverywhere",
	"HeldDash",
	"AllStrawberriesAreGoldens",
	"DontRefillDashOnGround",
	"RoomLighting",
	"RoomBloom",
	"GlitchEffect",
	"GameSpeed",
	"EverythingIsUnderwater",
	"BadelineBossesEverywhere",
	"BadelineAttackPattern",
	"ChangePatternsOfExistingBosses",
	"FirstBadelineSpawnRandom",
	"BadelineBossCount",
	"BadelineBossNodeCount",
	"ColorGrading",
	"JellyfishEverywhere",
	"ExplodeLaunchSpeed",
	"RisingLavaEverywhere",
	"RisingLavaSpeed",
	"InvertHorizontalControls",
	"BounceEverywhere"
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