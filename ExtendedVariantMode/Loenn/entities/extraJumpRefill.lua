local refill = {}

refill.name = "ExtendedVariantMode/ExtraJumpRefill"
refill.depth = -100
refill.placements = {
    {
        name = "default",
        data = {
            extraJumps = 1,
            capped = false,
            cap = -1,
            oneUse = false,
            texture = "ExtendedVariantMode/jumprefillblue",
            respawnTime = 2.5,
            breakEvenWhenFull = false
        }
    }
}

refill.fieldInformation = {
    extraJumps = {
        fieldType = "integer"
    },
    cap = {
        fieldType = "integer"
    }
}

function refill.texture(room, entity)
    return "objects/" .. entity.texture .. "/idle00"
end

return refill
