local trigger = {}

trigger.name = "ExtendedVariantMode/WindEverywhereTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        newValue = "Left",
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
            ["Left"] = "Left",
            ["Right"] = "Right",
            ["Left Strong"] = "LeftStrong",
            ["Right Strong"] = "RightStrong",
            ["Right Crazy"] = "RightCrazy",
            ["Left On-Off"] = "LeftOnOff",
            ["Right On-Off"] = "RightOnOff",
            ["Alternating"] = "Alternating",
            ["Left On-Off Fast"] = "LeftOnOffFast",
            ["Right On-Off Fast"] = "RightOnOffFast",
            ["Down"] = "Down",
            ["Up"] = "Up",
            ["Random"] = "Random"
        },
        editable = false
    }
}

return trigger
