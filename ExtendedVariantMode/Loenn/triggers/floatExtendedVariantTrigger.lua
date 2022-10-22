local trigger = {}

trigger.name = "ExtendedVariantMode/FloatExtendedVariantTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        variantChange = "Gravity",
        enable = true,
        newValue = 1.0,
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
            "AirFriction",
            "AnxietyEffect",
            "BackgroundBlurLevel",
            "BackgroundBrightness",
            "BadelineLag",
            "BlurLevel",
            "BoostMultiplier",
            "CoyoteTime",
            "DashLength",
            "DashSpeed",
            "DelayBetweenBadelines",
            "ExplodeLaunchSpeed",
            "FallSpeed",
            "ForegroundEffectOpacity",
            "Friction",
            "GameSpeed",
            "GlitchEffect",
            "Gravity",
            "HiccupStrength",
            "HyperdashSpeed",
            "JumpHeight",
            "MinimumDelayBeforeThrowing",
            "PickupDuration",
            "RegularHiccups",
            "RisingLavaSpeed",
            "RoomLighting",
            "RoomBloom",
            "ScreenShakeIntensity",
            "SnowballDelay",
            "SpeedX",
            "SuperdashSteeringSpeed",
            "SwimmingSpeed",
            "WallBouncingSpeed",
            "WallSlidingSpeed",
            "ZoomLevel"
        },
        editable = false
    }
}

return trigger
