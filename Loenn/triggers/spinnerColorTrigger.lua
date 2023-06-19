local trigger = {}

trigger.name = "ExtendedVariantMode/SpinnerColorTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        newValue = "Default",
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
        options = { "Default", "Red", "Purple", "Blue", "Rainbow" },
        editable = false
    }
}

return trigger
