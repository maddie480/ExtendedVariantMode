# Extended Variant Mode

A code mod allowing to ~~destroy Celeste~~ change some things in the game mechanics.

Adds some options to the Mod Options menu:

*   **Gravity** and **Max fall speed**: can be used together  create a "space" effect, where falling is slower
*   **Jump height**: control the max height of Madeline' jumps
*   **Max jump duration**: Madeline will continue to go up for longer (or for a shorter time) if you hold the Jump button.
*   **Horizontal speed**: affects all horizontal movement when going around
*   **Swimming speed**: changes the speed cap when Madeline is swimming inside water

*   **Stamina**: affects the ability to climb and grab
*   **Dash speed** and **Dash duration**: allows for faster and/or longer dashes
*   **Dash count**: disable dashing or give Madeline up to 5 dashes (also affects refills)
*   **Dash timer multiplier**: this one messes with the internal timer Celeste uses to check if you are dashing or not. Raising this will give you more time to walljump for example!
*   **Ground friction**: make the ground more or less slippery everywhere
*   **Air friction**: makes steering in the air more or less quick... setting this to 0x makes it impossible to steer left/right in air.
*   **Disable wall jumping** and **Disable climb jumping** (in case you want to make Celeste not Celeste anymore)
*   **Disable jumping out of water**: this prevents Madeline from jumping out of water, and also from jumping on water (which is normally possible without entering water with good timing).
*   **Disable climbing up or down**: you can still grab walls, but won't be able to climb up, down, or both. You can still go up by climb jumping (_unless you disable that too_).

*   **Jump count**: removes your ability to jump or gives you the ability to jump again in midair.
    *    **Refill Jumps on Dash Refill** will give you back your extra jumps whenever you get your dash back.
    *    **Reset Jump Count on Ground** allows you to choose whether you want to refill jumps on ground (if you have a base Jump Count higher than 1) or whether you get to keep extra jumps when landing on the ground in maps using jump refills!
    *    **Delay Between Midair Jumps** adds a cooldown between two midair jumps, to prevent the player from using them too fast.
*   **Jump Boost**: Modifies the horizontal jump boost you get from jumping, climbjumping, wallboosting and wallbouncing. (brought to you by @SnipUndercover)
*   **Upside Down**: this flips the game upside down. Much like the Mirror Mode variant, but vertically.
*   **Hyperdash Speed** and **Wallbouncing Speed**: controls the speed of hyperdashes (stacks up with X speed) or wallboucing (stacks up with jump height)
*   **Explode Launch Speed**: Modifies the speed given by bumpers, puffer fish and respawning seekers. You can also choose to **disable super boosts** that you get by holding away from the explosion.
*   **Horizontal Spring Bounce Duration**: determines the time during which you cannot change direction after bouncing on a spring.
*   **Wall Sliding Speed**: Modifies the speed Madeline gets when sliding on a wall. She usually slows down for a bit while friction particles appear, but you can make her accelerate instead!

*   **Boost Multiplier**: Makes the speed you get from jumping from a moving block smaller or bigger. You can also _invert_ the direction of that boost.
*   **Horizontal Liftboost Cap** / **Upwards Liftboost Cap** / **Downwards Liftboost Cap**: modifies the maximum speed you get from jumping from a moving block... or makes it outright unlimited. (brought to you by @psyGamer)

*   **Disable neutral jumping**: Prevents you from using that technique to climb walls without losing stamina. Neutral jumps now behave like normal jumps!
*   **Horizontal Wall Jump Duration**: When you don't neutral jump, the game forces Madeline to go in the opposite direction for a bit. You can make that delay shorter or longer with this variant. _Setting that to 0x allows you to neutral jump by just moving towards the wall and jumping!_
*   **Badeline Chasers Everywhere**: Get Badeline to chase you on every screen in the mountain! (with customizable number of Badelines and delay because why not)
    *   The "Affect Existing Chasers" option will also modify the number of Badelines on screens where there already are chasers, like Old Site in the base game. except in the screen where you first encounter Badeline because that would trigger the cutscene 10 times at the same time
