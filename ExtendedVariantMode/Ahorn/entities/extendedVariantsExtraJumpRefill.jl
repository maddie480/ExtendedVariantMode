module ExtendedVariantModeExtraJumpRefill

using ..Ahorn, Maple

@mapdef Entity "ExtendedVariantMode/ExtraJumpRefill" ExtraJumpRefill(x::Integer, y::Integer,
    extraJumps::Int=1, capped::Bool=false, cap::Int=-1, oneUse::Bool=false, texture::String="ExtendedVariantMode/jumprefillblue")

const placements = Ahorn.PlacementDict(
    "Extra Jump Refill (Extended Variant Mode)" => Ahorn.EntityPlacement(
        ExtraJumpRefill
    )
)

function Ahorn.selection(entity::ExtraJumpRefill)
    sprite = "objects/" * get(entity.data, "texture", "ExtendedVariantMode/jumprefillblue") * "/idle00"
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ExtraJumpRefill, room::Maple.Room)
    sprite = "objects/" * get(entity.data, "texture", "ExtendedVariantMode/jumprefillblue") * "/idle00"
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end