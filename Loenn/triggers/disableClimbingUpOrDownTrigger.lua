local trigger = {}

trigger.name = "ExtendedVariantMode/DisableClimbingUpOrDownTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        newValue = "Both",
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
        options = { "Disabled", "Up", "Down", "Both" },
        editable = false
    }
}

return trigger
