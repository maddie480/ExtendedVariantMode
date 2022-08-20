local enums = require("consts.celeste_enums")

local trigger = {}

trigger.name = "ExtendedVariantMode/ResetVariantsTrigger"
trigger.placements = {
    {
        name = "vanilla",
        data = {
            vanilla = true,
            extended = false
        }
    },
    {
        name = "extended",
        data = {
            vanilla = false,
            extended = true
        }
    },
    {
        name = "both",
        data = {
            vanilla = true,
            extended = true
        }
    }
}

return trigger
