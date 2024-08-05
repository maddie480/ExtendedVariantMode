local trigger = {}

trigger.name = "ExtendedVariantMode/BooleanVanillaVariantTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        variantChange = "Invincible",
        newValue = true,
        revertOnLeave = false,
        revertOnDeath = true,
        delayRevertOnDeath = false,
        withTeleport = false,
        coversScreen = false,
        flag = "",
        flagInverted = false,
        onlyOnce = false,
        toggle = false
    }
}

trigger.fieldInformation = {
    variantChange = {
        options = {
            "DashAssist",
            "Hiccups",
            "InfiniteStamina",
            "Invincible",
            "InvisibleMotion",
            "LowFriction",
            "MirrorMode",
            "NoGrabbing",
            "PlayAsBadeline",
            "SuperDashing",
            "ThreeSixtyDashing"
        },
        editable = false
    }
}

return trigger
