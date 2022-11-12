local trigger = {}

trigger.name = "ExtendedVariantMode/AirDashesTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        variantChange = "AirDashes",
        enable = true,
        newValue = "Infinite",
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
        options = { "Normal", "Two", "Infinite" },
        editable = false
    }
}

trigger.ignoredFields = { "variantChange", "_name" }

return trigger
