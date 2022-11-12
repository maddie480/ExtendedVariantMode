local trigger = {}

trigger.name = "ExtendedVariantMode/MadelineBackpackModeTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        newValue = "NoBackpack",
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
            ["Default"] = "Default",
            ["No Backpack"] = "NoBackpack",
            ["With Backpack"] = "Backpack"
        },
        editable = false
    }
}

return trigger
