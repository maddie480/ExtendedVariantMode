using Celeste;
using Celeste.Mod;
using Celeste.Mod.UI;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Monocle;
using System;
using System.Collections.Generic;

namespace ExtendedVariants.UI {
    class ModOptionsEntries {

        private ExtendedVariantsSettings Settings => ExtendedVariantsModule.Settings;

        private TextMenu.Option<bool> masterSwitchOption;
        private TextMenu.Option<int> gravityOption;
        private TextMenu.Option<int> fallSpeedOption;
        private TextMenu.Option<int> jumpHeightOption;
        private TextMenu.Option<int> speedXOption;
        private TextMenu.Option<int> staminaOption;
        private TextMenu.Option<int> dashSpeedOption;
        private TextMenu.Option<int> dashCountOption;
        private TextMenu.Option<int> frictionOption;
        private TextMenu.Option<bool> disableWallJumpingOption;
        private TextMenu.Option<int> jumpCountOption;
        private TextMenu.Option<bool> refillJumpsOnDashRefillOption;
        private TextMenu.Option<bool> upsideDownOption;
        private TextMenu.Option<int> hyperdashSpeedOption;
        private TextMenu.Option<int> wallBouncingSpeedOption;
        private TextMenu.Option<int> dashLengthOption;
        private TextMenu.Option<bool> forceDuckOnGroundOption;
        private TextMenu.Option<bool> invertDashesOption;
        private TextMenu.Option<bool> disableNeutralJumpingOption;
        private TextMenu.Option<bool> changeVariantsRandomlyOption;
        private TextMenu.Option<bool> badelineChasersEverywhereOption;
        private TextMenu.Option<int> chaserCountOption;
        private TextMenu.Option<bool> affectExistingChasersOption;
        private TextMenu.Option<int> regularHiccupsOption;
        private TextMenu.Option<int> roomLightingOption;
        private TextMenu.Option<bool> oshiroEverywhereOption;
        private TextMenu.Option<int> windEverywhereOption;
        private TextMenu.Option<bool> snowballsEverywhereOption;
        private TextMenu.Option<int> snowballDelayOption;
        private TextMenu.Option<int> addSeekersOption;
        private TextMenu.Option<int> badelineLagOption;
        private TextMenu.Item resetToDefaultOption;
        private TextMenu.Item randomizerOptions;
        
        /// <summary>
        /// List of options shown for multipliers.
        /// </summary>
        private static int[] multiplierScale = new int[] {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100, 250, 500, 1000
        };

        /// <summary>
        /// Formats a multiplier (with no decimal point if not required).
        /// </summary>
        private Func<int, string> multiplierFormatter = option => {
            option = multiplierScale[option];
            if (option % 10 == 0) {
                return $"{option / 10f:n0}x";
            }
            return $"{option / 10f:n1}x";
        };

        /// <summary>
        /// Finds out the index of a multiplier in the multiplierScale table.
        /// If it is not present, will return the previous option.
        /// (For example, 18x will return the index for 10x.)
        /// </summary>
        /// <param name="option">The multiplier</param>
        /// <returns>The index of the multiplier in the multiplierScale table</returns>
        private int indexFromMultiplier(int option) {
            for (int index = 0; index < multiplierScale.Length - 1; index++) {
                if (multiplierScale[index + 1] > option) {
                    return index;
                }
            }

            return multiplierScale.Length - 1;
        }

