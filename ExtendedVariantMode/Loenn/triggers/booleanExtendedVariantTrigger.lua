local trigger = {}

trigger.name = "ExtendedVariantMode/BooleanExtendedVariantTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        variantChange = "DisableNeutralJumping",
        newValue = true,
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
            "AffectExistingChasers",
            "AllStrawberriesAreGoldens",
            "AllowLeavingTheoBehind",
            "AllowThrowingTheoOffscreen",
            "AlwaysInvisible",
            "BadelineBossesEverywhere",
            "BadelineChasersEverywhere",
            "BounceEverywhere",
            "ChangePatternsOfExistingBosses",
            "DashTrailAllTheTime",
            "DisableClimbJumping",
            "DisableClimbingUpOrDown",
            "DisableDashCooldown",
            "DisableJumpingOutOfWater",
            "DisableMadelineSpotlight",
            "DisableNeutralJumping",
            "DisableOshiroSlowdown",
            "DisableRefillsOnScreenTransition",
            "DisableSeekerSlowdown",
            "DisableSuperBoosts",
            "DisableWallJumping",
            "DisplayDashCount",
            "DontRefillStaminaOnGround",
            "EveryJumpIsUltra",
            "EverythingIsUnderwater",
            "FirstBadelineSpawnRandom",
            "ForceDuckOnGround",
            "FriendlyBadelineFollower",
            "HeldDash",
            "InvertDashes",
            "InvertGrab",
            "InvertHorizontalControls",
            "InvertVerticalControls",
            "LegacyDashSpeedBehavior",
            "NoFreezeFrames",
            "OshiroEverywhere",
            "PreserveExtraDashesUnderwater",
            "RefillJumpsOnDashRefill",
            "RestoreDashesOnRespawn",
            "RisingLavaEverywhere",
            "SnowballsEverywhere",
            "TheoCrystalsEverywhere",
            "UpsideDown"
        },
        editable = true -- TODO change to false when lists are scrollable in Lönn
    }
}

return trigger
