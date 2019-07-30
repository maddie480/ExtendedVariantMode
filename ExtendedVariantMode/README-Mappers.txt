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
- New Value: pick the value you want to set the variant to. Those are the internal values used by the mod:
	- Gravity, FallSpeed, JumpHeight, SpeedX, DashSpeed, Friction, HyperdashSpeed, WallBouncingSpeed, DashLength: the value is the multiplier * 10 (for example, "12" will set the option to 1.2x).
		- The only exception is Friction, where 0 is actually 0.05x and -1 is 0x.
	- Stamina: the value is the max stamina / 10 (for example, "20" will set max stamina to 200, the default value being 110).
	- DashCount: the value is simply the dash count you want, -1 being the default behavior (depending on the inventory).
	- JumpCount: the number of jumps you want, 6 is infinite (yep).
	- DisableWallJumping, UpsideDown, ForceDuckOnGround, InvertDashes, DisableNeutralJumping: 1 to enable, 0 to disable.
	- ChangeVariantsRandomly: 0 to disable, 1 to change vanilla variants, 2 to change extended variants, 3 for both
	- ChangeVariantsInterval: The interval in seconds between two variant changes
- Enable: uncheck this if you want the variant to be reset to its default value, disregarding the "New Value" option.

Please note that changes in variants become permanent only when the player goes to a new screen or hits a Change Respawn Trigger. If the player dies in the same screen, values will get reset.
This rule allows keeping gameplay consistent if an Extended Variant Trigger is set in the middle of a long screen: the first part should be played without the change, and the second part with it.