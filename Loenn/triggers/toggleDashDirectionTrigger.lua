local trigger = {}

trigger.name = "ExtendedVariantMode/ToggleDashDirectionTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        dashDirection = 512, -- 0b1000000000
        enable = true,
        revertOnLeave = false,
        revertOnDeath = true
    }
}

trigger.fieldInformation = {
    dashDirection = {
        options = {
            ["Top"] =          512, -- 0b1000000000
            ["Top-Right"] =    256, -- 0b0100000000
            ["Right"] =        128, -- 0b0010000000
            ["Bottom-Right"] = 64,  -- 0b0001000000
            ["Bottom"] =       32,  -- 0b0000100000
            ["Bottom-Left"] =  16,  -- 0b0000010000
            ["Left"] =         8,   -- 0b0000001000
            ["Top-Left"] =     4    -- 0b0000000100
        },
        editable = false
    }
}

return trigger
