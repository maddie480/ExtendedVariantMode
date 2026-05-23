local trigger = {}

trigger.name = "ExtendedVariantMode/DashbounceControlTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        enable = true,
        newValue = "Off",
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
            ["Off"] = "Off",
            ["Hold"] = "Hold",
            ["Never"] = "Never"
        },
        editable = false
    }
}

return trigger
