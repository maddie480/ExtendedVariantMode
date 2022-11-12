local refill = {}

refill.name = "ExtendedVariantMode/RecoverJumpRefill"
refill.depth = -100
refill.placements = {
    {
        name = "default",
        data = {
            oneUse = false,
            texture = "ExtendedVariantMode/jumprefill",
            respawnTime = 2.5,
            breakEvenWhenFull = false
        }
    }
}

function refill.texture(room, entity)
    return "objects/" .. entity.texture .. "/idle00"
end

return refill
