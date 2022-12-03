module ExtendedVariantModeExtendedVariantOnScreenDisplayTrigger

using ..Ahorn, Maple

@mapdef Trigger "ExtendedVariantMode/ExtendedVariantOnScreenDisplayTrigger" ExtendedVariantOnScreenDisplayTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
    enable::Bool=true)

const placements = Ahorn.PlacementDict(
    "Extended Variant On-Screen Display Trigger\n(Extended Variant Mode)" => Ahorn.EntityPlacement(
        ExtendedVariantOnScreenDisplayTrigger,
        "rectangle"
    )
)

end