        public void CreateAllOptions(TextMenu menu, bool inGame) {
            gravityOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_GRAVITY"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.Gravity), indexFromMultiplier(10)).Change(i => Settings.Gravity = multiplierScale[i]);
            fallSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FALLSPEED"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.FallSpeed), indexFromMultiplier(10)).Change(i => Settings.FallSpeed = multiplierScale[i]);
            jumpHeightOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_JUMPHEIGHT"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.JumpHeight), indexFromMultiplier(10)).Change(i => Settings.JumpHeight = multiplierScale[i]);
            speedXOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SPEEDX"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.SpeedX), indexFromMultiplier(10)).Change(i => Settings.SpeedX = multiplierScale[i]);
            staminaOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_STAMINA"),
                i => $"{i * 10}", 0, 50, Settings.Stamina, 11).Change(i => Settings.Stamina = i);
            dashSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHSPEED"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.DashSpeed), indexFromMultiplier(10)).Change(i => Settings.DashSpeed = multiplierScale[i]);
            dashCountOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHCOUNT"), i => {
                if (i == -1) {
                    return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                }
                return i.ToString();
            }, -1, 5, Settings.DashCount, 0).Change(i => Settings.DashCount = i);
            frictionOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FRICTION"),
                i => {
                    switch (i) {
                        case -1: return "0x";
                        case 0: return "0.05x";
                        default: return multiplierFormatter(i);
                    }
                }, -1, multiplierScale.Length - 1, Settings.Friction == -1 ? -1 : indexFromMultiplier(Settings.Friction), indexFromMultiplier(10) + 1)
                .Change(i => Settings.Friction = (i == -1 ? -1 : multiplierScale[i]));
            disableWallJumpingOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEWALLJUMPING"), Settings.DisableWallJumping, false)
                .Change(b => Settings.DisableWallJumping = b);
            jumpCountOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_JUMPCOUNT"),
                i => {
                    if (i == 6) {
                        return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INFINITE");
                    }
                    return i.ToString();
                }, 0, 6, Settings.JumpCount, 1).Change(i => {
                    Settings.JumpCount = i;
                    refreshOptionMenuEnabledStatus();
                });
            refillJumpsOnDashRefillOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_REFILLJUMPSONDASHREFILL"), Settings.RefillJumpsOnDashRefill, false)
                .Change(b => Settings.RefillJumpsOnDashRefill = b);
            upsideDownOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_UPSIDEDOWN"), Settings.UpsideDown, false)
                .Change(b => Settings.UpsideDown = b);
            hyperdashSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HYPERDASHSPEED"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.HyperdashSpeed), indexFromMultiplier(10)).Change(i => Settings.HyperdashSpeed = multiplierScale[i]);
            wallBouncingSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_WALLBOUNCINGSPEED"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.WallBouncingSpeed), indexFromMultiplier(10)).Change(i => Settings.WallBouncingSpeed = multiplierScale[i]);
            dashLengthOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHLENGTH"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.DashLength), indexFromMultiplier(10)).Change(i => Settings.DashLength = multiplierScale[i]);
            forceDuckOnGroundOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FORCEDUCKONGROUND"), Settings.ForceDuckOnGround, false)
                .Change(b => Settings.ForceDuckOnGround = b);
            invertDashesOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INVERTDASHES"), Settings.InvertDashes, false)
                .Change(b => Settings.InvertDashes = b);
            disableNeutralJumpingOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLENEUTRALJUMPING"), Settings.DisableNeutralJumping, false)
                .Change(b => Settings.DisableNeutralJumping = b);
            badelineChasersEverywhereOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINECHASERSEVERYWHERE"), Settings.BadelineChasersEverywhere, false)
                .Change(b => Settings.BadelineChasersEverywhere = b);
            affectExistingChasersOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AFFECTEXISTINGCHASERS"), Settings.AffectExistingChasers, false)
                .Change(b => Settings.AffectExistingChasers = b);
            chaserCountOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHASERCOUNT"),
                i => i.ToString(), 1, 10, Settings.ChaserCount, 0).Change(i => Settings.ChaserCount = i);
            changeVariantsRandomlyOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHANGEVARIANTSRANDOMLY"), Settings.ChangeVariantsRandomly, false)
                .Change(b => {
                    Settings.ChangeVariantsRandomly = b;
                    refreshOptionMenuEnabledStatus();
                });
            regularHiccupsOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_REGULARHICCUPS"),
                i => i == 0 ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLED") : multiplierFormatter(i).Replace("x", "s"), 
                0, multiplierScale.Length - 1, indexFromMultiplier(Settings.RegularHiccups), 0).Change(i => {
                    Settings.RegularHiccups = multiplierScale[i];
                    (ExtendedVariantsModule.Instance.VariantHandlers[ExtendedVariantsModule.Variant.RegularHiccups] as RegularHiccups).UpdateTimerFromSettings();
                });
            roomLightingOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ROOMLIGHTING"),
                i => {
                    if (i == -1) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                    return $"{i * 10}%";
                }, -1, 10, Settings.RoomLighting, 0).Change(i => {
                    Settings.RoomLighting = i;
                    if(Engine.Scene.GetType() == typeof(Level)) {
                        // currently in level, change lighting right away
                        Level lvl = (Engine.Scene as Level);
                        lvl.Lighting.Alpha = (i == -1 ? lvl.BaseLightingAlpha + lvl.Session.LightingAlphaAdd : 1 - (i / 10f));
                    }
                });
            oshiroEverywhereOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_OSHIROEVERYWHERE"), Settings.OshiroEverywhere, false)
                .Change(b => Settings.OshiroEverywhere = b);
            windEverywhereOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_WINDEVERYWHERE"),
                i => {
                    if (i == 0) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLED");
                    if (i == WindEverywhere.AvailableWindPatterns.Length + 1) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_WINDEVERYWHERE_RANDOM");
                    return Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_WINDEVERYWHERE_{WindEverywhere.AvailableWindPatterns[i - 1].ToString()}");
                },
                0, WindEverywhere.AvailableWindPatterns.Length + 1, Settings.WindEverywhere, 0)
                .Change(i => Settings.WindEverywhere = i);
            snowballsEverywhereOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SNOWBALLSEVERYWHERE"), Settings.SnowballsEverywhere, false)
                .Change(b => Settings.SnowballsEverywhere = b);
            snowballDelayOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SNOWBALLDELAY"),
                i => multiplierFormatter(i).Replace("x", "s"), 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.SnowballDelay), 8)
                .Change(i => Settings.SnowballDelay = multiplierScale[i]);
            addSeekersOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ADDSEEKERS"),
                i => i.ToString(), 0, 5, indexFromMultiplier(Settings.AddSeekers), 0).Change(i => Settings.AddSeekers = i);
            badelineLagOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINELAG"),
                i => i == 0 ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT") : multiplierFormatter(i).Replace("x", "s"),
                0, multiplierScale.Length - 1, indexFromMultiplier(Settings.BadelineLag), 0).Change(i => Settings.BadelineLag = multiplierScale[i]);

            // create the "master switch" option with specific enable/disable handling.
            masterSwitchOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MASTERSWITCH"), Settings.MasterSwitch)
                .Change(v => {
                    Settings.MasterSwitch = v;
                    if (!v) {
                        // We are disabling extended variants: reset values to their defaults.
                        ExtendedVariantsModule.Instance.ResetToDefaultSettings();
                        refreshOptionMenuValues();
                    }

                    refreshOptionMenuEnabledStatus();
                });

            // Add a button to easily revert to default values
            resetToDefaultOption = new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESETTODEFAULT")).Pressed(() => {
                ExtendedVariantsModule.Instance.ResetToDefaultSettings();
                refreshOptionMenuValues();
                refreshOptionMenuEnabledStatus();
            });

            if(inGame) {
                Level level = Engine.Scene as Level;

                // this is how it works in-game
                randomizerOptions = new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER")).Pressed(() => {
                    // close the Mod Options menu
                    menu.RemoveSelf();

                    // create our menu and prepare it
                    TextMenu randomizerMenu = OuiRandomizerOptions.BuildMenu();

                    randomizerMenu.OnESC = randomizerMenu.OnCancel = () => {
                        // close the randomizer options
                        Audio.Play(SFX.ui_main_button_back);
                        randomizerMenu.Close();

                        // and open the Mod Options menu back (this should work, right? we only removed it from the scene earlier, but it still exists and is intact)
                        // "what could possibly go wrong?" ~ famous last words
                        level.Add(menu);
                    };

                    randomizerMenu.OnPause = () => {
                        // we're unpausing, so close that menu, and save the mod Settings because the Mod Options menu won't do that for us
                        Audio.Play(SFX.ui_main_button_back);
                        randomizerMenu.CloseAndRun(Everest.SaveSettings(), () => {
                            level.Paused = false;
                            Engine.FreezeTimer = 0.15f;
                        });
                    };

                    // finally, add the menu to the scene
                    level.Add(randomizerMenu);
                });
            } else {
                // this is how it works in the main menu: way more simply than the in-game mess.
                randomizerOptions = new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER")).Pressed(() => {
                    OuiModOptions.Instance.Overworld.Goto<OuiRandomizerOptions>();
                });
            }

            refreshOptionMenuEnabledStatus();

            menu.Add(masterSwitchOption);
            menu.Add(resetToDefaultOption);

            addHeading(menu, "VERTICALSPEED");
            menu.Add(gravityOption);
            menu.Add(fallSpeedOption);

            addHeading(menu, "JUMPING");
            menu.Add(jumpHeightOption);
            menu.Add(wallBouncingSpeedOption);
            menu.Add(disableWallJumpingOption);
            menu.Add(jumpCountOption);
            menu.Add(refillJumpsOnDashRefillOption);

            addHeading(menu, "DASHING");
            menu.Add(dashSpeedOption);
            menu.Add(dashLengthOption);
            menu.Add(hyperdashSpeedOption);
            menu.Add(dashCountOption);

            addHeading(menu, "MOVING");
            menu.Add(speedXOption);
            menu.Add(frictionOption);

            addHeading(menu, "CHASERS");
            menu.Add(badelineChasersEverywhereOption);
            menu.Add(chaserCountOption);
            menu.Add(affectExistingChasersOption);
            menu.Add(badelineLagOption);

            addHeading(menu, "EVERYWHERE");
            menu.Add(oshiroEverywhereOption);
            menu.Add(windEverywhereOption);
            menu.Add(snowballsEverywhereOption);
            menu.Add(snowballDelayOption);
            menu.Add(addSeekersOption);

            addHeading(menu, "OTHER");
            menu.Add(staminaOption);
            menu.Add(upsideDownOption);
            menu.Add(disableNeutralJumpingOption);
            menu.Add(regularHiccupsOption);
            menu.Add(roomLightingOption);

            addHeading(menu, "TROLL");
            menu.Add(forceDuckOnGroundOption);
            menu.Add(invertDashesOption);

            addHeading(menu, "RANDOMIZER");
            menu.Add(changeVariantsRandomlyOption);
            menu.Add(randomizerOptions);
        }

        private void addHeading(TextMenu menu, String headingNameResource) {
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HEADING") + " - " + Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HEADING_" + headingNameResource)));
        }

        private void refreshOptionMenuValues() {
            setValue(gravityOption, 0, indexFromMultiplier(Settings.Gravity));
            setValue(fallSpeedOption, 0, indexFromMultiplier(Settings.FallSpeed));
            setValue(jumpHeightOption, 0, indexFromMultiplier(Settings.JumpHeight));
            setValue(speedXOption, 0, indexFromMultiplier(Settings.SpeedX));
            setValue(staminaOption, 0, Settings.Stamina);
            setValue(dashSpeedOption, 0, indexFromMultiplier(Settings.DashSpeed));
            setValue(dashCountOption, -1, Settings.DashCount);
            setValue(frictionOption, -1, Settings.Friction == -1 ? -1 : indexFromMultiplier(Settings.Friction));
            setValue(disableWallJumpingOption, Settings.DisableWallJumping);
            setValue(jumpCountOption, 0, Settings.JumpCount);
            setValue(refillJumpsOnDashRefillOption, Settings.RefillJumpsOnDashRefill);
            setValue(upsideDownOption, Settings.UpsideDown);
            setValue(hyperdashSpeedOption, 0, indexFromMultiplier(Settings.HyperdashSpeed));
            setValue(wallBouncingSpeedOption, 0, indexFromMultiplier(Settings.WallBouncingSpeed));
            setValue(dashLengthOption, 0, indexFromMultiplier(Settings.DashLength));
            setValue(forceDuckOnGroundOption, Settings.ForceDuckOnGround);
            setValue(invertDashesOption, Settings.InvertDashes);
            setValue(disableNeutralJumpingOption, Settings.DisableNeutralJumping);
            setValue(badelineChasersEverywhereOption, Settings.BadelineChasersEverywhere);
            setValue(chaserCountOption, 1, Settings.ChaserCount);
            setValue(affectExistingChasersOption, Settings.AffectExistingChasers);
            setValue(changeVariantsRandomlyOption, Settings.ChangeVariantsRandomly);
            setValue(regularHiccupsOption, 0, indexFromMultiplier(Settings.RegularHiccups));
            setValue(roomLightingOption, -1, Settings.RoomLighting);
            setValue(oshiroEverywhereOption, Settings.OshiroEverywhere);
            setValue(windEverywhereOption, 0, Settings.WindEverywhere);
            setValue(snowballsEverywhereOption, Settings.SnowballsEverywhere);
            setValue(snowballDelayOption, 0, Settings.SnowballDelay);
            setValue(addSeekersOption, 0, Settings.AddSeekers);
            setValue(badelineLagOption, 0, Settings.BadelineLag);
        }

        private void refreshOptionMenuEnabledStatus() {
            gravityOption.Disabled = !Settings.MasterSwitch;
            fallSpeedOption.Disabled = !Settings.MasterSwitch;
            jumpHeightOption.Disabled = !Settings.MasterSwitch;
            speedXOption.Disabled = !Settings.MasterSwitch;
            staminaOption.Disabled = !Settings.MasterSwitch;
            dashCountOption.Disabled = !Settings.MasterSwitch;
            dashSpeedOption.Disabled = !Settings.MasterSwitch;
            frictionOption.Disabled = !Settings.MasterSwitch;
            disableWallJumpingOption.Disabled = !Settings.MasterSwitch;
            jumpCountOption.Disabled = !Settings.MasterSwitch;
            refillJumpsOnDashRefillOption.Disabled = !Settings.MasterSwitch || Settings.JumpCount < 2;
            resetToDefaultOption.Disabled = !Settings.MasterSwitch;
            upsideDownOption.Disabled = !Settings.MasterSwitch;
            hyperdashSpeedOption.Disabled = !Settings.MasterSwitch;
            wallBouncingSpeedOption.Disabled = !Settings.MasterSwitch;
            dashLengthOption.Disabled = !Settings.MasterSwitch;
            forceDuckOnGroundOption.Disabled = !Settings.MasterSwitch;
            invertDashesOption.Disabled = !Settings.MasterSwitch;
            disableNeutralJumpingOption.Disabled = !Settings.MasterSwitch;
            badelineChasersEverywhereOption.Disabled = !Settings.MasterSwitch;
            chaserCountOption.Disabled = !Settings.MasterSwitch;
            affectExistingChasersOption.Disabled = !Settings.MasterSwitch;
            changeVariantsRandomlyOption.Disabled = !Settings.MasterSwitch;
            regularHiccupsOption.Disabled = !Settings.MasterSwitch;
            roomLightingOption.Disabled = !Settings.MasterSwitch;
            oshiroEverywhereOption.Disabled = !Settings.MasterSwitch;
            windEverywhereOption.Disabled = !Settings.MasterSwitch;
            snowballsEverywhereOption.Disabled = !Settings.MasterSwitch;
            snowballDelayOption.Disabled = !Settings.MasterSwitch;
            addSeekersOption.Disabled = !Settings.MasterSwitch;
            badelineLagOption.Disabled = !Settings.MasterSwitch;
            randomizerOptions.Disabled = !Settings.MasterSwitch || !Settings.ChangeVariantsRandomly;
        }

        private void setValue(TextMenu.Option<int> option, int min, int newValue) {
            newValue -= min;

            if(newValue != option.Index) {
                // replicate the vanilla behaviour
                option.PreviousIndex = option.Index;
                option.Index = newValue;
                option.ValueWiggler.Start();
            }
        }

        private void setValue(TextMenu.Option<bool> option, bool newValue) {
            if (newValue != (option.Index == 1)) {
                // replicate the vanilla behaviour
                option.PreviousIndex = option.Index;
                option.Index = newValue ? 1 : 0;
                option.ValueWiggler.Start();
            }
        }
    }
}