*   **Badeline Bosses Everywhere**: Spawn up to _5 bosses_ (from chapter 6) in every room of the game. The first boss will spawn in the opposite side of the room (this is customizable though), the other ones at random positions. Each boss can have up to 5 different positions before disappearing. You can customize the bosses' pattern, and also apply this pattern to existing bosses.
*   **Oshiro Everywhere**: Have Oshiro (or even _multiple Oshiros_) attack you on every screen! You can also disable the slowdown Oshiro triggers if he gets close to you.
*   **Wind Everywhere**: Adds wind to every screen. Also replaces wind on screens where there is already wind. You can pick "No Wind" in order to disable wind on screens that have it!
*   **Snowballs Everywhere**: Add snowballs to every screen etc etc (you can also set the delay between two snowballs!)
*   **Add Seekers**: This adds seekers in random locations on every screen. (If the screen already contained seekers, those will add up.) You can also disable the slowdown effect the seekers trigger when they get closed to you (this only happens in some levels though).
*   **Theo Crystals Everywhere**: Whenever you respawn, a Theo crystal spawns with you. You can't change screens unless you hold it!
*   **Rising Lava / Falling Ice Everywhere**: Makes "sandwich lava" (_this is the internal name for this thing_) appear in every room, with customizable speed. Dashing makes it switch from lava (up) to ice (down).
*   **Regular Hiccups:** Exactly like the vanilla variant, but you can set the hiccup frequency instead of having a random delay between 1.2 and 1.8 seconds. You can make it go down to 0.1 second. Except you shouldn't. _(If you also enable the vanilla Hiccups variant, they will stack up and you will get twice the hiccups.)_ You can also modify the hiccups' strength.
*   Ability to enable or disable visual effects in the game at will:
    *   **Room Lighting**: Set the lighting of all the rooms in the game. In case you want Golden Ridge to be dark and Mirror Temple to be bright.
    *   **Background Brightness**: Room lighting doesn't affect backgrounds in this game. This variant allows you to turn down the background brightness if you're going for darkness in levels like Golden Ridge.
    *   **Foreground Effect Opacity**: Changes the opacity of all effects and stylegrounds appearing in the foreground, in front of Madeline (like snow or dust).

    *   **Disable Madeline Spotlight**: Prevents Madeline from emiting light. That's some way to spice up the Invisible Motion variant for example.
    *   **Disable Keys Spotlight**: Prevents keys from emitting light. This can be used in combination with Disable Madeline Spotlight for more darkness if Madeline is carrying a key.
    *   **Spinner Color**: Changes the color of all of the spinners in the level to one of the vanilla colors: Blue, Red, Purple or Rainbow. Also affects custom spinners from Frost Helper and Viv Helper!
    *   **Display Dash Count**: displays how many dashes Madeline has with a number floating over her head. Useful if you are in a map with a lot of dashes and can't use hair colors to tell how many dashes you have left... or if you just gave yourself 10 dashes through the Dash Count variant.
    *   **Display Speedometer**: shows Madeline's horizontal, vertical or overall speed over her head.

    *   **Room Bloom**: Gives all rooms the "bloom" effect, or remove it. If you don't know what bloom is (I didn't either), check the screenshots
    *   **Glitch Effect**: That's the effect used when hitting a Badeline boss or in the "void" section of Mirror Temple (where you control a seeker).
    *   **Anxiety Effect**: That's the effect that is used when Oshiro is attacking, among other things. Here, Madeline is the anxiety source! (Yeah, literally. That's a thing in code.)

    *   **Blur Effect**: Just blur out the whole screen.
    *   **Background Blur Effect**: Blur out the game's backgrounds.
    *   **Zoom Level**: Zooms the game in or out for more challenge!

    *   **Color Grading**: Apply any of the "color grades" from the game (for example, the ones from Core, or from the credits), but also the "Tetris" color grade from the "Celsius" map by 0x0ade, and some _special_ original ones. You can also use color grades from any mod you have installed!

    *   **Dash Trail All The Time**: This allows you to keep your dash trail / after effect all the time, not only during your dash.
    *   **Friendly Badeline Follower**: Badeline will float behind you, similar to what happens in credits.
    *   **Madeline's Backpack**: makes Madeline have a backpack or not in every map.

