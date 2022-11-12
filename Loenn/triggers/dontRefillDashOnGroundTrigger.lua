local trigger = {}

trigger.name = "ExtendedVariantMode/DontRefillDashOnGroundTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        newValue = "ON",
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
            Default = "DEFAULT",
            On = "ON",
            Off = "OFF"
        },
        editable = false
    }
}

return trigger
