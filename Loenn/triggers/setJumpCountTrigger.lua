local trigger = {}

trigger.name = "ExtendedVariantMode/SetJumpCountTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        jumpCount = 0,
        mode = "Set"
    }
}

trigger.fieldInformation = {
    mode = {
        options = { "Set", "Cap" },
        editable = false
    },
    jumpCount = {
        fieldType = "integer"
    }
}

return trigger