*   **Everything is Underwater**: All levels are now water levels! You get to swim through Summit for example.
*   **Always In Feather**: Madeline is flying at all times, as if she collected a feather that never runs out! (brought to you by @SnipUndercover)
*   **Permanent Dash Attack**: Triggers all the things that are usually triggered by dashing (such as breaking dash blocks, or wallbouncing) all the time! Combine this with Always In Feather to be able to advance past dash blocks while flying around.
*   **Permanent Bino Interaction Storage**: Reproduces the effect of a technique that resets the state of Madeline to "normal" on screen transitions, allowing to do weird stuff like canceling Badeline launches and red boosters. (brought to you by @SnipUndercover)
*   **Auto Jump**: Forces Madeline to input full height jumps whenever possible. (brought to you by @AdamKorinek)
*   **Auto Dash**: ... same, but for dashing: forces Madeline to dash whenever possible. (brought to you by @AdamKorinek)
*   **Force Duck on Ground**: Exactly what it says, you have to duck whenever you touch the ground!
*   **Invert Dash**: Dash right by pressing Left + Dash.
*   **Dash Direction**: Prevent yourself from dashing diagonally, or on the contrary, only allow diagonal dashes. You can even disable individual dash directions if you want.
*   **Dash Restriction**: Only allow yourself to dash when airborne or only on the ground. (brought to you by @SnipUndercover)
*   **Invert Grab**: Grab on walls by _not_ pressing the Grab key. And you have to press Grab if you don't want to grab.
*   **Invert Horizontal Controls** and **Invert Vertical Controls**: Pressing Left makes Madeline go right, and/or pressing Up makes Madeline crouch.

*   **Bounce Everywhere**: Bounce each time you hit a surface, like if there were springs everywhere!
*   **Held Dash**: Your dash continues until you let go the Dash key.
*   **All Strawberries are Goldens**: Strawberries won't collect until you finish the level, and dying with one will send you back to the first you collected.
*   **Don't Refill Dash on Ground**: Reproduces what happens in Core. Falling on the ground won't give your dash back. You can also use this option to get ground refills in levels where you don't normally have them.
*   **Don't Refill Stamina on Ground**: Your stamina won't refill by touching the ground anymore, much like what Core does with the dash. The only ways to get your stamina back are changing screens or grabbing refills!

*   **Disable Refills on Screen Transition**: prevents you from getting your dash and stamina back with screen transitions.
*   **Spawn dash count** grants Madeline additional dashes on spawn. You can also start with 0 dashes, which can be useful in combination with disabling dash refills on ground & screen transitions. (brought to you by @casju0)
*   **Screen transition dash count** grants Madeline additional dashes on screen transition. (brought to you by @casju0)
*   **Restore Dashes on Respawn**: saves your dash count when you hit a respawn point, and restores it when you respawn. Makes sense when used with no ground refills.

