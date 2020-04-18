** Using Extended Variant Mode in maps **

First, add the dependency to your map. For example, everest.yaml will look like this:

- Name: TestEVMMap
  Version: 0.0.1
  Dependencies:
    - Name: Everest
      Version: 1.0.0
    - Name: ExtendedVariantMode
      Version: 0.1.0

To enable a variant in a map, you can place an "Extended Variant Trigger" in Ahorn. These have 3 options:
- Variant: pick the variant you want to change
- New Value: pick the value you want to set the variant to. Those are the internal values used by the mod, and thus, can have kinda convoluted logic:
	- Gravity, FallSpeed, JumpHeight, SpeedX, DashSpeed, Friction, HyperdashSpeed, ExplodeLaunchSpeed, WallBouncingSpeed, DashLength, 
		HiccupStrength, GameSpeed, RisingLavaSpeed, SuperdashSteeringSpeed, ScreenShakeIntensity, ZoomLevel: the value is the multiplier * 10 (for example, "12" will set the option to 1.2x).
		- The only exception is Friction (ground friction) and AirFriction, where 0 is actually 0.05x and -1 is 0x.
	- Stamina: the value is the max stamina / 10 (for example, "20" will set max stamina to 200, the default value being 110).
	- DashCount: the value is simply the dash count you want, -1 being the default behavior (depending on the inventory).
	- JumpCount: the number of jumps you want, 6 is infinite (yep).
	- ChaserCount: the number of chasers you want for the "BadelineChasersEverywhere" variant.
	- BadelineAttackPattern: 0 is random, 1 to 15 are specific patterns. Refer to the FinalBoss class in ILSpy or wait for a documentation of these. :ohnoshiro:
	- FirstBadelineSpawnRandom: 0 to make the first Badeline boss spawn at the opposite of the room, 1 to make her spawn at random.
	- BadelineBossCount: the number of bosses you want
	- BadelineBossNodeCount: the number of nodes (positions before Badeline goes offscreen) you want
	- DisableWallJumping, DisableClimbJumping, UpsideDown, ForceDuckOnGround, InvertDashes, DisableNeutralJumping, BadelineChasersEverywhere, AffectExistingChasers, 
		RefillJumpsOnDashRefill, OshiroEverywhere, DisableOshiroSlowdown, DisableSeekerSlowdown, TheoCrystalsEverywhere, HeldDash, AllStrawberriesAreGoldens, 
		DontRefillDashOnGround, EverythingIsUnderwater, BadelineBossesEverywhere, ChangePatternsOfExistingBosses, InvertGrab, JellyfishEverywhere, 
		RisingLavaEverywhere, InvertHorizontalControls, BounceEverywhere: 1 to enable, 0 to disable.
	- OshiroCount, ReverseOshiroCount: the number of Oshiros you want, when OshiroEverywhere is enabled
	- RegularHiccups: the number of tenths of seconds after which a hiccup should occur (for example 15 for 1.5s), 0 to disable
	- RoomLighting: the room's lighting in % divided by 10 (9 => 90%), -1 to disable
	- BlurLevel: the percentage displayed in-game, divided by 10 (5 => 50%). 50% gives a Gaussian blur with sample scale = 0.5. Don't ask me too many details.
	- RoomBloom:
		- -1 = don't change anything
		- 0 to 10 => bloom base = value / 10, bloom strength = 1. For example, 8 => bloom base = 0.8, bloom strength = 1, displayed as 80% in the menu
		- 11 to 14 => bloom base = 1, bloom strength = value - 9. For example, 12 => bloom base = 1, bloom strength = 3, displayed as 300% in the menu
	- GlitchEffect, AnxietyEffect: the value in percent / 5 (for example 19 will give 95%), -1 to disable
	- WindEverywhere: 0 = disabled, 1 = Left, 2 = Right, 3 = LeftStrong, 4 = RightStrong, 5 = RightCrazy, 6 = LeftOnOff, 7 = RightOnOff, 8 = Alternating, 9 = LeftOnOffFast,
        10 = RightOnOffFast, 11 = Down, 12 = Up, 13 = Random
	- SnowballDelay: The delay between two snowballs, multiplied by 10. Default is 8 (for 0.8s).
	- BadelineLag: The delay between the player and the first Badeline, multiplied by 10, default is 0 (actually 1.55s).
	- DelayBetweenBadelines: The delay between two Badelines, multiplied by 10, default is 4 (0.4s).
	- AddSeekers: The number of seekers to add.
	- ColorGrading: -1 to disable. 0 = "none", 1 = "oldsite", 2 = "panicattack", 3 = "templevoid", 4 = "reflection", 5 = "credits", 6 = "cold", 7 = "hot", 8 = "feelingdown", 9 = "golden".
		10 = "tetris" (from Celsius), 11 = "greyscale", 12 = "sepia", 13 = "inverted", 14 = "rgbshift1", 15 = "rgbshift2", 16 = "corrupted".
- Revert On Leave: Set the variant back to its original value when Madeline leaves the trigger.
- Enable: uncheck this if you want the variant to be reset to its default value, disregarding the "New Value" option.

After you placed your first trigger, be sure to exit out of the level and to start it again. Trigger stuff is only initialized when you enter a level with an extended variant trigger in it.

Please note that changes in variants become permanent only when the player goes to a new screen or hits a Change Respawn Trigger. If the player dies in the same screen, values will get reset.
This rule allows keeping gameplay consistent if an Extended Variant Trigger is set in the middle of a long screen: the first part should be played without the change, and the second part with it.

Use cases:

- If you want a variant to be enabled for the whole map
	=> just add an Extended Variant Trigger right on the spawn point at the beginning of your map.
- If you want a variant to be enabled on a specific area in a room
	=> cover the area with an Extended Variant Trigger, and check "Revert On Leave".
- If you want a variant to be enabled in a room
	=> either cover the entire room with an Extended Variant Trigger and check "Revert On Leave", or follow the next point.
- If you want a variant to be enabled in a bunch of successive rooms (for example, rooms 3 to 7)
	=> put an Extended Variant Trigger to **enable** the variant at the beginning of room 3 and at the end of room 7.
	=> put an Extended Variant Trigger to **disable** the variant at the end of room 2 and at the beginning of room 8.
	=> make sure that these triggers cover the spawn points.
	This will ensure the variant is always applied to rooms 3 to 7, even if the player goes back from room 8 to room 7 for example.
