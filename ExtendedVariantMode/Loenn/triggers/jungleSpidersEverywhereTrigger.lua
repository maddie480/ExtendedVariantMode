local trigger = {}

trigger.name = "ExtendedVariantMode/JungleSpidersEverywhereTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        newValue = "Disabled",
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
        options = { "Disabled", "Blue", "Purple", "Red" },
        editable = false
    }
}

return trigger