*   **Game Speed**: Like the vanilla assist/variant, but it goes down to 0x and up to 100x! This stacks up with the vanilla variant if you enable both.
*   **No Freeze Frames**: disables the freeze frames that happen when you dash, collect crystals, etc. (brought to you by @WEGFan)
*   **Jellyfish Everywhere**: Have a jellyfish spawn above you on each screen if you don't already have one.
*   **Steering Speed for Super Dashes**: turn quicker or slower in the air when you have the Super Dashing variant enabled.
*   **Screen Shake Intensity**: just in case you want super screen shake for some reason. _Seizure warning on this one!_
*   **Every Jump is an Ultra**: when you jump or climb jump, Madeline will automatically duck and get a 1.2x horizontal speed boost, like an "ultra" (a speed tech you can do to go fast).
*   **Ultra Speed Multiplier**: ... and this one allows you to adjust that 1.2x horizontal speed boost. That also applies to the "Every Jump is an Ultra" variant! It also affects the speed you get when dashing down-left or down-right against the ground.
*   **Coyote Time**: allows you to modify the time during which you can jump after walking off a platform (0.1 second by default).
*   **Preserve Extra Dashes Underwater**: if you have 2 dashes with a max of 1 (like in Farewell with double dash crystals), dashing in water would usually make you keep this second dash. this setting allows you to turn that behavior off.
*   **Disable Dash Cooldown**: makes it so you don't have to wait for 0.2 seconds after a dash before being able to dash again.
*   **Corner Correction**: when you dash into a wall corner, Madeline can slide by up to 4 pixels to get around the corner. This variant allows you to remove this behavior or to amplify it!
*   **Wall Jump Distance** and **Wall Bounce Distance**: allow Madeline to only wall jump or wall bounce while being closer to the wall... or allow her to wall jump from further away!
*   **Fast Fall Acceleration**: tunes how fast Madeline's speed switches from slow to fast falling.
*   **Madeline is Always Invisible**: just makes Madeline invisible. Like the vanilla Invisible Motion variant, but even when Madeline is not moving!
*   **Corrected Mirror Mode**: Fixes Mirror Mode so that holding 2 opposite directions doesn't make Madeline freak out in place. (brought to you by @nhruo123)
*   A few variants related to **holdables** (like **Theo and jellies**):
    * **Pickup Duration**: the time it takes for Madeline to take the holdable. If you make this delay bigger, the animation will be slowed down, and Madeline's position will be frozen for longer.
    * **Minimum Delay Before Throwing**: the minimum delay after grabbing the holdable before you can throw it away.
    * **Delay Before Regrabbing**: the minimum delay after throwing a holdable before you can grab it again.

*   **True No Grabbing**: Completely disables the grab input check. Useful for maps that repurpose the Grab input. (brought to you by @DominicAglialoro)
*   **Bufferable Grab**: _(only via trigger)_ Makes the grab input bufferable, for maps that introduce a new action when pressing Grab. (brought to you by @DominicAglialoro)
*   **Wall-less Wallbounces**: Allows wallbounces to be done... even if no wall is present! (brought to you by @SnipUndercover)
*   **Mid-Air Tech**: This variant allows for inputting supers, hypers, etc. in midair. Goes well with wall-less wallbounces. (brought to you by @balt-dev)

*   **Preserve Wallbounce Speed**: Allows Madeline to keep her vertical speed when wallbouncing. (brought to you by @SnipUndercover)
*   **Speed-Based Upwards Dash Stretching**<!-- SBUDS :tm: -->: Having a lot of horizontal speed causes Madeline's dash to stretch horizontally. This variant allows Madeline's upwards dashes to also stretch, if she has enough speed. (brought to you by @SnipUndercover)
*   **Disable Jump Gravity Lowering**: You won't be able to divide the gravity by 2 during a jump by holding the button anymore!
*   **Disable Auto-Jump Gravity Lowering**: Same as above, but for jumps forced by the game, for example, after a dash. (brought to you by @SunsetQuasar)

*   **Slowfall Gravity Multiplier**: allows you to set the intensity of the gravity lowering when you hold Jump, while being below the vertical speed threshold. (brought to you by @SunsetQuasar)
*   **Slowfall Speed Threshold**: allows you to change the vertical speed threshold mentioned above. (brought to you by @SunsetQuasar)

