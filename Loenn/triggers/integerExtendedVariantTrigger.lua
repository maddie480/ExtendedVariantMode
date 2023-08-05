local trigger = {}

trigger.name = "ExtendedVariantMode/IntegerExtendedVariantTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        variantChange = "DashCount",
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
    variantChange = {
        options = {
            "AddSeekers",
            "BadelineBossCount",
            "BadelineBossNodeCount",
            "ChaserCount",
            "CornerCorrection",
            "DashCount",
            "JellyfishEverywhere",
            "OshiroCount",
            "ReverseOshiroCount",
            "Stamina",
            "WallBounceDistance",
            "WallJumpDistance"
        },
        editable = false
    },
    newValue = {
        fieldType = "integer"
    }
}

return trigger
