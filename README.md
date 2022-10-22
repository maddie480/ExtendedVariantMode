# Extended Variant Mode

A code mod allowing to ~~destroy Celeste~~ change some things in the game mechanics.  
  
Adds some options to the Mod Options menu:  

*   **Gravity** and **Max fall speed**: can be used together  create a "space" effect, where falling is slower
*   **Jump height**: control the max height of Madeline' jumps
*   **Horizontal speed**: affects all horizontal movement when going around
*   **Swimming speed**: changes the speed cap when Madeline is swimming inside water
    
*   **Stamina**: affects the ability to climb and grab
*   **Dash speed** and **Dash duration**: allows for faster and/or longer dashes
*   **Dash count**: disable dashing or give Madeline up to 5 dashes (also affects refills)
*   **Ground friction**: make the ground more or less slippery everywhere
*   **Air friction**: makes steering in the air more or less quick... setting this to 0x makes it impossible to steer left/right in air.
*   **Disable wall jumping** and **Disable climb jumping** (in case you want to make Celeste not Celeste anymore)
*   **Disable jumping out of water**: this prevents Madeline from jumping out of water, and also from jumping on water (which is normally possible without entering water with good timing).
*   **Disable climbing up or down**: you can still grab walls, but won't be able to climb up or down. You can still go up by climb jumping (_unless you disable that too_).  

*   **Jump count**: removes your ability to jump or gives you the ability to jump again in midair (Refill Jumps on Dash Refill will give you back your extra jumps whenever you get your dash back)
*   **Upside Down**: this flips the game upside down. Much like the Mirror Mode variant, but vertically.
*   **Hyperdash Speed** and **Wallbouncing Speed**: controls the speed of hyperdashes (stacks up with X speed) or wallboucing (stacks up with jump height)
*   **Explode Launch Speed**: Modifies the speed given by bumpers, puffer fish and respawning seekers. You can also choose to **disable super boosts** that you get by holding away from the explosion.  
*   **Wall Sliding Speed**: Modifies the speed Madeline gets when sliding on a wall. She usually slows down for a bit while friction particles appear, but you can make her accelerate instead!
    
*   **Boost Multiplier**: Makes the speed you get from jumping from a moving block smaller or bigger. You can also _invert_ the direction of that boost.  
    
*   **Disable neutral jumping**: Prevents you from using that technique to climb walls without losing stamina. Neutral jumps now behave like normal jumps!
*   **Badeline Chasers Everywhere**: Get Badeline to chase you on every screen in the mountain! (with customizable number of Badelines and delay because why not)
    *   The "Affect Existing Chasers" option will also modify the number of Badelines on screens where there already are chasers, like Old Site in the base game. except in the screen where you first encounter Badeline because that would trigger the cutscene 10 times at the same time
*   **Badeline Bosses Everywhere**: Spawn up to _5 bosses_ (from chapter 6) in every room of the game. The first boss will spawn in the opposite side of the room (this is customizable though), the other ones at random positions. Each boss can have up to 5 different positions before disappearing. You can customize the bosses' pattern, and also apply this pattern to existing bosses.
*   **Oshiro Everywhere**: Have Oshiro (or even _multiple Oshiros_) attack you on every screen! You can also disable the slowdown Oshiro triggers if he gets close to you.
*   **Wind Everywhere**: Adds wind to every screen. Also replaces wind on screens where there is already wind.
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
*   **Force Duck on Ground**: Exactly what it says, you have to duck whenever you touch the ground!
*   **Invert Dash**: Dash right by pressing Left + Dash.
*   **Dash Direction**: Prevent yourself from dashing diagonally, or on the contrary, only allow diagonal dashes. You can even disable individual dash directions if you want.  
    
*   **Invert Grab**: Grab on walls by _not_ pressing the Grab key. And you have to press Grab if you don't want to grab.
*   **Invert Horizontal Controls** and **Invert Vertical Controls**: Pressing Left makes Madeline go right, and/or pressing Up makes Madeline crouch.  
    
