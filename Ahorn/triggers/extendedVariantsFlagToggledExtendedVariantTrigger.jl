module ExtendedVariantModeFlagToggledExtendedVariantTrigger

using ..Ahorn, Maple

# obsolete
@mapdef Trigger "ExtendedVariantMode/FlagToggledExtendedVariantTrigger" FlagToggledExtendedVariantTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
    variantChange::String="Gravity", enable::Bool=true, newValue::Integer=10, revertOnLeave::Bool=false, revertOnDeath::Bool=true, withTeleport::Bool=false, flag::String="flag_name", inverted::Bool=false)

end