# Extended Variant Mode

A simple code mod for Celeste I did to play around with the IL manipulation capabilities of Everest. 
IL manipulation basically allows code modders to inject their code (almost) anywhere in Celeste, and that can be used to modify some specific things in the game's engine... that's exactly what this code mod does.

This mod allows to ~~break~~ change some stuff in the game mechanics... much like Variant Mode in the original.

Adds the following options to the Mod Options menu:
* **Gravity** and **Max fall speed**: can be used together to create a "space" effect, where falling is slower
* **Jump height**: control the max height of Madeline's jumps
* **Walk speed**: affects all horizontal movement when going around
* **Stamina**: affects the ability to climb and grab
* **Dash speed** and **Dash duration**: allows for faster and/or longer dashes
* **Dash count**: disable dashing or give Madeline up to 5 dashes (also affects refills)
* **Ground friction**: make the ground more or less slippery everywhere
* **Disable wall jumping**: remove your ability to wall jump (... let's make Celeste not Celeste anymore)
* **Jump count**: removes your ability to jump or gives you the ability to jump again in midair
* **Upside Down**: this flips the game upside down. Much like the Mirror Mode variant, but vertically.
* **Hyperdash Speed** and **Wallbouncing speed**: controls the speed of hyperdashes (stacks up with X speed) or wallboucing (stacks up with jump height)
* **Disable neutral jumping**: Prevents you from using that technique to climb walls without losing stamina. Neutral jumps now behave like normal jumps!
* **Badeline Chasers Everywhere**: Get Badeline to chase you on every screen in the mountain (with customizable number of Badelines because why not).

This has some Troll Variants too (weird ideas that somehow got implemented anyway):
* **Force Duck on Ground**: Exactly what it says, you have to duck whenever you touch the ground!
* **Invert Dash**: Dash right by pressing Left + Dash.
* **Change Variants Randomly**: The mod will change some variants while you play (_including vanilla variants_), turning the game into complete chaos.

Enabling Extended Variant Mode will add the Variant Mode logo to the Chapter Complete screen.

Variants are also usable by maps, with the Extended Variants Trigger. Check the [README-Mappers.txt](ExtendedVariantMode/README-Mappers.txt) for more details.

**Compatible with Everest 868 and later.**

## How to install

You can download this code mod [on GameBanana](https://gamebanana.com/gamefiles/9486).

To build the project yourself:
* Clone or download the repo
* Compile the project with Visual Studio
* Copy the directory `ExtendedVariantMode\bin\Debug` in the `Mods` folder located in the Celeste directory
