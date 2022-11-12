local trigger = {}

trigger.name = "ExtendedVariantMode/DisplaySpeedometerTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        newValue = "BOTH",
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
            Disabled = "DISABLED",
            Horizontal = "HORIZONTAL",
            Vertical = "VERTICAL",
            Both = "BOTH"
        },
        editable = false
    }
}

return trigger
