module ExtendedVariantModeJumpRefill

using ..Ahorn, Maple

@mapdef Entity "ExtendedVariantMode/JumpRefill" JumpRefill(x::Integer, y::Integer, oneUse::Bool=false)

const placements = Ahorn.PlacementDict(
    "Jump Refill (Extended Variant Mode)" => Ahorn.EntityPlacement(
        JumpRefill
    )
)

sprite = "objects/ExtendedVariantMode/jumprefill/idle00"

function Ahorn.selection(entity::JumpRefill)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::JumpRefill, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end