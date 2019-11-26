module ExtendedVariants

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantTrigger" ExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, variantChange::String="Gravity", enable::Bool=true, newValue::Integer=10, revertOnLeave::Bool=false)

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
	"RegularHiccups",
	"HiccupStrength",
	"OshiroEverywhere",
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
	"GameSpeed",
	"EverythingIsUnderwater",
	"BadelineBossesEverywhere",
	"BadelineAttackPattern",
	"ChangePatternsOfExistingBosses",
	"ColorGrading"
]

const placements = Ahorn.PlacementDict(
	"Extended Variant Trigger (ExtendedVariants)" => Ahorn.EntityPlacement(
		ExtendedVariantTrigger,
		"rectangle"
	)
)

Ahorn.editingOptions(trigger::ExtendedVariantTrigger) = Dict{String, Any}(
	"variantChange" => variants
)

end