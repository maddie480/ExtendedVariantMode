module ExtendedVariantModeExtendedVariantFadeTrigger

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantMode/ExtendedVariantFadeTrigger" ExtendedVariantFadeTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", valueA::Integer=10, valueB::Integer=10, positionMode::String="LeftToRight", revertOnDeath::Bool=true)

const placements = Ahorn.PlacementDict(
	"Extended Variant Fade Trigger (Extended Variant Mode)" => Ahorn.EntityPlacement(
		ExtendedVariantFadeTrigger,
		"rectangle"
	)
)

Ahorn.editingOptions(trigger::ExtendedVariantFadeTrigger) = Dict{String, Any}(
	"variantChange" => Ahorn.ExtendedVariantDictionary.Variants,
	"positionMode" => Maple.trigger_position_modes
)

end
