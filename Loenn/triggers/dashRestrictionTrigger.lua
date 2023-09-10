local trigger = {}

trigger.name = "ExtendedVariantMode/DashRestrictionTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        enable = true,
        newValue = 0,
        revertOnLeave = false,
        revertOnDeath = true,
        delayRevertOnDeath = false,
        withTeleport = false,
        coversScreen = false,
        flag = "",
        flagInverted = false,
        onlyOnce = false
    }
}

trigger.fieldInformation = {
    newValue = {
        options = { 
            ["None"] = 0, 
            ["Grounded Only"] = 1, 
            ["Airborne Only"] = 2
        },
        editable = false
    }
}

return trigger
