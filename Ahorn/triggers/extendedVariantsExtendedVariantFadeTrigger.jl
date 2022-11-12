module ExtendedVariantModeExtendedVariantFadeTrigger

using ..Ahorn, Maple

# obsolete
@mapdef Trigger "ExtendedVariantMode/ExtendedVariantFadeTrigger" ExtendedVariantFadeTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", valueA::Integer=10, valueB::Integer=10, positionMode::String="LeftToRight", revertOnDeath::Bool=true)
	
@mapdef Trigger "ExtendedVariantMode/FloatExtendedVariantFadeTrigger" FloatExtendedVariantFadeTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", valueA::Number=1.0, valueB::Number=1.0, positionMode::String="LeftToRight", revertOnDeath::Bool=true)

const placements = Ahorn.PlacementDict(
	"Extended Variant Fade Trigger (Extended Variant Mode)" => Ahorn.EntityPlacement(
		FloatExtendedVariantFadeTrigger,
		"rectangle"
	)
)

Ahorn.editingOptions(trigger::FloatExtendedVariantFadeTrigger) = Dict{String, Any}(
	"variantChange" => Ahorn.ExtendedVariantDictionary.FloatVariants,
	"positionMode" => Maple.trigger_position_modes
)

end
