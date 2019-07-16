# Extended Variant Mode

A simple code mod for Celeste I did to play around with the IL manipulation capabilities of Everest. 
IL manipulation basically allows code modders to inject their code (almost) anywhere in Celeste, and that can be used to modify some specific things in the game's engine... that's exactly what this code mod does.

This mod allows to ~~break~~ change some stuff in the game mechanics... much like Variant Mode in the original.

Adds the following options to the Mod Options menu:
* **Gravity**: affects falling speed, jump height and climbing
* **Walk speed**: affects all horizontal movement when going around
* **Stamina**: affects the ability to climb and grab
* **Dash speed**: modifies the speed of dashing (... duh)
* **Dash count**: disable dashing or give Madeline up to 5 dashes (also affects refills)
* **Ground friction**: make the ground more or less slippery everywhere

## How to install

... just wait for the first release. You can still clone the repo, open it with Visual Studio and compile the code mod, but it goes wrong quite quick:

```
Celeste Error Log
==========================================

Ver 1.2.6.1 [Everest: 859-azure-98148]
07/16/2019 20:50:39
System.InvalidProgramException: Le compilateur JIT a rencontré une limitation interne.
   at Celeste.Mod.ExtendedVariants.ExtendedVariantsModule.<>c__DisplayClass11_0.<ModNormalUpdate>b__0()
```

(Actually, it currently only works with a modded version of Everest that sits on my hard drive.)
