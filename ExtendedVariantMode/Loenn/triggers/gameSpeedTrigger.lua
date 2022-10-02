local trigger = {}

trigger.name = "ExtendedVariantMode/GameSpeedTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        variantChange = "VanillaGameSpeed",
        enable = true,
        newValue = 10,
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
        fieldType = "integer",
        options = {
            ["0.5x"] = 5,
            ["0.6x"] = 6,
            ["0.7x"] = 7,
            ["0.8x"] = 8,
            ["0.9x"] = 9,
            ["1x"] = 10,
            ["1.2x"] = 12,
            ["1.4x"] = 14,
            ["1.6x"] = 16
        },
        editable = false
    }
}

trigger.ignoredFields = { "variantChange", "_name" }

return trigger
