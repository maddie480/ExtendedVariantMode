module ExtendedVariantModeExtendedVariantTrigger

using ..Ahorn, Maple

# obsolete
@mapdef Trigger "ExtendedVariantMode/ExtendedVariantTrigger" ExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", enable::Bool=true, newValue::Integer=10, revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/BooleanExtendedVariantTrigger" BooleanExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="DisableNeutralJumping", newValue::Bool=true, revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false)

@mapdef Trigger "ExtendedVariantMode/IntegerExtendedVariantTrigger" IntegerExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="DashCount", enable::Bool=true, newValue::Int=1, revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false)

@mapdef Trigger "ExtendedVariantMode/FloatExtendedVariantTrigger" FloatExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", enable::Bool=true, newValue::Number=1.0, revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false)

@mapdef Trigger "ExtendedVariantMode/BadelineAttackPatternTrigger" BadelineAttackPatternTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="BadelineAttackPattern", enable::Bool=true, newValue::Int=1, revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/DashDirectionTrigger" DashDirectionTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false,
	topLeft::Bool=true, top::Bool=true, topRight::Bool=true, left::Bool=true, right::Bool=true, bottomLeft::Bool=true, bottom::Bool=true, bottomRight::Bool=true,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/DisplaySpeedometerTrigger" DisplaySpeedometerTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::String="BOTH", revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false, coversScreen::Bool=false, flag::String="", flagInverted::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/DontRefillDashOnGroundTrigger" DontRefillDashOnGroundTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::String="ON", revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false, coversScreen::Bool=false, flag::String="", flagInverted::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/JumpCountTrigger" JumpCountTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="JumpCount", newValue::Int=1, infinite::Bool=false, revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/MadelineBackpackModeTrigger" MadelineBackpackModeTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::String="NoBackpack", revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false, coversScreen::Bool=false, flag::String="", flagInverted::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/WindEverywhereTrigger" WindEverywhereTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::String="Left", revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false, coversScreen::Bool=false, flag::String="", flagInverted::Bool=false)
	
const placements = Ahorn.PlacementDict(
	"Extended Variant Trigger (Boolean) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		BooleanExtendedVariantTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Integer) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		IntegerExtendedVariantTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Float) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		FloatExtendedVariantTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Badeline Attack Pattern) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		BadelineAttackPatternTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Dash Direction) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		DashDirectionTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Display Speedometer) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		DisplaySpeedometerTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Don't Refill Dash On Ground) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		DontRefillDashOnGroundTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Jump Count) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		JumpCountTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Madeline Backpack Mode) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		MadelineBackpackModeTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Wind Everywhere) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		WindEverywhereTrigger,
		"rectangle"
	),
)

Ahorn.editingOptions(trigger::BooleanExtendedVariantTrigger) = Dict{String, Any}(
	"variantChange" => Ahorn.ExtendedVariantDictionary.BooleanVariants
)
Ahorn.editingOptions(trigger::IntegerExtendedVariantTrigger) = Dict{String, Any}(
	"variantChange" => Ahorn.ExtendedVariantDictionary.IntegerVariants
)
Ahorn.editingOptions(trigger::FloatExtendedVariantTrigger) = Dict{String, Any}(
	"variantChange" => Ahorn.ExtendedVariantDictionary.FloatVariants
)
Ahorn.editingOptions(trigger::BadelineAttackPatternTrigger) = Dict{String, Any}(
	"variantChange" => Dict{String, Int}(
		"Slow Shots" => 1,
		"Beam > Shot" => 2,
		"Fast double-shots" => 3,
		"Double-shots > Beam" => 4,
		"Beam > Double-shots" => 5,
		"Slow double-shots" => 9,
		"Nothing" => 10,
		"Fast beam" => 14,
		"Slow beam" => 15
	)
)
Ahorn.editingOptions(trigger::DisplaySpeedometerTrigger) = Dict{String, Any}(
	"variantChange" => Dict{String, Int}(
		"Disabled" => "DISABLED",
		"Horizontal" => "HORIZONTAL",
		"Vertical" => "VERTICAL",
		"Both" => "BOTH"
	)
)
Ahorn.editingOptions(trigger::DontRefillDashOnGroundTrigger) = Dict{String, Any}(
	"variantChange" => Dict{String, Int}(
		"Default" => "DEFAULT",
		"On" => "ON",
		"Off" => "OFF"
	)
)
Ahorn.editingOptions(trigger::MadelineBackpackModeTrigger) = Dict{String, Any}(
	"variantChange" => Dict{String, Int}(
		"Default" => "Default",
		"NoBackpack" => "No Backpack",
		"Backpack" => "With Backpack"
	)
)
Ahorn.editingOptions(trigger::WindEverywhereTrigger) = Dict{String, Any}(
	"variantChange" => Dict{String, Int}(
		"Default" => "Default",
		"Left" => "Left",
		"Right" => "Right",
		"LeftStrong" => "Left Strong",
		"RightStrong" => "Right Strong",
		"RightCrazy" => "Right Crazy",
		"LeftOnOff" => "Left On-Off",
		"RightOnOff" => "Right On-Off",
		"Alternating" => "Alternating",
		"LeftOnOffFast" => "Left On-Off Fast",
		"RightOnOffFast" => "Right On-Off Fast",
		"Down" => "Down",
		"Up" => "Up",
		"Random" => "Random"
	)
)

end