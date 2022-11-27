module ExtendedVariantModeExtendedVariantTrigger

using ..Ahorn, Maple

# obsolete
@mapdef Trigger "ExtendedVariantMode/ExtendedVariantTrigger" ExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", enable::Bool=true, newValue::Integer=10, revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false,
	onlyOnce::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/BooleanExtendedVariantTrigger" BooleanExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="DisableNeutralJumping", newValue::Bool=true, revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)

@mapdef Trigger "ExtendedVariantMode/IntegerExtendedVariantTrigger" IntegerExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="DashCount", enable::Bool=true, newValue::Int=1, revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)

@mapdef Trigger "ExtendedVariantMode/FloatExtendedVariantTrigger" FloatExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Gravity", enable::Bool=true, newValue::Number=1.0, revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)

@mapdef Trigger "ExtendedVariantMode/BadelineAttackPatternTrigger" BadelineAttackPatternTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="BadelineAttackPattern", enable::Bool=true, newValue::Int=1, revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/DashDirectionTrigger" DashDirectionTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false,
	topLeft::Bool=true, top::Bool=true, topRight::Bool=true, left::Bool=true, right::Bool=true, bottomLeft::Bool=true, bottom::Bool=true, bottomRight::Bool=true,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/DisplaySpeedometerTrigger" DisplaySpeedometerTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::String="BOTH", revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false, coversScreen::Bool=false,
	flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/DontRefillDashOnGroundTrigger" DontRefillDashOnGroundTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::String="ON", revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false, coversScreen::Bool=false,
	flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/JumpCountTrigger" JumpCountTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="JumpCount", newValue::Int=1, infinite::Bool=false, revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)

@mapdef Trigger "ExtendedVariantMode/MaxFallSpeedTrigger" MaxFallSpeedTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::Number=1.0, legacy::Bool=false, revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/MadelineBackpackModeTrigger" MadelineBackpackModeTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::String="NoBackpack", revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false, coversScreen::Bool=false,
	flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/WindEverywhereTrigger" WindEverywhereTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::String="Left", revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false, delayRevertOnDeath::Bool=false, coversScreen::Bool=false,
	flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)
	
@mapdef Trigger "ExtendedVariantMode/ColorGradeTrigger" ColorGradeTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	colorGrade::String="none", revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false, delayRevertOnDeath::Bool=false, coversScreen::Bool=false,
	flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)

@mapdef Trigger "ExtendedVariantMode/JungleSpidersEverywhereTrigger" JungleSpidersEverywhereTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::String="Disable", revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false, delayRevertOnDeath::Bool=false, coversScreen::Bool=false,
	flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)

@mapdef Trigger "ExtendedVariantMode/BooleanVanillaVariantTrigger" BooleanVanillaVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="Invincible", newValue::Bool=true, revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)

@mapdef Trigger "ExtendedVariantMode/GameSpeedTrigger" GameSpeedTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="VanillaGameSpeed", enable::Bool=true, newValue::Int=10, revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)

@mapdef Trigger "ExtendedVariantMode/AirDashesTrigger" AirDashesTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	variantChange::String="AirDashes", enable::Bool=true, newValue::String="Infinite", revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false,
	coversScreen::Bool=false, flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)

@mapdef Trigger "ExtendedVariantMode/DisableClimbingUpOrDownTrigger" DisableClimbingUpOrDownTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
	newValue::String="Both", revertOnLeave::Bool=false, revertOnDeath::Bool=true, delayRevertOnDeath::Bool=false, withTeleport::Bool=false, coversScreen::Bool=false,
	flag::String="", flagInverted::Bool=false, onlyOnce::Bool=false)

