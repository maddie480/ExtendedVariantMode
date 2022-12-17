using Celeste;
using Celeste.Mod;
using Celeste.Mod.UI;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.UI {
    /// <summary>
    /// This class is kind of a mess, but this is where all the options for Extended Variants are defined.
    /// This is called to build every menu or submenu containing extended variants options (parts to display or hide
    /// are managed by the various menus / submenus depending on where you are and your display preferences).
    /// </summary>
    public class ModOptionsEntries {
        private ExtendedVariantsSettings Settings => ExtendedVariantsModule.Settings;

        private TextMenu.Option<bool> masterSwitchOption;
        private TextMenu.Option<bool> optionsOutOfModOptionsMenuOption;
        private TextMenu.Option<bool> submenusForEachCategoryOption;
        private TextMenu.Option<bool> automaticallyResetVariantsOption;
        private TextMenuOptionExt<int> gravityOption;
        private TextMenuOptionExt<int> fallSpeedOption;
        private TextMenuOptionExt<int> jumpHeightOption;
        private TextMenuOptionExt<int> jumpDurationOption;
        private TextMenuOptionExt<int> speedXOption;
        private TextMenuOptionExt<int> swimmingSpeedOption;
        private TextMenuOptionExt<int> boostMultiplierOption;
        private TextMenuOptionExt<int> staminaOption;
        private TextMenuOptionExt<int> dashSpeedOption;
        private TextMenuOptionExt<bool> legacyDashSpeedBehaviorOption;
        private TextMenuOptionExt<int> dashCountOption;
        private TextMenuOptionExt<int> cornerCorrectionOption;
        private TextMenuOptionExt<bool> disableDashCooldownOption;
        private TextMenuOptionExt<bool> heldDashOption;
        private TextMenuOptionExt<int> frictionOption;
        private TextMenuOptionExt<int> airFrictionOption;
        private TextMenuOptionExt<bool> disableWallJumpingOption;
        private TextMenuOptionExt<bool> disableClimbJumpingOption;
        private TextMenuOptionExt<bool> disableJumpingOutOfWaterOption;
        private TextMenuOptionExt<int> jumpCountOption;
        private TextMenuOptionExt<bool> refillJumpsOnDashRefillOption;
        private TextMenuOptionExt<bool> resetJumpCountOnGroundOption;
        private TextMenuOptionExt<bool> upsideDownOption;
        private TextMenuOptionExt<int> hyperdashSpeedOption;
        private TextMenuOptionExt<int> explodeLaunchSpeedOption;
        private TextMenuOptionExt<int> horizontalSpringBounceDurationOption;
        private TextMenuOptionExt<int> horizontalWallJumpDurationOption;
        private TextMenuOptionExt<bool> disableSuperBoostsOption;
        private TextMenuOptionExt<bool> dontRefillStaminaOnGroundOption;
        private TextMenuOptionExt<int> wallBouncingSpeedOption;
        private TextMenuOptionExt<int> wallSlidingSpeedOption;
        private TextMenuOptionExt<int> dashLengthOption;
        private TextMenuOptionExt<int> dashTimerMultiplierOption;
        private TextMenuOptionExt<int> dashDirectionOption;
        private Celeste.TextMenuExt.SubMenu dashDirectionsSubMenu;
        private TextMenuOptionExt<bool> forceDuckOnGroundOption;
        private TextMenuOptionExt<bool> invertDashesOption;
        private TextMenuOptionExt<bool> invertGrabOption;
        private TextMenuOptionExt<bool> invertHorizontalControlsOption;
        private TextMenuOptionExt<bool> invertVerticalControlsOption;
        private TextMenuOptionExt<bool> disableNeutralJumpingOption;
        private TextMenuOptionExt<bool> changeVariantsRandomlyOption;
        private TextMenuOptionExt<bool> badelineChasersEverywhereOption;
        private TextMenuOptionExt<int> chaserCountOption;
        private TextMenuOptionExt<bool> affectExistingChasersOption;
        private TextMenuOptionExt<bool> badelineBossesEverywhereOption;
        private TextMenuOptionExt<int> badelineAttackPatternOption;
        private TextMenuOptionExt<bool> changePatternOfExistingBossesOption;
        private TextMenuOptionExt<int> firstBadelineSpawnRandomOption;
        private TextMenuOptionExt<int> badelineBossCountOption;
        private TextMenuOptionExt<int> badelineBossNodeCountOption;
        private TextMenuOptionExt<int> regularHiccupsOption;
        private TextMenuOptionExt<int> hiccupStrengthOption;
        private TextMenuOptionExt<int> roomLightingOption;
        private TextMenuOptionExt<int> roomBloomOption;
        private TextMenuOptionExt<int> glitchEffectOption;
        private TextMenuOptionExt<int> anxietyEffectOption;
        private TextMenuOptionExt<int> blurLevelOption;
        private TextMenuOptionExt<int> backgroundBlurLevelOption;
        private TextMenuOptionExt<int> zoomLevelOption;
        private TextMenuOptionExt<bool> everythingIsUnderwaterOption;
        private TextMenuOptionExt<bool> oshiroEverywhereOption;
        private TextMenuOptionExt<int> oshiroCountOption;
        private TextMenuOptionExt<int> reverseOshiroCountOption;
        private TextMenuOptionExt<bool> disableOshiroSlowdownOption;
        private TextMenuOptionExt<int> windEverywhereOption;
        private TextMenuOptionExt<bool> snowballsEverywhereOption;
        private TextMenuOptionExt<int> snowballDelayOption;
        private TextMenuOptionExt<int> addSeekersOption;
        private TextMenuOptionExt<bool> disableSeekerSlowdownOption;
        private TextMenuOptionExt<bool> theoCrystalsEverywhereOption;
        private TextMenuOptionExt<bool> allowThrowingTheoOffscreenOption;
        private TextMenuOptionExt<bool> allowLeavingTheoBehindOption;
        private TextMenuOptionExt<int> badelineLagOption;
        private TextMenuOptionExt<int> delayBetweenBadelinesOption;
        private TextMenuOptionExt<bool> allStrawberriesAreGoldensOption;
        private TextMenuOptionExt<int> dontRefillDashOnGroundOption;
        private TextMenuOptionExt<bool> disableRefillsOnScreenTransitionOption;
        private TextMenuOptionExt<bool> restoreDashesOnRespawnOption;
        private TextMenuOptionExt<int> gameSpeedOption;
        private TextMenuOptionExt<int> colorGradingOption;
        private TextMenuOptionExt<int> jellyfishEverywhereOption;
        private TextMenuOptionExt<bool> risingLavaEverywhereOption;
        private TextMenuOptionExt<int> risingLavaSpeedOption;
        private TextMenuOptionExt<bool> bounceEverywhereOption;
        private TextMenuOptionExt<int> jungleSpidersEverywhereOption;
        private TextMenuOptionExt<int> superdashSteeringSpeedOption;
        private TextMenuOptionExt<int> screenShakeIntensityOption;
        private TextMenuOptionExt<int> backgroundBrightnessOption;
        private TextMenuOptionExt<bool> disableMadelineSpotlightOption;
        private TextMenuOptionExt<bool> disableKeysSpotlightOption;
        private TextMenuOptionExt<int> foregroundEffectOpacityOption;
        private TextMenuOptionExt<bool> madelineIsSilhouetteOption;
        private TextMenuOptionExt<bool> madelineHasPonytailOption;
        private TextMenuOptionExt<bool> dashTrailAllTheTimeOption;
        private TextMenuOptionExt<int> disableClimbingUpOrDownOption;
        private TextMenuOptionExt<int> pickupDurationOption;
        private TextMenuOptionExt<int> minimumDelayBeforeThrowingOption;
        private TextMenuOptionExt<int> delayBeforeRegrabbingOption;
        private TextMenuOptionExt<bool> friendlyBadelineFollowerOption;
        private TextMenuOptionExt<bool> displayDashCountOption;
        private TextMenuOptionExt<bool> everyJumpIsUltraOption;
        private TextMenuOptionExt<int> madelineBackpackModeOption;
        private TextMenuOptionExt<int> coyoteTimeOption;
        private TextMenuOptionExt<bool> noFreezeFramesOption;
        private TextMenuOptionExt<bool> preserveExtraDashesUnderwaterOption;
        private TextMenuOptionExt<bool> alwaysInvisibleOption;
        private TextMenuOptionExt<int> displaySpeedometerOption;
        private TextMenu.Item resetExtendedToDefaultOption;
        private TextMenu.Item resetVanillaToDefaultOption;
        private TextMenu.Item randomizerOptions;

        private List<TextMenu.Item> allOptions;

        private static int[] badelineBossesPatternsOptions = {
            0, // random (technical option)
            1, // slow shots
            2, // beam => shot
            3, // 5 fast double shots
            4, // 5 fast double shots => beam
            5, // beam => 3 fast double shots
            // 6 is beam w/ 0.7s pause
            // 7 is like 1 except it shoots every 1.6s instead of 1.95s...
            // 8 are beams w/ 0.9s pause
            9, // double shots
            10, // this is literally nothing, but hey, this is an option after all
            // 11 are really slow shots (every 2.5s)
            // 12 doesn't actually exist
            // 13 is like 1 but only from the 2nd node, and we only have 1 anyway
            14, // beams w/ 0.5s pause
            15 // beams w/ 1.4s pause
        };

        private int indexFromPatternValue(int option) {
            for (int index = 0; index < badelineBossesPatternsOptions.Length - 1; index++) {
                if (badelineBossesPatternsOptions[index + 1] > option) {
                    return index;
                }
            }

            return badelineBossesPatternsOptions.Length - 1;
        }

        private static float[] multiplierScale = new float[] {
            0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2,
            2.5f, 3, 3.5f, 4, 4.5f, 5, 6, 7, 8, 9, 10, 25, 50, 100
        };

        private static float[] multiplierScaleFriction = new float[] {
            0, 0.05f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2,
            2.5f, 3, 3.5f, 4, 4.5f, 5, 6, 7, 8, 9, 10, 25, 50, 100
        };

        private static float[] multiplierScaleBadelineLag = new float[] {
            0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.55f, 1.6f, 1.7f, 1.8f, 1.9f, 2,
            2.5f, 3, 3.5f, 4, 4.5f, 5, 6, 7, 8, 9, 10, 25, 50, 100
        };

        private static float[] multiplierScaleWithNegatives = new float[] {
            -100, -50, -25, -10, -9, -8, -7, -6, -5, -4.5f, -4, -3.5f, -3, -2.5f,
            -2, -1.9f, -1.8f, -1.7f, -1.6f, -1.5f, -1.4f, -1.3f, -1.2f, -1.1f, -1, -0.9f, -0.8f, -0.7f, -0.6f, -0.5f, -0.4f, -0.3f, -0.2f, -0.1f,
            0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2,
            2.5f, 3, 3.5f, 4, 4.5f, 5, 6, 7, 8, 9, 10, 25, 50, 100
        };

        private static TextMenuOptionExt<int> getScaleOption<T>(string name, string suffix, T value, int defIndex, T[] scale, Action<T> setter, Func<T, string> formatter = null) where T : IComparable {
            List<T> choices = new List<T>(scale);

            // go forward until choices[position] is higher or equal to our value
            int position = 0;
            while (position < choices.Count && value.CompareTo(choices[position]) > 0) {
                position++;
            }

            // handle off-scale values
            if (position == choices.Count) {
                // we are higher than the entire scale => add ourselves to the end
                choices.Add(value);
            } else if (value.CompareTo(choices[position]) != 0) {
                // we stopped on a value that is higher than us (not equal) => insert ourselves there
                choices.Insert(position, value);

                if (position <= defIndex) {
                    // we pushed the default index to the right!
                    defIndex++;
                }
            }

            TextMenuExt.Slider slider = new TextMenuExt.Slider(Dialog.Clean(name),
                i => {
                    T valueToFormat = choices[i];
                    if (formatter != null) {
                        return formatter(valueToFormat);
                    }

                    if (valueToFormat is float f && (int) f == f) {
                        // float that represents an integer value, like 5.0f => 5
                        return $"{f:n0}{suffix}";
                    }
                    return valueToFormat + suffix;
                },
                0, choices.Count - 1, position, defIndex);
            slider.Change(i => setter(choices[i]));
            return slider;
        }

        public enum VariantCategory {
            All, Movement, GameElements, Visual, GameplayTweaks, None
        }

        /// <summary>
        /// Builds the HUGE extended variants options menu, or part of it.
        /// </summary>
        /// <param name="category">The category to include in the menu, All to include them all with subheaders</param>
        /// <param name="includeMasterSwitch">If the master switch and Reset to default options should be included</param>
        /// <param name="includeCategorySubmenus">If submenus per-category should be included</param>
        /// <param name="includeRandomizer">If randomizer options should be included (with sub-header)</param>
        /// <param name="submenuBackAction">An Action to come back to the current menu from a submenu</param>
        /// <param name="menu">The menu to inject the options to</param>
        /// <param name="inGame">true if we are in the pause menu, false if we are in the overworld</param>
        /// <param name="forceEnabled">true if extended variants cannot be disabled because the map being played requires them. Only useful if includeMasterSwitch = true</param>
        public void CreateAllOptions(VariantCategory category, bool includeMasterSwitch, bool includeCategorySubmenus, bool includeRandomizer, bool includeOpenSubmenuButton,
            Action submenuBackAction, TextMenu menu, bool inGame, bool forceEnabled) {

            if (includeMasterSwitch) {
                // create the "master switch" option with specific enable/disable handling.
                masterSwitchOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MASTERSWITCH"), Settings.MasterSwitch) {
                    Disabled = forceEnabled // if variants are force-enabled, you can't disable them, so you have to disable the master switch.
                }
                    .Change(v => {
                        Settings.MasterSwitch = v;
                        refreshOptionMenuEnabledStatus();

                        // (de)activate all hooks!
                        if (v) ExtendedVariantsModule.Instance.HookStuff();
                        else ExtendedVariantsModule.Instance.UnhookStuff();
                    });

                if (!inGame) {
                    optionsOutOfModOptionsMenuOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_OPTIONSOUTOFMODOPTIONSMENU"), Settings.OptionsOutOfModOptionsMenuEnabled)
                        .Change(b => {
                            Settings.OptionsOutOfModOptionsMenuEnabled = b;
                            if (!Settings.SubmenusForEachCategoryEnabled) reloadModOptions();
                        });
                    submenusForEachCategoryOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SUBMENUFOREACHCATEGORY"), Settings.SubmenusForEachCategoryEnabled)
                        .Change(b => {
                            Settings.SubmenusForEachCategoryEnabled = b;
                            reloadModOptions();
                        });
                    automaticallyResetVariantsOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AUTOMATICALLYRESETVARIANTS"), Settings.AutomaticallyResetVariants)
                        .Change(b => Settings.AutomaticallyResetVariants = b);
                }
            }

            if (category == VariantCategory.All || includeCategorySubmenus) {
                // Add buttons to easily revert to default values (vanilla and extended variants)
                if (inGame) {
                    resetVanillaToDefaultOption = new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESETTODEFAULT_VANILLA")).Pressed(() => {
                        ExtendedVariantsModule.Instance.ResetVariantsToDefaultSettings(isVanilla: true);
                    });
                }

                resetExtendedToDefaultOption = new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESETTODEFAULT_EXTENDED")).Pressed(() => {
                    ExtendedVariantsModule.Instance.ResetVariantsToDefaultSettings(isVanilla: false);
                    resetAllOptionsToDefault();
                    refreshOptionMenuEnabledStatus();
                });
            }

            // ======
            if (category == VariantCategory.All || category == VariantCategory.Movement) {
                // Vertical Speed
                gravityOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_GRAVITY", "x", Settings.Gravity, 10, multiplierScale, f => Settings.Gravity = f);
                fallSpeedOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_FALLSPEED", "x", Settings.FallSpeed, 10, multiplierScale, f => {
                    Settings.FallSpeed = f;
                    ((FallSpeed) Instance.VariantHandlers[Variant.FallSpeed]).OnVariantChanged();
                });

                // Jumping
                jumpHeightOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_JUMPHEIGHT", "x", Settings.JumpHeight, 10, multiplierScale, f => Settings.JumpHeight = f);
                jumpDurationOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_JUMPDURATION", "x", Settings.JumpDuration, 10, multiplierScale, f => Settings.JumpDuration = f);
                wallBouncingSpeedOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_WALLBOUNCINGSPEED", "x", Settings.WallBouncingSpeed, 10, multiplierScale, f => Settings.WallBouncingSpeed = f);
                disableWallJumpingOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEWALLJUMPING"), Settings.DisableWallJumping, false)
                    .Change(b => Settings.DisableWallJumping = b);
                disableClimbJumpingOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLECLIMBJUMPING"), Settings.DisableClimbJumping, false)
                    .Change(b => Settings.DisableClimbJumping = b);
                disableJumpingOutOfWaterOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEJUMPINGOUTOFWATER"), Settings.DisableJumpingOutOfWater, false)
                    .Change(b => Settings.DisableJumpingOutOfWater = b);
                disableNeutralJumpingOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLENEUTRALJUMPING"), Settings.DisableNeutralJumping, false)
                    .Change(b => Settings.DisableNeutralJumping = b);
                jumpCountOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_JUMPCOUNT", "", Settings.JumpCount, 1, new int[] { 0, 1, 2, 3, 4, 5, int.MaxValue }, i => {
                    Settings.JumpCount = i;
                    refreshOptionMenuEnabledStatus();
                }, i => {
                    if (i == int.MaxValue) {
                        return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INFINITE");
                    }
                    return i.ToString();
                });
                refillJumpsOnDashRefillOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_REFILLJUMPSONDASHREFILL"), Settings.RefillJumpsOnDashRefill, false)
                    .Change(b => Settings.RefillJumpsOnDashRefill = b);
                resetJumpCountOnGroundOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESETJUMPCOUNTONGROUND"), Settings.ResetJumpCountOnGround, true)
                    .Change(b => Settings.ResetJumpCountOnGround = b);
                everyJumpIsUltraOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_EVERYJUMPISULTRA"), Settings.EveryJumpIsUltra, false)
                    .Change(b => Settings.EveryJumpIsUltra = b);
                coyoteTimeOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_COYOTETIME", "x", Settings.CoyoteTime, 10, multiplierScale, f => Settings.CoyoteTime = f);

                // Dashing
                dashSpeedOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_DASHSPEED", "x", Settings.DashSpeed, 10, multiplierScale, f => Settings.DashSpeed = f);
                legacyDashSpeedBehaviorOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_LEGACYDASHSPEEDBEHAVIOR"), Settings.LegacyDashSpeedBehavior, false)
                    .Change(value => {
                        Settings.LegacyDashSpeedBehavior = value;

                        // hot swap the "dash speed" variant handler
                        Instance.VariantHandlers[Variant.DashSpeed].Unload();
                        Instance.VariantHandlers[Variant.DashSpeed] = Settings.LegacyDashSpeedBehavior ? (AbstractExtendedVariant) new DashSpeedOld() : new DashSpeed();
                        Instance.VariantHandlers[Variant.DashSpeed].Load();
                    });
                dashLengthOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_DASHLENGTH", "x", Settings.DashLength, 10, multiplierScale, f => Settings.DashLength = f);
                dashTimerMultiplierOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_DASHTIMERMULTIPLIER", "x", Settings.DashTimerMultiplier, 10, multiplierScale, f => Settings.DashTimerMultiplier = f);
                dashDirectionOption = (TextMenuExt.Slider) new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION"),
                    i => Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION_{i}"), 0, 3, GetDashDirectionIndex(), 0).Change(i => {
                        switch (i) {
                            case 1:
                                Settings.AllowedDashDirections = new bool[][] {
                                    new bool[] { false, true, false },
                                    new bool[] { true, true, true },
                                    new bool[] { false, true, false }
                                };
                                break;
                            case 2:
                                Settings.AllowedDashDirections = new bool[][] {
                                    new bool[] { true, false, true },
                                    new bool[] { false, true, false },
                                    new bool[] { true, false, true }
                                };
                                break;
                            case 3:
                                Settings.AllowedDashDirections = new bool[][] {
                                    new bool[] { false, false, false },
                                    new bool[] { false, true, false },
                                    new bool[] { false, false, false }
                                };
                                break;
                            default:
                                Settings.AllowedDashDirections = new bool[][] { new bool[] { true, true, true }, new bool[] { true, true, true }, new bool[] { true, true, true } };
                                break;
                        }
                        refreshOptionMenuEnabledStatus();

                        if (i == 3) {
                            Settings.AllowedDashDirections = new bool[][] { new bool[] { true, true, true }, new bool[] { true, true, true }, new bool[] { true, true, true } };
                        }
                    });

                // build the dash direction submenu.
                dashDirectionsSubMenu = new Celeste.TextMenuExt.SubMenu(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION_ALLOWED"), enterOnSelect: false);
                string[,] directionNames = new string[,] { { "TOPLEFT", "TOP", "TOPRIGHT" }, { "LEFT", "CENTER", "RIGHT" }, { "BOTTOMLEFT", "BOTTOM", "BOTTOMRIGHT" } };

                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) {
                        if (i == 1 && j == 1) continue;

                        int a = i, b = j;

                        TextMenu.OnOff toggle = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION_" + directionNames[i, j]), Settings.AllowedDashDirections[i][j]);
                        toggle.Change(value => Settings.AllowedDashDirections[a][b] = value);
                        dashDirectionsSubMenu.Add(toggle);
                    }
                }

                hyperdashSpeedOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_HYPERDASHSPEED", "x", Settings.HyperdashSpeed, 10, multiplierScale, f => Settings.HyperdashSpeed = f);
                dashCountOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_DASHCOUNT", "", Settings.DashCount, 0, new int[] { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, i => Settings.DashCount = i, i => {
                    if (i == -1) {
                        return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                    }
                    return i.ToString();
                });
                heldDashOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HELDDASH"), Settings.HeldDash, false)
                    .Change(b => Settings.HeldDash = b);

                dontRefillDashOnGroundOption = (TextMenuExt.Slider) new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DONTREFILLDASHONGROUND"),
                    i => new string[] { Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT"), Dialog.Clean("OPTIONS_ON"), Dialog.Clean("OPTIONS_OFF") }[i],
                    0, 2, (int) Settings.DashRefillOnGroundState, 0)
                    .Change(i => Settings.DashRefillOnGroundState = (DontRefillDashOnGround.DashRefillOnGroundConfiguration) i);
                disableRefillsOnScreenTransitionOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEREFILLSONSCREENTRANSITION"), Settings.DisableRefillsOnScreenTransition, false)
                    .Change(b => Settings.DisableRefillsOnScreenTransition = b);
                dontRefillStaminaOnGroundOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DONTREFILLSTAMINAONGROUND"), Settings.DontRefillStaminaOnGround, false)
                    .Change(b => Settings.DontRefillStaminaOnGround = b);
                restoreDashesOnRespawnOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESTOREDASHESONRESPAWN"), Settings.RestoreDashesOnRespawn, false)
                    .Change(b => Settings.RestoreDashesOnRespawn = b);
                superdashSteeringSpeedOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_SUPERDASHSTEERINGSPEED", "x", Settings.SuperdashSteeringSpeed, 10, multiplierScale, f => Settings.SuperdashSteeringSpeed = f);
                preserveExtraDashesUnderwaterOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_PRESERVEEXTRADASHESUNDERWATER"), Settings.PreserveExtraDashesUnderwater, true)
                    .Change(b => Settings.PreserveExtraDashesUnderwater = b);
                disableDashCooldownOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEDASHCOOLDOWN"), Settings.DisableDashCooldown, false)
                    .Change(b => Settings.DisableDashCooldown = b);

                cornerCorrectionOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_CORNERCORRECTION", "", Settings.CornerCorrection, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 },
                    i => Settings.CornerCorrection = i, i => i + "px");

                // Moving
                speedXOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_SPEEDX", "x", Settings.SpeedX, 10, multiplierScale, f => Settings.SpeedX = f);
                swimmingSpeedOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_SWIMMINGSPEED", "x", Settings.SwimmingSpeed, 10, multiplierScale, f => Settings.SwimmingSpeed = f);
                boostMultiplierOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_BOOSTMULTIPLIER", "x", Settings.BoostMultiplier, 44, multiplierScaleWithNegatives, f => Settings.BoostMultiplier = f);
                explodeLaunchSpeedOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_EXPLODELAUNCHSPEED", "x", Settings.ExplodeLaunchSpeed, 10, multiplierScale, f => Settings.ExplodeLaunchSpeed = f);
                wallSlidingSpeedOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_WALLSLIDINGSPEED", "x", Settings.WallSlidingSpeed, 10, multiplierScale, f => Settings.WallSlidingSpeed = f);
                disableSuperBoostsOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLESUPERBOOSTS"), Settings.DisableSuperBoosts, false)
                    .Change(b => Settings.DisableSuperBoosts = b);
                frictionOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_FRICTION", "x", Settings.Friction, 11, multiplierScaleFriction, f => Settings.Friction = f);
                airFrictionOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_AIRFRICTION", "x", Settings.AirFriction, 11, multiplierScaleFriction, f => Settings.AirFriction = f);

                disableClimbingUpOrDownOption = (TextMenuExt.Slider) new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLECLIMBINGUPORDOWN"),
                    i => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLECLIMBINGUPORDOWN_" + Enum.GetNames(typeof(DisableClimbingUpOrDown.ClimbUpOrDownOptions))[i]),
                    0, Enum.GetNames(typeof(DisableClimbingUpOrDown.ClimbUpOrDownOptions)).Length - 1, (int) Settings.DisableClimbingUpOrDown, 0)
                    .Change(i => Settings.DisableClimbingUpOrDown = (DisableClimbingUpOrDown.ClimbUpOrDownOptions) i);

                horizontalSpringBounceDurationOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_HORIZONTALSPRINGBOUNCEDURATION", "x", Settings.HorizontalSpringBounceDuration, 10, multiplierScale, f => Settings.HorizontalSpringBounceDuration = f);
                horizontalWallJumpDurationOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_HORIZONTALWALLJUMPDURATION", "x", Settings.HorizontalWallJumpDuration, 10, multiplierScale, f => Settings.HorizontalWallJumpDuration = f);

                // Holdables
                pickupDurationOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_PICKUPDURATION", "x", Settings.PickupDuration, 10, multiplierScale, f => Settings.PickupDuration = f);
                minimumDelayBeforeThrowingOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_MINIMUMDELAYBEFORETHROWING", "x", Settings.MinimumDelayBeforeThrowing, 10, multiplierScale, f => Settings.MinimumDelayBeforeThrowing = f);
                delayBeforeRegrabbingOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_DELAYBEFOREREGRABBING", "x", Settings.DelayBeforeRegrabbing, 10, multiplierScale, f => Settings.DelayBeforeRegrabbing = f);
            }

            // ======
            if (category == VariantCategory.All || category == VariantCategory.GameElements) {
                // Badeline Chasers
                badelineChasersEverywhereOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINECHASERSEVERYWHERE"), Settings.BadelineChasersEverywhere, false)
                    .Change(b => Settings.BadelineChasersEverywhere = b);
                chaserCountOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_CHASERCOUNT", "", Settings.ChaserCount, 0, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, i => Settings.ChaserCount = i);
                affectExistingChasersOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AFFECTEXISTINGCHASERS"), Settings.AffectExistingChasers, false)
                    .Change(b => Settings.AffectExistingChasers = b);
                badelineLagOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_BADELINELAG", "s", Settings.BadelineLag, 16, multiplierScaleBadelineLag, f => Settings.BadelineLag = f);
                delayBetweenBadelinesOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_DELAYBETWEENBADELINES", "s", Settings.DelayBetweenBadelines, 4, multiplierScale, f => Settings.DelayBetweenBadelines = f);

                // Badeline Bosses
                badelineBossesEverywhereOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINEBOSSESEVERYWHERE"), Settings.BadelineBossesEverywhere, false)
                    .Change(b => {
                        Settings.BadelineBossesEverywhere = b;
                        refreshOptionMenuEnabledStatus();
                    });
                badelineAttackPatternOption = (TextMenuExt.Slider) new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINEATTACKPATTERN"),
                    i => {
                        if (i == 0) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_WINDEVERYWHERE_RANDOM");
                        return Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_BADELINEPATTERN_{badelineBossesPatternsOptions[i]}");
                    }, 0, badelineBossesPatternsOptions.Length - 1, indexFromPatternValue(Settings.BadelineAttackPattern), 0)
                    .Change(i => Settings.BadelineAttackPattern = badelineBossesPatternsOptions[i]);
                changePatternOfExistingBossesOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHANGEPATTERNSOFEXISTINGBOSSES"), Settings.ChangePatternsOfExistingBosses, false)
                    .Change(b => Settings.ChangePatternsOfExistingBosses = b);
                firstBadelineSpawnRandomOption = (TextMenuExt.Slider) new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FIRSTBADELINESPAWNRANDOM"),
                    i => i == 1 ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FIRSTBADELINESPAWNRANDOM_ON") : Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FIRSTBADELINESPAWNRANDOM_OFF"),
                    0, 1, Settings.FirstBadelineSpawnRandom ? 1 : 0, 0).Change(i => Settings.FirstBadelineSpawnRandom = (i != 0));
                badelineBossCountOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_BADELINEBOSSCOUNT", "", Settings.BadelineBossCount, 0, new int[] { 1, 2, 3, 4, 5 }, i => Settings.BadelineBossCount = i);
                badelineBossNodeCountOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_BADELINEBOSSNODECOUNT", "", Settings.BadelineBossNodeCount, 0, new int[] { 1, 2, 3, 4, 5 }, i => Settings.BadelineBossNodeCount = i);

                // Oshiro
                oshiroEverywhereOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_OSHIROEVERYWHERE"), Settings.OshiroEverywhere, false)
                    .Change(b => {
                        Settings.OshiroEverywhere = b;
                        refreshOptionMenuEnabledStatus();
                    });
                oshiroCountOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_OSHIROCOUNT", "", Settings.OshiroCount, 1, new int[] { 0, 1, 2, 3, 4, 5 }, i => Settings.OshiroCount = i);
                reverseOshiroCountOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_REVERSEOSHIROCOUNT", "", Settings.ReverseOshiroCount, 0, new int[] { 0, 1, 2, 3, 4, 5 }, i => Settings.ReverseOshiroCount = i);
                disableOshiroSlowdownOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEOSHIROSLOWDOWN"), Settings.DisableOshiroSlowdown, false)
                    .Change(b => Settings.DisableOshiroSlowdown = b);

                // Other elements
                windEverywhereOption = (TextMenuExt.Slider) new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_WINDEVERYWHERE"),
                    i => Dialog.Clean(i == 0 ? "MODOPTIONS_EXTENDEDVARIANTS_DISABLED" : "MODOPTIONS_EXTENDEDVARIANTS_WINDEVERYWHERE_" + Enum.GetNames(typeof(WindEverywhere.WindPattern))[i]),
                    0, Enum.GetNames(typeof(WindEverywhere.WindPattern)).Length - 1, (int) Settings.WindEverywhere, 0)
                    .Change(i => Settings.WindEverywhere = (WindEverywhere.WindPattern) i);
                snowballsEverywhereOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SNOWBALLSEVERYWHERE"), Settings.SnowballsEverywhere, false)
                    .Change(b => Settings.SnowballsEverywhere = b);
                snowballDelayOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_SNOWBALLDELAY", "s", Settings.SnowballDelay, 8, multiplierScale, f => Settings.SnowballDelay = f);
                addSeekersOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_ADDSEEKERS", "", Settings.AddSeekers, 0, new int[] { 0, 1, 2, 3, 4, 5 }, i => Settings.AddSeekers = i);
                disableSeekerSlowdownOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLESEEKERSLOWDOWN"), Settings.DisableSeekerSlowdown, false)
                    .Change(b => {
                        Settings.DisableSeekerSlowdown = b;
                        if (b && Engine.Scene is Level level && level.Tracker.CountEntities<Seeker>() != 0) {
                            // since we are in a map with seekers and we are killing slowdown, set speed to 1 to be sure we aren't making the current slowdown permanent. :maddyS:
                            Engine.TimeRate = 1f;
                        }
                    });
                theoCrystalsEverywhereOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_THEOCRYSTALSEVERYWHERE"), Settings.TheoCrystalsEverywhere, false)
                    .Change(b => {
                        Settings.TheoCrystalsEverywhere = b;
                        refreshOptionMenuEnabledStatus();
                    });
                allowThrowingTheoOffscreenOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_allowThrowingTheoOffscreen"), Settings.AllowThrowingTheoOffscreen, false)
                    .Change(b => Settings.AllowThrowingTheoOffscreen = b);
                allowLeavingTheoBehindOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_allowLeavingTheoBehind"), Settings.AllowLeavingTheoBehind, false)
                    .Change(b => Settings.AllowLeavingTheoBehind = b);
                jellyfishEverywhereOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_JELLYFISHEVERYWHERE", "", Settings.JellyfishEverywhere, 0, new int[] { 0, 1, 2, 3 }, i => Settings.JellyfishEverywhere = i);
                risingLavaEverywhereOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RISINGLAVAEVERYWHERE"), Settings.RisingLavaEverywhere, false)
                    .Change(b => Settings.RisingLavaEverywhere = b);
                risingLavaSpeedOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_RISINGLAVASPEED", "x", Settings.RisingLavaSpeed, 10, multiplierScale, f => Settings.RisingLavaSpeed = f);

                jungleSpidersEverywhereOption = (TextMenuExt.Slider) new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_JUNGLESPIDERSEVERYWHERE"),
                    i => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_JUNGLESPIDERSEVERYWHERE_" + Enum.GetNames(typeof(JungleSpidersEverywhere.SpiderType))[i]),
                    0, Enum.GetNames(typeof(JungleSpidersEverywhere.SpiderType)).Length - 1, (int) Settings.JungleSpidersEverywhere, 0)
                    .Change(i => Settings.JungleSpidersEverywhere = (JungleSpidersEverywhere.SpiderType) i);
            }

            // ======
            if (category == VariantCategory.All || category == VariantCategory.Visual) {
                upsideDownOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_UPSIDEDOWN"), Settings.UpsideDown, false)
                    .Change(b => Settings.UpsideDown = b);

                roomLightingOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_ROOMLIGHTING", "", Settings.RoomLighting, 0, new float[] {
                    -1f, 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f
                }, f => {
                    Settings.RoomLighting = f;

                    if (Engine.Scene.GetType() == typeof(Level)) {
                        // currently in level, change lighting right away
                        Level lvl = (Engine.Scene as Level);
                        lvl.Lighting.Alpha = (f == -1 ? (lvl.DarkRoom ? lvl.Session.DarkRoomAlpha : lvl.BaseLightingAlpha + lvl.Session.LightingAlphaAdd) : 1 - f);
                    }
                }, f => {
                    if (f == -1) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                    return $"{f * 100}%";
                });

                backgroundBrightnessOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_BACKGROUNDBRIGHTNESS", "", Settings.BackgroundBrightness, 10, new float[] {
                    0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f
                }, f => Settings.BackgroundBrightness = f, f => $"{f * 100}%");

                foregroundEffectOpacityOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_FOREGROUNDEFFECTOPACITY", "", Settings.ForegroundEffectOpacity, 10, new float[] {
                    0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f
                }, f => Settings.ForegroundEffectOpacity = f, f => $"{f * 100}%");

                disableMadelineSpotlightOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEMADELINESPOTLIGHT"), Settings.DisableMadelineSpotlight, false)
                    .Change(b => Settings.DisableMadelineSpotlight = b);
                disableKeysSpotlightOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEKEYSSPOTLIGHT"), Settings.DisableKeysSpotlight, false)
                    .Change(b => {
                        Settings.DisableKeysSpotlight = b;
                        ((DisableKeysSpotlight) ExtendedVariantsModule.Instance.VariantHandlers[Variant.DisableKeysSpotlight]).OnSettingChanged();
                    });
                madelineIsSilhouetteOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MADELINEISSILHOUETTE"), Settings.MadelineIsSilhouette, false)
                    .Change(b => Settings.MadelineIsSilhouette = b);
                dashTrailAllTheTimeOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHTRAILALLTHETIME"), Settings.DashTrailAllTheTime, false)
                    .Change(b => Settings.DashTrailAllTheTime = b);
                madelineHasPonytailOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MADELINEHASPONYTAIL"), Settings.MadelineHasPonytail, false)
                    .Change(b => Settings.MadelineHasPonytail = b);

                madelineBackpackModeOption = (TextMenuExt.Slider) new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MADELINEBACKPACKMODE"),
                    i => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MADELINEBACKPACKMODE_" + Enum.GetNames(typeof(MadelineBackpackMode.MadelineBackpackModes))[i]),
                    0, Enum.GetNames(typeof(MadelineBackpackMode.MadelineBackpackModes)).Length - 1, (int) Settings.MadelineBackpackMode, 0)
                    .Change(i => {
                        Settings.MadelineBackpackMode = (MadelineBackpackMode.MadelineBackpackModes) i;

                        Player p = Engine.Scene.Tracker.GetEntity<Player>();
                        if (p != null) {
                            if (p.Active) {
                                p.ResetSpriteNextFrame(p.DefaultSpriteMode);
                            } else {
                                p.ResetSprite(p.DefaultSpriteMode);
                            }
                        }
                    });

                roomBloomOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_ROOMBLOOM", "", Settings.RoomBloom, 0, new float[] {
                    -1f, 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 2f, 3f, 4f, 5f
                }, f => Settings.RoomBloom = f, f => f == -1f ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT") : $"{f * 100}%");

                glitchEffectOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_GLITCHEFFECT", "", Settings.GlitchEffect, 0, new float[] {
                    -1f, 0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f
                }, f => Settings.GlitchEffect = f, f => f == -1f ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT") : $"{f * 100}%");

                anxietyEffectOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_ANXIETYEFFECT", "", Settings.AnxietyEffect, 0, new float[] {
                    -1f, 0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f
                }, f => Settings.AnxietyEffect = f, f => f == -1f ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT") : $"{f * 100}%");

                blurLevelOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_BLURLEVEL", "", Settings.BlurLevel, 0, new float[] {
                    0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f
                }, f => Settings.BlurLevel = f, f => $"{f * 100}%");

                backgroundBlurLevelOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_BACKGROUNDBLURLEVEL", "", Settings.BackgroundBlurLevel, 0, new float[] {
                    0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f
                }, f => Settings.BackgroundBlurLevel = f, f => $"{f * 100}%");

                zoomLevelOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_ZOOMLEVEL", "x", Settings.ZoomLevel, 10, multiplierScale, f => Settings.ZoomLevel = f);

                // go through all Everest mod assets, and list all color grades that exist.
                List<string> allColorGrades = new List<string>(ColorGrading.ExistingColorGrades);
                foreach (string colorgrade in Everest.Content.Map.Values
                    .Where(asset => asset.Type == typeof(Texture2D) && asset.PathVirtual.StartsWith("Graphics/ColorGrading/"))
                    .Select(asset => asset.PathVirtual.Substring("Graphics/ColorGrading/".Length))) {

                    if (!allColorGrades.Contains(colorgrade)) {
                        allColorGrades.Add(colorgrade);
                    }
                }

                colorGradingOption = (TextMenuExt.Slider) new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_COLORGRADING"),
                    i => {
                        if (i == -1) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");

                        if (i >= ColorGrading.ExistingColorGrades.Count) {
                            // mod color grade - try formatting it somewhat nicely.
                            string colorGradeName = allColorGrades[i];
                            if (colorGradeName.Contains("/")) colorGradeName = colorGradeName.Substring(colorGradeName.LastIndexOf("/") + 1);
                            if (colorGradeName.Length > 15) colorGradeName = colorGradeName.Substring(0, 15) + "...";
                            return "Mod - " + colorGradeName.SpacedPascalCase();
                        }

                        // "none" => read MODOPTIONS_EXTENDEDVARIANTS_CG_none
                        // "celsius/tetris" => read MODOPTIONS_EXTENDEDVARIANTS_CG_tetris
                        string resourceName = ColorGrading.ExistingColorGrades[i];
                        if (resourceName.Contains("/")) resourceName = resourceName.Substring(resourceName.LastIndexOf("/") + 1);
                        return Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_CG_{resourceName}");

                    }, -1, allColorGrades.Count - 1, string.IsNullOrEmpty(Settings.ColorGrading) ? -1 : allColorGrades.IndexOf(Settings.ColorGrading), 0)
                    .Change(i => Settings.ColorGrading = (i == -1 ? "" : allColorGrades[i]));

                screenShakeIntensityOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_SCREENSHAKEINTENSITY", "x", Settings.ScreenShakeIntensity, 10, multiplierScale, f => Settings.ScreenShakeIntensity = f);

                friendlyBadelineFollowerOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FRIENDLYBADELINEFOLLOWER"), Settings.FriendlyBadelineFollower, false)
                    .Change(b => Settings.FriendlyBadelineFollower = b);

                displayDashCountOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISPLAYDASHCOUNT"), Settings.DisplayDashCount, false)
                    .Change(b => Settings.DisplayDashCount = b);


                displaySpeedometerOption = (TextMenuExt.Slider) new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISPLAYSPEEDOMETER"),
                    i => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISPLAYSPEEDOMETER_" + Enum.GetNames(typeof(DisplaySpeedometer.SpeedometerConfiguration))[i]),
                    0, Enum.GetNames(typeof(DisplaySpeedometer.SpeedometerConfiguration)).Length - 1, (int) Settings.DisplaySpeedometer, 0)
                    .Change(i => Settings.DisplaySpeedometer = (DisplaySpeedometer.SpeedometerConfiguration) i);
            }

            // ======
            if (category == VariantCategory.All || category == VariantCategory.GameplayTweaks) {
                // ex-"Other" category
                gameSpeedOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_GAMESPEED", "x", Settings.GameSpeed, 10, multiplierScale, f => Settings.GameSpeed = f);

                noFreezeFramesOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_NOFREEZEFRAMES"), Settings.NoFreezeFrames, false)
                    .Change(b => Settings.NoFreezeFrames = b);

                everythingIsUnderwaterOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_EVERYTHINGISUNDERWATER"), Settings.EverythingIsUnderwater, false)
                    .Change(b => Settings.EverythingIsUnderwater = b);

                staminaOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_STAMINA", "", Settings.Stamina, 11, new int[] {
                    0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250, 260, 270, 280, 290, 300,
                    310, 320, 330, 340, 350, 360, 370, 380, 390, 400, 410, 420, 430, 440, 450, 460, 470, 480, 490, 500
                }, i => Settings.Stamina = i);
                regularHiccupsOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_REGULARHICCUPS", "", Settings.RegularHiccups, 0, multiplierScale, f => {
                    Settings.RegularHiccups = f;
                    (ExtendedVariantsModule.Instance.VariantHandlers[ExtendedVariantsModule.Variant.RegularHiccups] as RegularHiccups).UpdateTimerFromSettings();
                }, f => f == 0f ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLED") : $"{f}s");
                hiccupStrengthOption = getScaleOption("MODOPTIONS_EXTENDEDVARIANTS_HICCUPSTRENGTH", "x", Settings.HiccupStrength, 10, multiplierScale, f => Settings.HiccupStrength = f);

                allStrawberriesAreGoldensOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ALLSTRAWBERRIESAREGOLDENS"), Settings.AllStrawberriesAreGoldens, false)
                    .Change(b => Settings.AllStrawberriesAreGoldens = b);

                // ex-"Troll" category
                forceDuckOnGroundOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FORCEDUCKONGROUND"), Settings.ForceDuckOnGround, false)
                .Change(b => Settings.ForceDuckOnGround = b);
                invertDashesOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INVERTDASHES"), Settings.InvertDashes, false)
                    .Change(b => Settings.InvertDashes = b);
                invertGrabOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INVERTGRAB"), Settings.InvertGrab, false)
                    .Change(b => Settings.InvertGrab = b);
                invertHorizontalControlsOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INVERTHORIZONTALCONTROLS"), Settings.InvertHorizontalControls, false)
                    .Change(b => Settings.InvertHorizontalControls = b);
                invertVerticalControlsOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INVERTVERTICALCONTROLS"), Settings.InvertVerticalControls, false)
                    .Change(b => Settings.InvertVerticalControls = b);
                bounceEverywhereOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BOUNCEEVERYWHERE"), Settings.BounceEverywhere, false)
                    .Change(b => Settings.BounceEverywhere = b);
                alwaysInvisibleOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ALWAYSINVISIBLE"), Settings.AlwaysInvisible, false)
                    .Change(b => Settings.AlwaysInvisible = b);
            }

            if (includeRandomizer) {
                changeVariantsRandomlyOption = (TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHANGEVARIANTSRANDOMLY"), Settings.ChangeVariantsRandomly, false)
                    .Change(b => {
                        Settings.ChangeVariantsRandomly = b;
                        refreshOptionMenuEnabledStatus();
                    });

                randomizerOptions = AbstractSubmenu.BuildOpenMenuButton<OuiRandomizerOptions>(menu, inGame, submenuBackAction, new object[0]);
            }

            TextMenu.SubHeader verticalSpeedTitle = null, jumpingTitle = null, dashingTitle = null, movingTitle = null, chasersTitle = null, bossesTitle = null,
                oshiroTitle = null, theoTitle = null, everywhereTitle = null, otherTitle = null, trollTitle = null, randomizerTitle = null, submenusTitle = null,
                madelineTitle = null, levelTitle = null, holdablesTitle = null;

            if (category == VariantCategory.All || category == VariantCategory.Movement) {
                verticalSpeedTitle = buildHeading(menu, "VERTICALSPEED");
                jumpingTitle = buildHeading(menu, "JUMPING");
                dashingTitle = buildHeading(menu, "DASHING");
                movingTitle = buildHeading(menu, "MOVING");
                holdablesTitle = buildHeading(menu, "HOLDABLES");
            }

            if (category == VariantCategory.All || category == VariantCategory.GameElements) {
                chasersTitle = buildHeading(menu, "CHASERS");
                bossesTitle = buildHeading(menu, "BOSSES");
                oshiroTitle = buildHeading(menu, "OSHIRO");
                theoTitle = buildHeading(menu, "THEO");
                everywhereTitle = buildHeading(menu, "EVERYWHERE");
            }

            if (category == VariantCategory.All || category == VariantCategory.Visual) {
                madelineTitle = buildHeading(menu, "MADELINE");
                levelTitle = buildHeading(menu, "LEVEL");
            }

            if (category == VariantCategory.All || category == VariantCategory.GameplayTweaks) {
                otherTitle = buildHeading(menu, "OTHER");
                trollTitle = buildHeading(menu, "TROLL");
            }

            if (includeRandomizer) randomizerTitle = buildHeading(menu, "RANDOMIZER");

            TextMenuButtonExt movementSubmenu = null, gameElementsSubmenu = null, visualSubmenu = null, gameplayTweaksSubmenu = null;

            if (includeCategorySubmenus) {
                submenusTitle = new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HEADING"));
                movementSubmenu = AbstractSubmenu.BuildOpenMenuButton<OuiCategorySubmenu>(menu, inGame, submenuBackAction, new object[] { VariantCategory.Movement });
                gameElementsSubmenu = AbstractSubmenu.BuildOpenMenuButton<OuiCategorySubmenu>(menu, inGame, submenuBackAction, new object[] { VariantCategory.GameElements });
                visualSubmenu = AbstractSubmenu.BuildOpenMenuButton<OuiCategorySubmenu>(menu, inGame, submenuBackAction, new object[] { VariantCategory.Visual });
                gameplayTweaksSubmenu = AbstractSubmenu.BuildOpenMenuButton<OuiCategorySubmenu>(menu, inGame, submenuBackAction, new object[] { VariantCategory.GameplayTweaks });

                // each submenu entry should be highlighted if one of the options in it has a non-default value.

                movementSubmenu.GetHighlight = () =>
                    new List<Variant> { Variant.Gravity, Variant.FallSpeed, Variant.JumpHeight, Variant.WallBouncingSpeed, Variant.DisableWallJumping, Variant.DisableClimbJumping,
                    Variant.DisableNeutralJumping, Variant.JumpCount, Variant.DashSpeed, Variant.DashLength, Variant.HyperdashSpeed, Variant.DashCount, Variant.HeldDash,
                        Variant.DontRefillDashOnGround, Variant.SpeedX, Variant.SwimmingSpeed, Variant.Friction, Variant.AirFriction, Variant.ExplodeLaunchSpeed, Variant.SuperdashSteeringSpeed,
                        Variant.DisableClimbingUpOrDown, Variant.BoostMultiplier, Variant.DisableRefillsOnScreenTransition, Variant.RestoreDashesOnRespawn, Variant.EveryJumpIsUltra, Variant.CoyoteTime,
                        Variant.PreserveExtraDashesUnderwater, Variant.RefillJumpsOnDashRefill, Variant.LegacyDashSpeedBehavior, Variant.DisableSuperBoosts, Variant.DontRefillStaminaOnGround,
                        Variant.WallSlidingSpeed, Variant.DisableJumpingOutOfWater, Variant.DisableDashCooldown, Variant.CornerCorrection, Variant.PickupDuration, Variant.MinimumDelayBeforeThrowing,
                        Variant.DelayBeforeRegrabbing, Variant.DashTimerMultiplier, Variant.JumpDuration, Variant.HorizontalWallJumpDuration, Variant.HorizontalSpringBounceDuration,
                        Variant.ResetJumpCountOnGround }
                        .Exists(variant => !Instance.VariantHandlers[variant].GetVariantValue().Equals(Instance.VariantHandlers[variant].GetDefaultVariantValue())) || GetDashDirectionIndex() != 0;

                gameElementsSubmenu.GetHighlight = () =>
                    new List<Variant> { Variant.BadelineChasersEverywhere, Variant.BadelineBossesEverywhere, Variant.OshiroEverywhere, Variant.WindEverywhere,
                        Variant.SnowballsEverywhere, Variant.AddSeekers, Variant.TheoCrystalsEverywhere, Variant.JellyfishEverywhere, Variant.RisingLavaEverywhere,
                        Variant.ChaserCount, Variant.AffectExistingChasers, Variant.BadelineLag, Variant.DelayBetweenBadelines, Variant.BadelineAttackPattern,
                        Variant.ChangePatternsOfExistingBosses, Variant.FirstBadelineSpawnRandom, Variant.BadelineBossCount, Variant.BadelineBossNodeCount, Variant.OshiroCount,
                        Variant.ReverseOshiroCount, Variant.DisableOshiroSlowdown, Variant.SnowballDelay, Variant.DisableSeekerSlowdown, Variant.RisingLavaSpeed,
                        Variant.AllowThrowingTheoOffscreen, Variant.AllowLeavingTheoBehind, Variant.JungleSpidersEverywhere }
                        .Exists(variant => Instance.VariantHandlers.ContainsKey(variant) && !Instance.VariantHandlers[variant].GetVariantValue().Equals(Instance.VariantHandlers[variant].GetDefaultVariantValue()));

                visualSubmenu.GetHighlight = () =>
                    new List<Variant> { Variant.UpsideDown, Variant.RoomLighting, Variant.BackgroundBrightness, Variant.ForegroundEffectOpacity, Variant.DisableMadelineSpotlight, Variant.RoomBloom,
                        Variant.GlitchEffect, Variant.AnxietyEffect, Variant.BlurLevel, Variant.ZoomLevel, Variant.ColorGrading, Variant.ScreenShakeIntensity, Variant.MadelineIsSilhouette,
                        Variant.DashTrailAllTheTime, Variant.FriendlyBadelineFollower, Variant.DisplayDashCount, Variant.MadelineHasPonytail, Variant.MadelineBackpackMode, Variant.BackgroundBlurLevel,
                        Variant.DisplaySpeedometer, Variant.DisableKeysSpotlight }
                        .Exists(variant => Instance.VariantHandlers.ContainsKey(variant) && !Instance.VariantHandlers[variant].GetVariantValue().Equals(Instance.VariantHandlers[variant].GetDefaultVariantValue()));

                gameplayTweaksSubmenu.GetHighlight = () =>
                    new List<Variant> { Variant.GameSpeed, Variant.NoFreezeFrames, Variant.EverythingIsUnderwater, Variant.Stamina, Variant.RegularHiccups, Variant.AllStrawberriesAreGoldens,
                        Variant.ForceDuckOnGround, Variant.InvertDashes, Variant.InvertGrab, Variant.InvertHorizontalControls, Variant.InvertVerticalControls, Variant.BounceEverywhere,
                        Variant.AlwaysInvisible, Variant.HiccupStrength }
                        .Exists(variant => !Instance.VariantHandlers[variant].GetVariantValue().Equals(Instance.VariantHandlers[variant].GetDefaultVariantValue()));
            }

            TextMenu.Item openSubmenuButton = null;
            if (includeOpenSubmenuButton) {
                openSubmenuButton = AbstractSubmenu.BuildOpenMenuButton<OuiExtendedVariantsSubmenu>(menu, inGame,
                        () => OuiModOptions.Instance.Overworld.Goto<OuiModOptions>(), new object[] { inGame });
            }

            allOptions = new List<TextMenu.Item>() {
                // all sub-headers
                verticalSpeedTitle, jumpingTitle, dashingTitle, movingTitle, chasersTitle, bossesTitle, oshiroTitle, theoTitle, everywhereTitle, madelineTitle, levelTitle, otherTitle, trollTitle, randomizerTitle, submenusTitle,
                // all submenus
                movementSubmenu, gameElementsSubmenu, visualSubmenu, gameplayTweaksSubmenu,
                // all options excluding the master switch
                optionsOutOfModOptionsMenuOption, submenusForEachCategoryOption, automaticallyResetVariantsOption, openSubmenuButton,
                gravityOption, fallSpeedOption, jumpHeightOption, speedXOption, swimmingSpeedOption, staminaOption, dashSpeedOption, dashCountOption, legacyDashSpeedBehaviorOption,
                heldDashOption, frictionOption, airFrictionOption, disableWallJumpingOption, disableClimbJumpingOption, jumpCountOption, refillJumpsOnDashRefillOption, upsideDownOption, hyperdashSpeedOption,
                wallBouncingSpeedOption, dashLengthOption, forceDuckOnGroundOption, invertDashesOption, invertGrabOption, disableNeutralJumpingOption, changeVariantsRandomlyOption, badelineChasersEverywhereOption,
                chaserCountOption, affectExistingChasersOption, regularHiccupsOption, hiccupStrengthOption, roomLightingOption, roomBloomOption, glitchEffectOption, oshiroEverywhereOption, oshiroCountOption,
                reverseOshiroCountOption, everythingIsUnderwaterOption, disableOshiroSlowdownOption, windEverywhereOption, snowballsEverywhereOption, snowballDelayOption, addSeekersOption,
                disableSeekerSlowdownOption, theoCrystalsEverywhereOption, badelineLagOption, delayBetweenBadelinesOption, allStrawberriesAreGoldensOption, dontRefillDashOnGroundOption, gameSpeedOption,
                colorGradingOption, resetExtendedToDefaultOption, randomizerOptions, badelineBossesEverywhereOption, badelineAttackPatternOption, changePatternOfExistingBossesOption, firstBadelineSpawnRandomOption,
                badelineBossCountOption, badelineBossNodeCountOption, jellyfishEverywhereOption, explodeLaunchSpeedOption, risingLavaEverywhereOption, risingLavaSpeedOption, invertHorizontalControlsOption,
                bounceEverywhereOption, superdashSteeringSpeedOption, screenShakeIntensityOption, anxietyEffectOption, blurLevelOption, zoomLevelOption, dashDirectionOption, backgroundBrightnessOption,
                disableMadelineSpotlightOption, foregroundEffectOpacityOption, madelineIsSilhouetteOption, dashTrailAllTheTimeOption, disableClimbingUpOrDownOption, allowThrowingTheoOffscreenOption,
                allowLeavingTheoBehindOption, boostMultiplierOption, resetVanillaToDefaultOption, friendlyBadelineFollowerOption, dashDirectionsSubMenu, disableRefillsOnScreenTransitionOption,
                restoreDashesOnRespawnOption, disableSuperBoostsOption, displayDashCountOption, madelineHasPonytailOption, madelineBackpackModeOption, invertVerticalControlsOption, dontRefillStaminaOnGroundOption,
                everyJumpIsUltraOption, coyoteTimeOption, backgroundBlurLevelOption, noFreezeFramesOption, preserveExtraDashesUnderwaterOption, alwaysInvisibleOption, displaySpeedometerOption,
                wallSlidingSpeedOption, disableJumpingOutOfWaterOption, disableDashCooldownOption, disableKeysSpotlightOption, jungleSpidersEverywhereOption, cornerCorrectionOption,
                pickupDurationOption, minimumDelayBeforeThrowingOption, delayBeforeRegrabbingOption, dashTimerMultiplierOption, jumpDurationOption, horizontalSpringBounceDurationOption,
                horizontalWallJumpDurationOption, resetJumpCountOnGroundOption };

            refreshOptionMenuEnabledStatus();

            if (includeMasterSwitch) {
                menu.Add(masterSwitchOption);
                if (!inGame) {
                    menu.Add(optionsOutOfModOptionsMenuOption);
                    menu.Add(submenusForEachCategoryOption);
                    menu.Add(automaticallyResetVariantsOption);
                    automaticallyResetVariantsOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AUTOMATICALLYRESETVARIANTS_DESCRIPTION"));
                }
            }

            if (category == VariantCategory.All || includeCategorySubmenus) {
                if (resetVanillaToDefaultOption != null) menu.Add(resetVanillaToDefaultOption);
                menu.Add(resetExtendedToDefaultOption);
            }

            if (includeOpenSubmenuButton) {
                menu.Add(openSubmenuButton);
            }

            if (includeCategorySubmenus) {
                menu.Add(submenusTitle);
                menu.Add(movementSubmenu);
                menu.Add(gameElementsSubmenu);
                menu.Add(visualSubmenu);
                menu.Add(gameplayTweaksSubmenu);
            }

            if (category == VariantCategory.All || category == VariantCategory.Movement) {
                menu.Add(verticalSpeedTitle);
                menu.Add(gravityOption);
                menu.Add(fallSpeedOption);

                menu.Add(jumpingTitle);
                menu.Add(jumpHeightOption);
                menu.Add(jumpDurationOption);
                menu.Add(wallBouncingSpeedOption);
                menu.Add(disableWallJumpingOption);
                menu.Add(disableClimbJumpingOption);
                menu.Add(disableJumpingOutOfWaterOption);
                menu.Add(disableNeutralJumpingOption);
                menu.Add(horizontalWallJumpDurationOption);
                horizontalWallJumpDurationOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HORIZONTALWALLJUMPDURATION_HINT"));
                menu.Add(jumpCountOption);
                menu.Add(refillJumpsOnDashRefillOption);
                menu.Add(resetJumpCountOnGroundOption);
                menu.Add(everyJumpIsUltraOption);
                everyJumpIsUltraOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_EVERYJUMPISULTRA_DESC"));
                menu.Add(coyoteTimeOption);
                coyoteTimeOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_COYOTETIME_DESC"));

                menu.Add(dashingTitle);
                menu.Add(dashSpeedOption);
                menu.Add(legacyDashSpeedBehaviorOption);
                legacyDashSpeedBehaviorOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_LEGACYDASHSPEEDBEHAVIOR_HINT"));
                menu.Add(dashLengthOption);
                menu.Add(dashTimerMultiplierOption);
                dashTimerMultiplierOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHTIMERMULTIPLIER_DESCRIPTION"));
                menu.Add(dashDirectionOption);
                menu.Add(dashDirectionsSubMenu);
                menu.Add(hyperdashSpeedOption);
                menu.Add(superdashSteeringSpeedOption);
                menu.Add(dashCountOption);
                menu.Add(heldDashOption);
                menu.Add(dontRefillDashOnGroundOption);
                menu.Add(disableRefillsOnScreenTransitionOption);
                menu.Add(dontRefillStaminaOnGroundOption);
                menu.Add(restoreDashesOnRespawnOption);
                restoreDashesOnRespawnOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESTOREDASHESONRESPAWN_NOTE"));
                menu.Add(preserveExtraDashesUnderwaterOption);
                menu.Add(disableDashCooldownOption);
                menu.Add(cornerCorrectionOption);

                menu.Add(movingTitle);
                menu.Add(speedXOption);
                menu.Add(swimmingSpeedOption);
                menu.Add(frictionOption);
                menu.Add(airFrictionOption);
                menu.Add(explodeLaunchSpeedOption);
                menu.Add(wallSlidingSpeedOption);
                menu.Add(disableSuperBoostsOption);
                disableSuperBoostsOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLESUPERBOOSTS_NOTE"));
                menu.Add(boostMultiplierOption);
                menu.Add(disableClimbingUpOrDownOption);
                menu.Add(horizontalSpringBounceDurationOption);

                menu.Add(holdablesTitle);
                menu.Add(pickupDurationOption);
                menu.Add(minimumDelayBeforeThrowingOption);
                menu.Add(delayBeforeRegrabbingOption);
            }

            if (category == VariantCategory.All || category == VariantCategory.GameElements) {
                menu.Add(chasersTitle);
                menu.Add(badelineChasersEverywhereOption);
                menu.Add(chaserCountOption);
                menu.Add(affectExistingChasersOption);
                menu.Add(badelineLagOption);
                menu.Add(delayBetweenBadelinesOption);

                menu.Add(bossesTitle);
                menu.Add(badelineBossesEverywhereOption);
                menu.Add(badelineAttackPatternOption);
                menu.Add(changePatternOfExistingBossesOption);
                menu.Add(firstBadelineSpawnRandomOption);
                menu.Add(badelineBossCountOption);
                menu.Add(badelineBossNodeCountOption);

                menu.Add(oshiroTitle);
                menu.Add(oshiroEverywhereOption);
                menu.Add(oshiroCountOption);
                if (Instance.DJMapHelperInstalled) menu.Add(reverseOshiroCountOption);
                menu.Add(disableOshiroSlowdownOption);

                menu.Add(theoTitle);
                menu.Add(theoCrystalsEverywhereOption);
                menu.Add(allowThrowingTheoOffscreenOption);
                allowThrowingTheoOffscreenOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AllowThrowingTheoOffscreen_desc2"));
                allowThrowingTheoOffscreenOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AllowThrowingTheoOffscreen_desc1"));
                menu.Add(allowLeavingTheoBehindOption);

                menu.Add(everywhereTitle);
                menu.Add(windEverywhereOption);
                menu.Add(snowballsEverywhereOption);
                menu.Add(snowballDelayOption);
                menu.Add(addSeekersOption);
                menu.Add(disableSeekerSlowdownOption);
                menu.Add(jellyfishEverywhereOption);
                menu.Add(risingLavaEverywhereOption);
                menu.Add(risingLavaSpeedOption);
                if (Instance.JungleHelperInstalled) menu.Add(jungleSpidersEverywhereOption);
            }

            if (category == VariantCategory.All || category == VariantCategory.Visual) {
                menu.Add(madelineTitle);
                menu.Add(disableMadelineSpotlightOption);
                if (Instance.MaxHelpingHandInstalled || Instance.SpringCollab2020Installed) menu.Add(madelineIsSilhouetteOption);
                menu.Add(dashTrailAllTheTimeOption);
                if (Instance.MaxHelpingHandInstalled) menu.Add(madelineHasPonytailOption);
                menu.Add(displayDashCountOption);
                menu.Add(displaySpeedometerOption);
                menu.Add(madelineBackpackModeOption);

                menu.Add(levelTitle);
                menu.Add(upsideDownOption);
                menu.Add(roomLightingOption);
                menu.Add(backgroundBrightnessOption);
                menu.Add(foregroundEffectOpacityOption);
                menu.Add(roomBloomOption);
                menu.Add(glitchEffectOption);
                menu.Add(anxietyEffectOption);
                menu.Add(blurLevelOption);
                menu.Add(backgroundBlurLevelOption);
                menu.Add(zoomLevelOption);
                menu.Add(colorGradingOption);
                menu.Add(disableKeysSpotlightOption);
                menu.Add(screenShakeIntensityOption);
                menu.Add(friendlyBadelineFollowerOption);
            }

            if (category == VariantCategory.All || category == VariantCategory.GameplayTweaks) {
                menu.Add(otherTitle);
                menu.Add(gameSpeedOption);
                menu.Add(noFreezeFramesOption);
                noFreezeFramesOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_NOFREEZEFRAMES_DESC"));
                menu.Add(everythingIsUnderwaterOption);
                menu.Add(staminaOption);
                menu.Add(regularHiccupsOption);
                menu.Add(hiccupStrengthOption);
                menu.Add(allStrawberriesAreGoldensOption);
                menu.Add(alwaysInvisibleOption);

                menu.Add(trollTitle);
                menu.Add(forceDuckOnGroundOption);
                menu.Add(invertDashesOption);
                menu.Add(invertGrabOption);
                menu.Add(invertHorizontalControlsOption);
                menu.Add(invertVerticalControlsOption);
                menu.Add(bounceEverywhereOption);
            }

            if (includeRandomizer) {
                menu.Add(randomizerTitle);
                menu.Add(changeVariantsRandomlyOption);
                menu.Add(randomizerOptions);
            }
        }

        private TextMenu.SubHeader buildHeading(TextMenu menu, string headingNameResource) {
            return new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HEADING") + " - " + Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HEADING_" + headingNameResource));
        }

        private void resetAllOptionsToDefault() {
            gravityOption?.ResetToDefault();
            fallSpeedOption?.ResetToDefault();
            jumpHeightOption?.ResetToDefault();
            jumpDurationOption?.ResetToDefault();
            speedXOption?.ResetToDefault();
            swimmingSpeedOption?.ResetToDefault();
            boostMultiplierOption?.ResetToDefault();
            staminaOption?.ResetToDefault();
            dashSpeedOption?.ResetToDefault();
            legacyDashSpeedBehaviorOption?.ResetToDefault();
            dashCountOption?.ResetToDefault();
            cornerCorrectionOption?.ResetToDefault();
            frictionOption?.ResetToDefault();
            airFrictionOption?.ResetToDefault();
            disableWallJumpingOption?.ResetToDefault();
            disableClimbJumpingOption?.ResetToDefault();
            disableJumpingOutOfWaterOption?.ResetToDefault();
            jumpCountOption?.ResetToDefault();
            refillJumpsOnDashRefillOption?.ResetToDefault();
            resetJumpCountOnGroundOption?.ResetToDefault();
            upsideDownOption?.ResetToDefault();
            hyperdashSpeedOption?.ResetToDefault();
            explodeLaunchSpeedOption?.ResetToDefault();
            horizontalSpringBounceDurationOption?.ResetToDefault();
            horizontalWallJumpDurationOption?.ResetToDefault();
            disableSuperBoostsOption?.ResetToDefault();
            dontRefillStaminaOnGroundOption?.ResetToDefault();
            wallBouncingSpeedOption?.ResetToDefault();
            wallSlidingSpeedOption?.ResetToDefault();
            dashLengthOption?.ResetToDefault();
            dashTimerMultiplierOption?.ResetToDefault();
            dashDirectionOption?.ResetToDefault();
            forceDuckOnGroundOption?.ResetToDefault();
            invertDashesOption?.ResetToDefault();
            invertGrabOption?.ResetToDefault();
            invertHorizontalControlsOption?.ResetToDefault();
            invertVerticalControlsOption?.ResetToDefault();
            heldDashOption?.ResetToDefault();
            disableDashCooldownOption?.ResetToDefault();
            disableNeutralJumpingOption?.ResetToDefault();
            badelineChasersEverywhereOption?.ResetToDefault();
            chaserCountOption?.ResetToDefault();
            affectExistingChasersOption?.ResetToDefault();
            badelineBossesEverywhereOption?.ResetToDefault();
            badelineAttackPatternOption?.ResetToDefault();
            changePatternOfExistingBossesOption?.ResetToDefault();
            firstBadelineSpawnRandomOption?.ResetToDefault();
            badelineBossCountOption?.ResetToDefault();
            badelineBossNodeCountOption?.ResetToDefault();
            regularHiccupsOption?.ResetToDefault();
            hiccupStrengthOption?.ResetToDefault();
            roomLightingOption?.ResetToDefault();
            backgroundBrightnessOption?.ResetToDefault();
            foregroundEffectOpacityOption?.ResetToDefault();
            disableMadelineSpotlightOption?.ResetToDefault();
            disableKeysSpotlightOption?.ResetToDefault();
            roomBloomOption?.ResetToDefault();
            glitchEffectOption?.ResetToDefault();
            anxietyEffectOption?.ResetToDefault();
            blurLevelOption?.ResetToDefault();
            backgroundBlurLevelOption?.ResetToDefault();
            zoomLevelOption?.ResetToDefault();
            everythingIsUnderwaterOption?.ResetToDefault();
            oshiroEverywhereOption?.ResetToDefault();
            oshiroCountOption?.ResetToDefault();
            reverseOshiroCountOption?.ResetToDefault();
            disableOshiroSlowdownOption?.ResetToDefault();
            windEverywhereOption?.ResetToDefault();
            snowballsEverywhereOption?.ResetToDefault();
            snowballDelayOption?.ResetToDefault();
            addSeekersOption?.ResetToDefault();
            disableSeekerSlowdownOption?.ResetToDefault();
            theoCrystalsEverywhereOption?.ResetToDefault();
            allowThrowingTheoOffscreenOption?.ResetToDefault();
            allowLeavingTheoBehindOption?.ResetToDefault();
            risingLavaEverywhereOption?.ResetToDefault();
            risingLavaSpeedOption?.ResetToDefault();
            badelineLagOption?.ResetToDefault();
            delayBetweenBadelinesOption?.ResetToDefault();
            allStrawberriesAreGoldensOption?.ResetToDefault();
            dontRefillDashOnGroundOption?.ResetToDefault();
            disableRefillsOnScreenTransitionOption?.ResetToDefault();
            restoreDashesOnRespawnOption?.ResetToDefault();
            gameSpeedOption?.ResetToDefault();
            colorGradingOption?.ResetToDefault();
            jellyfishEverywhereOption?.ResetToDefault();
            bounceEverywhereOption?.ResetToDefault();
            jungleSpidersEverywhereOption?.ResetToDefault();
            superdashSteeringSpeedOption?.ResetToDefault();
            screenShakeIntensityOption?.ResetToDefault();
            madelineIsSilhouetteOption?.ResetToDefault();
            madelineHasPonytailOption?.ResetToDefault();
            dashTrailAllTheTimeOption?.ResetToDefault();
            madelineBackpackModeOption?.ResetToDefault();
            displaySpeedometerOption?.ResetToDefault();
            disableClimbingUpOrDownOption?.ResetToDefault();
            pickupDurationOption?.ResetToDefault();
            minimumDelayBeforeThrowingOption?.ResetToDefault();
            delayBeforeRegrabbingOption?.ResetToDefault();
            friendlyBadelineFollowerOption?.ResetToDefault();
            displayDashCountOption?.ResetToDefault();
            everyJumpIsUltraOption?.ResetToDefault();
            coyoteTimeOption?.ResetToDefault();
            noFreezeFramesOption?.ResetToDefault();
            preserveExtraDashesUnderwaterOption?.ResetToDefault();
            alwaysInvisibleOption?.ResetToDefault();
        }

        private void refreshOptionMenuEnabledStatus() {
            // hide everything if the master switch is off, show everything if it is on.
            foreach (TextMenu.Item item in allOptions) {
                if (item != null) item.Visible = Settings.MasterSwitch;
            }

            // special graying-out rules for some variant options
            if (oshiroCountOption != null) oshiroCountOption.Disabled = !Settings.OshiroEverywhere;
            if (reverseOshiroCountOption != null) reverseOshiroCountOption.Disabled = !Settings.OshiroEverywhere;
            if (randomizerOptions != null) randomizerOptions.Disabled = !Settings.ChangeVariantsRandomly;
            if (firstBadelineSpawnRandomOption != null) firstBadelineSpawnRandomOption.Disabled = !Settings.BadelineBossesEverywhere;
            if (badelineBossCountOption != null) badelineBossCountOption.Disabled = !Settings.BadelineBossesEverywhere;
            if (badelineBossNodeCountOption != null) badelineBossNodeCountOption.Disabled = !Settings.BadelineBossesEverywhere;
            if (allowThrowingTheoOffscreenOption != null) allowThrowingTheoOffscreenOption.Disabled = !Settings.TheoCrystalsEverywhere;
            if (allowLeavingTheoBehindOption != null) allowLeavingTheoBehindOption.Disabled = !Settings.TheoCrystalsEverywhere;
            if (dashDirectionsSubMenu != null) dashDirectionsSubMenu.Visible = GetDashDirectionIndex() == 3;
        }

        // gets the index of the currently selected Dash Direction index, from 0 to 3
        public static int GetDashDirectionIndex() {
            bool[][] settings = ExtendedVariantsModule.Settings.AllowedDashDirections;

            // first option: everything allowed
            if (
                settings[0][0] && settings[0][1] && settings[0][2] &&
                settings[1][0] && settings[1][1] && settings[1][2] &&
                settings[2][0] && settings[2][1] && settings[2][2]
            ) {
                return 0;
            }

            // second option: no diagonals
            if (
                !settings[0][0] && settings[0][1] && !settings[0][2] &&
                settings[1][0] && settings[1][1] && settings[1][2] &&
                !settings[2][0] && settings[2][1] && !settings[2][2]
            ) {
                return 1;
            }

            // third option: diagonals only
            if (
                settings[0][0] && !settings[0][1] && settings[0][2] &&
                !settings[1][0] && settings[1][1] && !settings[1][2] &&
                settings[2][0] && !settings[2][1] && settings[2][2]
            ) {
                return 2;
            }

            // fourth option: custom
            return 3;
        }

        private void reloadModOptions() {
            // transition to a "submenu" that will kick us back directly to Mod Options.
            // that will make Everest reload Mod Options while saving the position on the menu.
            OuiModOptions.Instance.Overworld.Goto<OuiModOptionsReloaderHelper>();
        }

        public class OuiModOptionsReloaderHelper : Oui, OuiModOptions.ISubmenu {
            public override IEnumerator Enter(Oui from) {
                Overworld.Goto<OuiModOptions>();
                yield break;
            }

            public override IEnumerator Leave(Oui next) {
                yield break;
            }
        }
    }
}