*   **Bounce Everywhere**: Bounce each time you hit a surface, like if there were springs everywhere!
*   **Held Dash**: Your dash continues until you let go the Dash key.
*   **All Strawberries are Goldens**: Strawberries won't collect until you finish the level, and dying with one will send you back to the first you collected.
*   **Don't Refill Dash on Ground**: Reproduces what happens in Core. Falling on the ground won't give your dash back. You can also use this option to get ground refills in levels where you don't normally have them.
*   **Don't Refill Stamina on Ground**: Your stamina won't refill by touching the ground anymore, much like what Core does with the dash. The only ways to get your stamina back are changing screens or grabbing refills!  
    
*   **Disable Refills on Screen Transition**: prevents you from getting your dash and stamina back with screen transitions.
*   **Restore Dashes on Respawn**: saves your dash count when you hit a respawn point, and restores it when you respawn. Makes sense when used with no ground refills.  
    
*   **Game Speed**: Like the vanilla assist/variant, but it goes down to 0x and up to 100x! This stacks up with the vanilla variant if you enable both.
*   **No Freeze Frames**: disables the freeze frames that happen when you dash, collect crystals, etc.
*   **Jellyfish Everywhere**: Have a jellyfish spawn above you on each screen if you don't already have one.
*   **Steering Speed for Super Dashes**: turn quicker or slower in the air when you have the Super Dashing variant enabled.
*   **Screen Shake Intensity**: just in case you want super screen shake for some reason. _Seizure warning on this one!_  
*   **Every Jump is an Ultra**: when you jump or climb jump, Madeline will automatically duck and get a 1.2x horizontal speed boost, like an "ultra" (a speed tech you can do to go fast).
*   **Coyote Time**: allows you to modify the time during which you can jump after walking off a platform (0.1 second by default).
*   **Preserve Extra Dashes Underwater**: if you have 2 dashes with a max of 1 (like in Farewell with double dash crystals), dashing in water would usually make you keep this second dash. this setting allows you to turn that behavior off.
*   **Disable Dash Cooldown**: makes it so you don't have to wait for 0.2 seconds after a dash before being able to dash again.
*   **Corner Correction**: when you dash into a wall corner, Madeline can slide by up to 4 pixels to get around the corner. This variant allows you to remove this behavior or to amplify it!
*   **Madeline is Always Invisible**: just makes Madeline invisible. Like the vanilla Invisible Motion variant, but even when Madeline is not moving!
*   A few variants related to **holdables** (like **Theo and jellies**):
    * **Pickup Duration**: the time it takes for Madeline to take the holdable. If you make this delay bigger, the animation will be slowed down, and Madeline's position will be frozen for longer.
    * **Minimum Delay Before Throwing**: the minimum delay after grabbing the holdable before you can throw it away.
    * **Delay Before Regrabbing**: the minimum delay after throwing a holdable before you can grab it again.

  
This can be extended by downloading other mods:  

*   Get [DJ Map Helper](https://gamebanana.com/gamefiles/8458) to add a **Reverse Oshiro Everywhere** option to Extended Variants!
*   Get the [2020 Celeste Spring Community Collab](https://gamebanana.com/maps/211745) or [max480's Helping Hand](https://github.com/max4805/MaxHelpingHand) (1.14.6 or newer) to add a visual option called **Madeline is a Silhouette**, allowing you to play as a ghost playback.
*   Get [max480's Helping Hand](https://github.com/max4805/MaxHelpingHand) (1.17.3 or newer) to add a visual option called **Madeline has a Ponytail**, which changes Madeline's hair to look like a ponytail, with different hair colors.  
*   Get [Jungle Helper](https://github.com/max4805/JungleHelper) (1.1.2 or newer) to add a **Jungle Spiders Everywhere** option, to spawn spiders falling from the top of the screen (found in [Into the Jungle](https://gamebanana.com/mods/292719)) on every screen of the game!

  
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

## How to install

You can download this mod [here!](https://0x0a.de/twoclick?https://gamebanana.com/mmdl/880146)

To build the project yourself:
* Clone or download the repo
* Compile the project with Visual Studio
* Copy the directory `ExtendedVariantMode\bin\Debug` in the `Mods` folder located in the Celeste directory
