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
        onlyOnce = false,
        toggle = false
    }
}

trigger.fieldInformation = {
    variantChange = {
        options = {
            "AffectExistingChasers",
            "AllStrawberriesAreGoldens",
            "AllowLeavingTheoBehind",
            "AllowThrowingTheoOffscreen",
            "AlternativeBuffering",
            "AlwaysFeather",
            "AlwaysInvisible",
            "AutoJump",
            "BadelineBossesEverywhere",
            "BadelineChasersEverywhere",
            "BounceEverywhere",
            "BufferableGrab",
            "ChangePatternsOfExistingBosses",
            "CornerboostProtection",
            "CorrectedMirrorMode",
            "CrouchDashFix",
            "DashBeforePickup",
            "DashTrailAllTheTime",
            "DisableClimbJumping",
            "DisableDashCooldown",
            "DisableJumpGravityLowering",
            "DisableJumpingOutOfWater",
            "DisableKeysSpotlight",
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
            "LiftboostProtection",
            "MidairTech",
            "NoFreezeFrames",
            "NoFreezeFramesAdvanceCassetteBlocks",
            "OshiroEverywhere",
            "PermanentBinoStorage",
            "PermanentDashAttack",
            "PreserveExtraDashesUnderwater",
            "PreserveWallbounceSpeed",
            "RefillJumpsOnDashRefill",
            "ResetJumpCountOnGround",
            "RestoreDashesOnRespawn",
            "RisingLavaEverywhere",
            "SaferDiagonalSmuggle",
            "SnowballsEverywhere",
            "StretchUpDashes",
            "TheoCrystalsEverywhere",
            "ThrowIgnoresForcedMove",
            "TrueNoGrabbing",
            "UltraProtection",
            "UpsideDown",
            "WalllessWallbounce"
        },
        editable = false
    }
}

return trigger
