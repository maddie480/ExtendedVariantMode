local trigger = {}

trigger.name = "ExtendedVariantMode/JumpCountTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        variantChange = "JumpCount",
        newValue = 1,
        infinite = false,
        capOnChange = false,
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
        fieldType = "integer"
    }
}

trigger.ignoredFields = { "variantChange", "_name" }

return trigger
