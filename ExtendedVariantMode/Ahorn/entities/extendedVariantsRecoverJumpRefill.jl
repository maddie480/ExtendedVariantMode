module ExtendedVariantModeRecoverJumpRefill

using ..Ahorn, Maple

@mapdef Entity "ExtendedVariantMode/RecoverJumpRefill" RecoverJumpRefill(x::Integer, y::Integer, oneUse::Bool=false, texture::String="ExtendedVariantMode/jumprefill", respawnTime::Number=2.5, breakEvenWhenFull::Bool=false)
@mapdef Entity "ExtendedVariantMode/JumpRefill" LegacyJumpRefill(x::Integer, y::Integer, oneUse::Bool=false, texture::String="ExtendedVariantMode/jumprefill", breakEvenWhenFull::Bool=false)

recoverJumpRefills = Union{RecoverJumpRefill, LegacyJumpRefill}

const placements = Ahorn.PlacementDict(
    "Recover Jump Refill (Extended Variant Mode)" => Ahorn.EntityPlacement(
        RecoverJumpRefill
    )
)

function Ahorn.selection(entity::recoverJumpRefills)
    sprite = "objects/" * get(entity.data, "texture", "ExtendedVariantMode/jumprefill") * "/idle00"
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::recoverJumpRefills, room::Maple.Room)
    sprite = "objects/" * get(entity.data, "texture", "ExtendedVariantMode/jumprefill") * "/idle00"
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end