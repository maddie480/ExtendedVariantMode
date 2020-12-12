module ExtendedVariantModeFlagToggledExtendedVariantTrigger

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantMode/FlagToggledExtendedVariantTrigger" FlagToggledExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", enable::Bool=true, newValue::Integer=10, revertOnLeave::Bool=false, revertOnDeath::Bool=true, flag::String="flag_name", inverted::Bool=false)

const placements = Ahorn.PlacementDict(
	"Extended Variant Trigger (Flag-Toggled) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		FlagToggledExtendedVariantTrigger,
		"rectangle"
	)
)

Ahorn.editingOptions(trigger::FlagToggledExtendedVariantTrigger) = Dict{String, Any}(
	"variantChange" => Ahorn.ExtendedVariantDictionary.Variants
)

end