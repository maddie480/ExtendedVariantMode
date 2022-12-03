module ExtendedVariantModeSetJumpCountTrigger

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantMode/SetJumpCountTrigger" SetJumpCountTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
    jumpCount::Int=0, mode::String="Set")

const placements = Ahorn.PlacementDict(
    "Set Jump Count Trigger (Extended Variant Mode)" => Ahorn.EntityPlacement(
        SetJumpCountTrigger,
        "rectangle"
    )
)

Ahorn.editingOptions(trigger::SetJumpCountTrigger) = Dict{String, Any}(
    "mode" => String["Set", "Cap"]
)

end
