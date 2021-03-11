module ExtendedVariantModeExtendedVariantTrigger

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantMode/ExtendedVariantTrigger" ExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", enable::Bool=true, newValue::Integer=10, revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false)

const placements = Ahorn.PlacementDict(
	"Extended Variant Trigger (Extended Variant Mode)" => Ahorn.EntityPlacement(
		ExtendedVariantTrigger,
		"rectangle"
	)
)

Ahorn.editingOptions(trigger::ExtendedVariantTrigger) = Dict{String, Any}(
	"variantChange" => Ahorn.ExtendedVariantDictionary.Variants
)

end