const placements = Ahorn.PlacementDict(
	"Extended Variant Trigger – Generic (On/Off Toggles) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		BooleanExtendedVariantTrigger,
		"rectangle"
	),
	"Extended Variant Trigger – Generic (Whole Numbers) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		IntegerExtendedVariantTrigger,
		"rectangle"
	),
	"Extended Variant Trigger – Generic (Decimal Numbers) (Extended Variant Mode)" => Ahorn.EntityPlacement(
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
	"Extended Variant Trigger (Max Fall Speed) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		MaxFallSpeedTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Wind Everywhere) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		WindEverywhereTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Color Grading) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		ColorGradeTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Jungle Spiders Everywhere) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		JungleSpidersEverywhereTrigger,
		"rectangle"
	),
	"Vanilla Variant Trigger – Generic (On/Off Toggles) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		BooleanVanillaVariantTrigger,
		"rectangle"
	),
	"Vanilla Variant Trigger (Game Speed) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		GameSpeedTrigger,
		"rectangle"
	),
	"Vanilla Variant Trigger (Air Dashes) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		AirDashesTrigger,
		"rectangle"
	),
	"Extended Variant Trigger (Disable Climbing Up or Down) (Extended Variant Mode)" => Ahorn.EntityPlacement(
		DisableClimbingUpOrDownTrigger,
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
	"newValue" => Dict{String, Int}(
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
	"newValue" => Dict{String, String}(
		"Disabled" => "DISABLED",
		"Horizontal" => "HORIZONTAL",
		"Vertical" => "VERTICAL",
		"Both" => "BOTH"
	)
)
Ahorn.editingOptions(trigger::DontRefillDashOnGroundTrigger) = Dict{String, Any}(
	"newValue" => Dict{String, String}(
		"Default" => "DEFAULT",
		"On" => "ON",
		"Off" => "OFF"
	)
)
Ahorn.editingOptions(trigger::MadelineBackpackModeTrigger) = Dict{String, Any}(
	"newValue" => Dict{String, String}(
		"Default" => "Default",
		"No Backpack" => "NoBackpack",
		"With Backpack" => "Backpack"
	)
)
Ahorn.editingOptions(trigger::WindEverywhereTrigger) = Dict{String, Any}(
	"newValue" => Dict{String, String}(
		"Default" => "Default",
		"Left" => "Left",
		"Right" => "Right",
		"Left Strong" => "LeftStrong",
		"Right Strong" => "RightStrong",
		"Right Crazy" => "RightCrazy",
		"Left On-Off" => "LeftOnOff",
		"Right On-Off" => "RightOnOff",
		"Alternating" => "Alternating",
		"Left On-Off Fast" => "LeftOnOffFast",
		"Right On-Off Fast" => "RightOnOffFast",
		"Down" => "Down",
		"Up" => "Up",
		"Random" => "Random"
	)
)
Ahorn.editingOptions(trigger::ColorGradeTrigger) = Dict{String, Any}(
	"colorGrade" => [
		"none",
		"oldsite",
		"panicattack",
		"templevoid",
		"reflection",
		"credits",
		"cold",
		"hot",
		"feelingdown",
		"golden",
		"max480/extendedvariants/celsius/tetris",
		"max480/extendedvariants/greyscale",
		"max480/extendedvariants/sepia",
		"max480/extendedvariants/inverted",
		"max480/extendedvariants/rgbshift1",
		"max480/extendedvariants/rgbshift2",
		"max480/extendedvariants/hollys_randomnoise"
	]
)
Ahorn.editingOptions(trigger::JungleSpidersEverywhereTrigger) = Dict{String, Any}(
	"newValue" => [
		"Disabled",
		"Blue",
		"Purple",
		"Red"
	]
)
Ahorn.editingOptions(trigger::BooleanVanillaVariantTrigger) = Dict{String, Any}(
	"variantChange" => [
		"DashAssist",
		"Hiccups",
		"InfiniteStamina",
		"Invincible",
		"InvisibleMotion",
		"LowFriction",
		"MirrorMode",
		"NoGrabbing",
		"PlayAsBadeline",
		"SuperDashing",
		"ThreeSixtyDashing"
	]
)
Ahorn.editingOptions(trigger::GameSpeedTrigger) = Dict{String, Any}(
	"newValue" => Dict{String, Int}(
		"0.5x" => 5,
		"0.6x" => 6,
		"0.7x" => 7,
		"0.8x" => 8,
		"0.9x" => 9,
		"1x" => 10,
		"1.2x" => 12,
		"1.4x" => 14,
		"1.6x" => 16
	)
)
Ahorn.editingOptions(trigger::AirDashesTrigger) = Dict{String, Any}(
	"newValue" => [ "Normal", "Two", "Infinite" ]
)
Ahorn.editingOptions(trigger::DisableClimbingUpOrDownTrigger) = Dict{String, Any}(
	"newValue" => [ "Disabled", "Up", "Down", "Both" ]
)

Ahorn.editingOrder(trigger::DashDirectionTrigger) = String["x", "y", "width", "height", "topLeft", "top", "topRight", "left", "right", "bottomLeft", "bottom", "bottomRight"]

# those are internally Integer Extended Variant Triggers that render differently: Jump Count has the "infinite" checkbox, and Badeline Attack Pattern has a dropdown.
Ahorn.editingIgnored(trigger::JumpCountTrigger, multiple::Bool=false) = String["variantChange"]
Ahorn.editingIgnored(trigger::BadelineAttackPatternTrigger, multiple::Bool=false) = String["variantChange"]
Ahorn.editingIgnored(trigger::GameSpeedTrigger, multiple::Bool=false) = String["variantChange"]
Ahorn.editingIgnored(trigger::AirDashesTrigger, multiple::Bool=false) = String["variantChange"]

end
