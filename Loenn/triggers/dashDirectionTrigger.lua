local trigger = {}

trigger.name = "ExtendedVariantMode/DashDirectionTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        revertOnLeave = false,
        revertOnDeath = true,
        delayRevertOnDeath = false,
        withTeleport = false,
        topLeft = true,
        top = true,
        topRight = true,
        left = true,
        right = true,
        bottomLeft = true,
        bottom = true,
        bottomRight = true,
        coversScreen = false,
        flag = "",
        flagInverted = false,
        onlyOnce = false
    }
}

trigger.fieldOrder = { "x", "y", "width", "height", "topLeft", "top", "topRight", "left", "right", "bottomLeft", "bottom", "bottomRight" }

return trigger
