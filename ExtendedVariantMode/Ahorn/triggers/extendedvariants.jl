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
	"DisableWallJumping",
	"JumpCount",
	"UpsideDown",
	"HyperdashSpeed",
	"WallBouncingSpeed",
	"DashLength",
	"ForceDuckOnGround",
	"InvertDashes",
	"DisableNeutralJumping",
	"ChangeVariantsRandomly",
	"ChangeVariantsInterval",
	"BadelineChasersEverywhere",
	"ChaserCount",
	"AffectExistingChasers",
	"RegularHiccups",
	"OshiroEverywhere"
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