module ExtendedVariantModeToggleDashDirectionTrigger

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantMode/ToggleDashDirectionTrigger" ToggleDashDirectionTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	dashDirection::Integer=0b1000000000, enable::Bool=true, revertOnLeave::Bool=false, revertOnDeath::Bool=true)

const placements = Ahorn.PlacementDict(
	"Toggle Dash Direction (Extended Variant Mode)" => Ahorn.EntityPlacement(
		ToggleDashDirectionTrigger,
		"rectangle"
	)
)

Ahorn.editingOptions(trigger::ToggleDashDirectionTrigger) = Dict{String, Any}(
	"dashDirection" => Dict{String, Any}(
		"Top" =>          0b1000000000,
		"Top-Right" =>    0b0100000000,
		"Right" =>        0b0010000000,
		"Bottom-Right" => 0b0001000000,
		"Bottom" =>       0b0000100000,
		"Bottom-Left" =>  0b0000010000,
		"Left" =>         0b0000001000,
		"Top-Left" =>     0b0000000100
	)
)

end