local trigger = {}

trigger.name = "ExtendedVariantMode/ColorGradeTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        colorGrade = "none",
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
    colorGrade = {
        options = {
            "none",
            "oldsite",
            "panicattack",
            "templevoid",
            "reflection",
            "credits",
            "cold",
            "hot",
            "feelingdown",
            "golden",
            "max480/extendedvariants/celsius/tetris",
            "max480/extendedvariants/greyscale",
            "max480/extendedvariants/sepia",
            "max480/extendedvariants/inverted",
            "max480/extendedvariants/rgbshift1",
            "max480/extendedvariants/rgbshift2",
            "max480/extendedvariants/hollys_randomnoise"
        }
    }
}

return trigger
