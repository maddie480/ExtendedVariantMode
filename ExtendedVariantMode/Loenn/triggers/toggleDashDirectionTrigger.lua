local trigger = {}

trigger.name = "ExtendedVariantMode/ToggleDashDirectionTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        dashDirection = 0b1000000000,
        enable = true,
        revertOnLeave = false,
        revertOnDeath = true
    }
}

trigger.fieldInformation = {
    dashDirection = {
        options = {
            ["Top"] =          0b1000000000,
            ["Top-Right"] =    0b0100000000,
            ["Right"] =        0b0010000000,
            ["Bottom-Right"] = 0b0001000000,
            ["Bottom"] =       0b0000100000,
            ["Bottom-Left"] =  0b0000010000,
            ["Left"] =         0b0000001000,
            ["Top-Left"] =     0b0000000100
        },
        editable = false
    }
}

return trigger
