local drawableSprite = require("structs.drawable_sprite")

local theoCrystal = {}

theoCrystal.name = "ExtendedVariantMode/TheoCrystal"
theoCrystal.depth = 100
theoCrystal.fieldInformation = {
    deathEffectColor = {
        fieldType = "color"
    }
}
theoCrystal.placements = {
    name = "theo_crystal",
    data = {
        allowThrowingOffscreen = false,
        allowLeavingBehind = false,
        forceSpawn = false,
        sprite = "theo_crystal",
        deathEffectColor = "228B22"
    }
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "characters/theoCrystal/idle00"

function theoCrystal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end

return theoCrystal
