module ExtendedVariantModeColorGradeTrigger

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantMode/ColorGradeTrigger" ColorGradeTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	colorGrade::String="none", revertOnDeath::Bool=true, onlyOnce::Bool=false)

const colorGrades = String["none", "oldsite", "panicattack", "templevoid", "reflection", "credits", "cold", "hot", "feelingdown", "golden",
	"max480/extendedvariants/celsius/tetris",
	"max480/extendedvariants/greyscale", "max480/extendedvariants/sepia", "max480/extendedvariants/inverted",
	"max480/extendedvariants/rgbshift1", "max480/extendedvariants/rgbshift2", "max480/extendedvariants/hollys_randomnoise"]

const placements = Ahorn.PlacementDict(
	"Color Grade Trigger (Extended Variant Mode)" => Ahorn.EntityPlacement(
		ColorGradeTrigger,
		"rectangle"
	)
)

Ahorn.editingOptions(trigger::ColorGradeTrigger) = Dict{String, Any}(
	"colorGrade" => colorGrades
)

end
