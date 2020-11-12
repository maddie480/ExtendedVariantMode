module ExtendedVariantModeExtendedVariantController

using ..Ahorn, Maple

@mapdef Entity "ExtendedVariantMode/ExtendedVariantController" ExtendedVariantController(x::Integer, y::Integer, variantChange::String="Gravity", enable::Bool=true, newValue::Integer=10, revertOnLeave::Bool=false)

const placements = Ahorn.PlacementDict(
	"Extended Variant Controller (Extended Variant Mode)" => Ahorn.EntityPlacement(
		ExtendedVariantController,
		"rectangle"
	)
)

Ahorn.editingOptions(entity::ExtendedVariantController) = Dict{String, Any}(
	"variantChange" => Ahorn.ExtendedVariantDictionary.Variants
)

function Ahorn.selection(entity::ExtendedVariantController)
	x, y = Ahorn.position(entity)

	return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ExtendedVariantController, room::Maple.Room) = Ahorn.drawImage(ctx, "ahorn/ExtendedVariantMode/extended_variant_controller", -12, -12)

end
