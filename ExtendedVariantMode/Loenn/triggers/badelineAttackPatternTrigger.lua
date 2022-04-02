local trigger = {}

trigger.name = "ExtendedVariantMode/BadelineAttackPatternTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        variantChange = "BadelineAttackPattern",
        enable = true,
        newValue = 1,
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
            ["Slow Shots"] = 1,
            ["Beam > Shot"] = 2,
            ["Fast double-shots"] = 3,
            ["Double-shots > Beam"] = 4,
            ["Beam > Double-shots"] = 5,
            ["Slow double-shots"] = 9,
            ["Nothing"] = 10,
            ["Fast beam"] = 14,
            ["Slow beam"] = 15
        },
        editable = false
    }
}

trigger.ignoredFields = { "variantChange", "_name" }

return trigger
