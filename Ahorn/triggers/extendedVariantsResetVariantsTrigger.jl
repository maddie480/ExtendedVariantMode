module ExtendedVariantModeResetVariantsTrigger

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantMode/ResetVariantsTrigger" ResetVariantsTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
    vanilla::Bool=false, extended::Bool=false)

const placements = Ahorn.PlacementDict(
    "Reset Variants Trigger (Vanilla) (Extended Variant Mode)" => Ahorn.EntityPlacement(
        ResetVariantsTrigger,
        "rectangle",
        Dict{String, Any}(
            "vanilla" => true,
            "extended" => false
        )
    ),
    "Reset Variants Trigger (Extended) (Extended Variant Mode)" => Ahorn.EntityPlacement(
        ResetVariantsTrigger,
        "rectangle",
        Dict{String, Any}(
            "vanilla" => false,
            "extended" => true
        )
    ),
    "Reset Variants Trigger (Both) (Extended Variant Mode)" => Ahorn.EntityPlacement(
        ResetVariantsTrigger,
        "rectangle",
        Dict{String, Any}(
            "vanilla" => true,
            "extended" => true
        )
    )
)

end
