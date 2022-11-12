module ExtendedVariantModeTheoCrystal

using ..Ahorn, Maple

@mapdef Entity "ExtendedVariantMode/TheoCrystal" ExtendedVariantTheoCrystal(x::Integer, y::Integer, allowThrowingOffscreen::Bool=false, allowLeavingBehind::Bool=false)

const placements = Ahorn.PlacementDict(
    "Extended Variant Theo Crystal\n(Extended Variant Mode)" => Ahorn.EntityPlacement(
        ExtendedVariantTheoCrystal
    )
)

sprite = "characters/theoCrystal/idle00.png"

function Ahorn.selection(entity::ExtendedVariantTheoCrystal)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y - 10)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ExtendedVariantTheoCrystal, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, -10)

end
