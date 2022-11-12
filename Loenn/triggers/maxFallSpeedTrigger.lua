local trigger = {}

trigger.name = "ExtendedVariantMode/MaxFallSpeedTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        newValue = 1,
        legacy = false,
        revertOnLeave = false,
        revertOnDeath = true,
        delayRevertOnDeath = false,
        coversScreen = false,
        flag = "",
        flagInverted = false,
        onlyOnce = false
    }
}

return trigger
