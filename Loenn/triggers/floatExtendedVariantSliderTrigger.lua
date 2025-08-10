local enums = require("consts.celeste_enums")

local trigger = {}

trigger.name = "ExtendedVariantMode/FloatExtendedVariantSliderTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        variantChange = "Gravity",
        slider = "gravity",
        revertOnDeath = true
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
            "DashTimerMultiplier",
            "DelayBeforeRegrabbing",
            "DelayBetweenBadelines",
            "ExplodeLaunchSpeed",
            "FallSpeed",
            "ForegroundEffectOpacity",
            "Friction",
            "GameSpeed",
            "GlitchEffect",
            "Gravity",
            "HiccupStrength",
            "HorizontalSpringBounceDuration",
            "HorizontalWallJumpDuration",
            "HyperdashSpeed",
            "JumpCooldown",
            "JumpDuration",
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
            "UnderwaterSpeedX",
            "UnderwaterSpeedY",
            "UltraSpeedMultiplier",
            "WallBouncingSpeed",
            "WallSlidingSpeed",
            "WaterSurfaceSpeedX",
            "WaterSurfaceSpeedY",
            "ZoomLevel"
        },
        editable = false
    }
}

return trigger
