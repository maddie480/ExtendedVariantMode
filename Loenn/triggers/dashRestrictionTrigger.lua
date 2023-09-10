local trigger = {}

trigger.name = "ExtendedVariantMode/DashRestrictionTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        enable = true,
        newValue = "None",
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
            ["None"] = "None",
            ["Grounded Only"] = "GroundedOnly",
            ["Airborne Only"] = "AirborneOnly"
        },
        editable = false
    }
}

return trigger