*   A few **consistency and leniency improvements**:
    *   **Ultra Protection**: Ensures that you gain ultra speed, even when buffering a jump or dash upon reaching the ground. (brought to you by @DominicAglialoro)
    *   **Liftboost Protection**: Fixes various inconsistencies with inheriting speed from moving platforms. (brought to you by @DominicAglialoro)
    *   **Cornerboost Protection**: Ensures that you gain jump speed when buffering a cornerboost. (brought to you by @DominicAglialoro)
    *   **Crouch Dash Fix**: Ensures that you have the expected crouch state when performing a normal or crouched dash. (brought to you by @DominicAglialoro)
    *   **Multi Buffering**: Allows you to buffer multiple of the same input before it is consumed. (brought to you by @DominicAglialoro)
    *   **Alternative Buffering**: Prevents buffered inputs from being canceled when releasing the input early. (brought to you by @DominicAglialoro)
    *   **Consistent Throwing**: Ignores the sprite scale factor and removes subpixels from the holdable throw position, making throws less inconsistent. (brought to you by @SnipUndercover)
    *   **Safer Diagonal Smuggle**: Makes the position window for diagonal dream smuggles the same as for horizontal smuggles. (brought to you by @DominicAglialoro)
    *   **Dash Before Pickup**: Gives dashing input priority over picking up throwables, making instant dash regrabs easier to perform. (brought to you by @DominicAglialoro)
    *   **Throw Ignores Forced Move**: Allows you to throw in either direction, even when an interaction forces you to move in one direction. (brought to you by @DominicAglialoro)


This can be extended by downloading other mods:

*   Get [DJ Map Helper](https://gamebanana.com/gamefiles/8458) to add a **Reverse Oshiro Everywhere** option to Extended Variants!
*   Get the [2020 Celeste Spring Community Collab](https://gamebanana.com/maps/211745) or [Maddie's Helping Hand](https://github.com/maddie480/MaddieHelpingHand) (1.14.6 or newer) to add a visual option called **Madeline is a Silhouette**, allowing you to play as a ghost playback.
*   Get [Maddie's Helping Hand](https://github.com/maddie480/MaddieHelpingHand) (1.17.3 or newer) to add a visual option called **Madeline has a Ponytail**, which changes Madeline's hair to look like a ponytail, with different hair colors.
*   Get [Jungle Helper](https://github.com/maddie480/JungleHelper) (1.1.2 or newer) to add a **Jungle Spiders Everywhere** option, to spawn spiders falling from the top of the screen (found in [Into the Jungle](https://gamebanana.com/mods/292719)) on every screen of the game!


This mod also includes a **Variant Randomizer**. You get a big bunch of options for that too:

*   pick which variants are randomized and which are not _(looking at you Invisible Motion)_.
*   change variants every X seconds or on each new screen
*   just change one variant at a time or replace everything each time with Reroll Mode
*   gradually disable variants if you get stuck on a room
*   have the list of enabled variants displayed on-screen while you play


Also includes entities and triggers for use with maps:

*   trigger the variant you want when you want on your map by using the **Extended Variants Trigger**. "Integer" variant are numbers without decimals (such as Add Seekers), "Float" variants are numbers with decimals (such as multipliers and percentages), and "Boolean" variants are the ones that can be turned on or off.
*   give the player extra midair jumps with **Jump Refills**. Recover Jump Refills give the player back their midair jumps (much like vanilla refills do with dashes), while Extra Jump Refills gives more jumps to the player, even if that exceeds the Jump Count variant setting.
*   if you want to get a Theo Crystal with the same properties as the extended variant (doesn't crash when moving up, can be left behind, or can be thrown offscreen in any direction), you can either use an Extended Variants Trigger or place an **Extended Variant Theo Crystal**.

You can trigger variants from [Lua Cutscenes](https://gamebanana.com/mods/53678) as well, by doing:

```lua
local luaCutscenesUtils = require("#ExtendedVariants.Module.LuaCutscenesUtils")
luaCutscenesUtils.TriggerIntegerVariant("JumpCount", 2, false)
```

The methods are `TriggerIntegerVariant`, `TriggerBooleanVariant`, `TriggerFloatVariant` and `TriggerVariant`. The third parameter (`true` or `false`) indicates whether the variant should revert if the player dies before changing rooms or hitting a change respawn trigger. You can also use these methods from other code mods.

## How to install

You can download this mod [here!](https://maddie480.ovh/celeste/dl?id=ExtendedVariantMode&twoclick=1)

To build the project yourself:
* Clone or download the repo
* Compile the project with Visual Studio
* Copy the directory `ExtendedVariantMode\bin\Debug` in the `Mods` folder located in the Celeste directory
