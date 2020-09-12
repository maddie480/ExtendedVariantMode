using Celeste;
using Celeste.Mod;
using Celeste.Mod.UI;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.UI {
    /// <summary>
    /// This class is kind of a mess, but this is where all the options for Extended Variants are defined.
    /// This is called to build every menu or submenu containing extended variants options (parts to display or hide
    /// are managed by the various menus / submenus depending on where you are and your display preferences).
    /// </summary>
    class ModOptionsEntries {
        private ExtendedVariantsSettings Settings => ExtendedVariantsModule.Settings;

        private TextMenu.Option<bool> masterSwitchOption;
        private TextMenu.Option<bool> optionsOutOfModOptionsMenuOption;
        private TextMenu.Option<bool> submenusForEachCategoryOption;
        private TextMenu.Option<bool> automaticallyResetVariantsOption;
        private TextMenu.Option<int> gravityOption;
        private TextMenu.Option<int> fallSpeedOption;
        private TextMenu.Option<int> jumpHeightOption;
        private TextMenu.Option<int> speedXOption;
        private TextMenu.Option<int> staminaOption;
        private TextMenu.Option<int> dashSpeedOption;
        private TextMenu.Option<bool> legacyDashSpeedBehaviorOption;
        private TextMenu.Option<int> dashCountOption;
        private TextMenu.Option<bool> heldDashOption;
        private TextMenu.Option<int> frictionOption;
        private TextMenu.Option<int> airFrictionOption;
        private TextMenu.Option<bool> disableWallJumpingOption;
        private TextMenu.Option<bool> disableClimbJumpingOption;
        private TextMenu.Option<int> jumpCountOption;
        private TextMenu.Option<bool> refillJumpsOnDashRefillOption;
        private TextMenu.Option<bool> upsideDownOption;
        private TextMenu.Option<int> hyperdashSpeedOption;
        private TextMenu.Option<int> explodeLaunchSpeedOption;
        private TextMenu.Option<int> wallBouncingSpeedOption;
        private TextMenu.Option<int> dashLengthOption;
        private TextMenu.Option<int> dashDirectionOption;
        private TextMenu.Option<bool> forceDuckOnGroundOption;
        private TextMenu.Option<bool> invertDashesOption;
        private TextMenu.Option<bool> invertGrabOption;
        private TextMenu.Option<bool> invertHorizontalControlsOption;
        private TextMenu.Option<bool> disableNeutralJumpingOption;
        private TextMenu.Option<bool> changeVariantsRandomlyOption;
        private TextMenu.Option<bool> badelineChasersEverywhereOption;
        private TextMenu.Option<int> chaserCountOption;
        private TextMenu.Option<bool> affectExistingChasersOption;
        private TextMenu.Option<bool> badelineBossesEverywhereOption;
        private TextMenu.Option<int> badelineAttackPatternOption;
        private TextMenu.Option<bool> changePatternOfExistingBossesOption;
        private TextMenu.Option<int> firstBadelineSpawnRandomOption;
        private TextMenu.Option<int> badelineBossCountOption;
        private TextMenu.Option<int> badelineBossNodeCountOption;
        private TextMenu.Option<int> regularHiccupsOption;
        private TextMenu.Option<int> hiccupStrengthOption;
        private TextMenu.Option<int> roomLightingOption;
        private TextMenu.Option<int> roomBloomOption;
        private TextMenu.Option<int> glitchEffectOption;
        private TextMenu.Option<int> anxietyEffectOption;
        private TextMenu.Option<int> blurLevelOption;
        private TextMenu.Option<int> zoomLevelOption;
        private TextMenu.Option<bool> everythingIsUnderwaterOption;
        private TextMenu.Option<bool> oshiroEverywhereOption;
        private TextMenu.Option<int> oshiroCountOption;
        private TextMenu.Option<int> reverseOshiroCountOption;
        private TextMenu.Option<bool> disableOshiroSlowdownOption;
        private TextMenu.Option<int> windEverywhereOption;
        private TextMenu.Option<bool> snowballsEverywhereOption;
        private TextMenu.Option<int> snowballDelayOption;
        private TextMenu.Option<int> addSeekersOption;
        private TextMenu.Option<bool> disableSeekerSlowdownOption;
        private TextMenu.Option<bool> theoCrystalsEverywhereOption;
        private TextMenu.Option<int> badelineLagOption;
        private TextMenu.Option<int> delayBetweenBadelinesOption;
        private TextMenu.Option<bool> allStrawberriesAreGoldensOption;
        private TextMenu.Option<bool> dontRefillDashOnGroundOption;
        private TextMenu.Option<int> gameSpeedOption;
        private TextMenu.Option<int> colorGradingOption;
        private TextMenu.Option<int> jellyfishEverywhereOption;
        private TextMenu.Option<bool> risingLavaEverywhereOption;
        private TextMenu.Option<int> risingLavaSpeedOption;
        private TextMenu.Option<bool> bounceEverywhereOption;
        private TextMenu.Option<int> superdashSteeringSpeedOption;
        private TextMenu.Option<int> screenShakeIntensityOption;
        private TextMenu.Option<int> backgroundBrightnessOption;
        private TextMenu.Option<bool> disableMadelineSpotlightOption;
        private TextMenu.Option<int> foregroundEffectOpacityOption;
        private TextMenu.Option<bool> madelineIsSilhouetteOption;
        private TextMenu.Item resetToDefaultOption;
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
                // Add a button to easily revert to default values
                resetToDefaultOption = new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESETTODEFAULT")).Pressed(() => {
                    ExtendedVariantsModule.Instance.ResetToDefaultSettings();
                    refreshOptionMenuValues();
                    refreshOptionMenuEnabledStatus();
                });
            }

            // ======
            if (category == VariantCategory.All || category == VariantCategory.Movement) {
                // Vertical Speed
                gravityOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_GRAVITY"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.Gravity), indexFromMultiplier(10)).Change(i => Settings.Gravity = multiplierScale[i]);
                fallSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FALLSPEED"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.FallSpeed), indexFromMultiplier(10)).Change(i => Settings.FallSpeed = multiplierScale[i]);

                // Jumping
                jumpHeightOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_JUMPHEIGHT"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.JumpHeight), indexFromMultiplier(10)).Change(i => Settings.JumpHeight = multiplierScale[i]);
                wallBouncingSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_WALLBOUNCINGSPEED"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.WallBouncingSpeed), indexFromMultiplier(10)).Change(i => Settings.WallBouncingSpeed = multiplierScale[i]);
                disableWallJumpingOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEWALLJUMPING"), Settings.DisableWallJumping, false)
                    .Change(b => Settings.DisableWallJumping = b);
                disableClimbJumpingOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLECLIMBJUMPING"), Settings.DisableClimbJumping, false)
                    .Change(b => Settings.DisableClimbJumping = b);
                disableNeutralJumpingOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLENEUTRALJUMPING"), Settings.DisableNeutralJumping, false)
                    .Change(b => Settings.DisableNeutralJumping = b);
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

                // Dashing
                dashSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHSPEED"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.DashSpeed), indexFromMultiplier(10)).Change(i => Settings.DashSpeed = multiplierScale[i]);
                legacyDashSpeedBehaviorOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_LEGACYDASHSPEEDBEHAVIOR"), Settings.LegacyDashSpeedBehavior, false)
                    .Change(value => {
                        Settings.LegacyDashSpeedBehavior = value;

                        // hot swap the "dash speed" variant handler
                        Instance.VariantHandlers[Variant.DashSpeed].Unload();
                        Instance.VariantHandlers[Variant.DashSpeed] = Settings.LegacyDashSpeedBehavior ? (AbstractExtendedVariant) new DashSpeedOld() : new DashSpeed();
                        Instance.VariantHandlers[Variant.DashSpeed].Load();
                    });
                dashLengthOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHLENGTH"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.DashLength), indexFromMultiplier(10)).Change(i => Settings.DashLength = multiplierScale[i]);
                dashDirectionOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION"),
                    i => Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION_{i}"), 0, 2, Settings.DashDirection, 0).Change(i => Settings.DashDirection = i);
                hyperdashSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HYPERDASHSPEED"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.HyperdashSpeed), indexFromMultiplier(10)).Change(i => Settings.HyperdashSpeed = multiplierScale[i]);
                dashCountOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHCOUNT"), i => {
                    if (i == -1) {
                        return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                    }
                    return i.ToString();
                }, -1, 10, Settings.DashCount, 0).Change(i => Settings.DashCount = i);
                heldDashOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HELDDASH"), Settings.HeldDash, false)
                    .Change(b => Settings.HeldDash = b);
                dontRefillDashOnGroundOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DONTREFILLDASHONGROUND"), Settings.DontRefillDashOnGround, false)
                    .Change(b => Settings.DontRefillDashOnGround = b);
                superdashSteeringSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SUPERDASHSTEERINGSPEED"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.SuperdashSteeringSpeed), indexFromMultiplier(10)).Change(i => Settings.SuperdashSteeringSpeed = multiplierScale[i]);

                // Moving
                speedXOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SPEEDX"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.SpeedX), indexFromMultiplier(10)).Change(i => Settings.SpeedX = multiplierScale[i]);
                explodeLaunchSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_EXPLODELAUNCHSPEED"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.ExplodeLaunchSpeed), indexFromMultiplier(10)).Change(i => Settings.ExplodeLaunchSpeed = multiplierScale[i]);
                frictionOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FRICTION"),
                    i => {
                        switch (i) {
                            case -1: return "0x";
                            case 0: return "0.05x";
                            default: return multiplierFormatter(i);
                        }
                    }, -1, multiplierScale.Length - 1, Settings.Friction == -1 ? -1 : indexFromMultiplier(Settings.Friction), indexFromMultiplier(10) + 1)
                    .Change(i => Settings.Friction = (i == -1 ? -1 : multiplierScale[i]));
                airFrictionOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AIRFRICTION"),
                    i => {
                        switch (i) {
                            case -1: return "0x";
                            case 0: return "0.05x";
                            default: return multiplierFormatter(i);
                        }
                    }, -1, multiplierScale.Length - 1, Settings.AirFriction == -1 ? -1 : indexFromMultiplier(Settings.AirFriction), indexFromMultiplier(10) + 1)
                    .Change(i => Settings.AirFriction = (i == -1 ? -1 : multiplierScale[i]));
            }

            // ======
            if (category == VariantCategory.All || category == VariantCategory.GameElements) {
                // Badeline Chasers
                badelineChasersEverywhereOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINECHASERSEVERYWHERE"), Settings.BadelineChasersEverywhere, false)
                    .Change(b => Settings.BadelineChasersEverywhere = b);
                chaserCountOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHASERCOUNT"),
                    i => i.ToString(), 1, 10, Settings.ChaserCount, 0).Change(i => Settings.ChaserCount = i);
                affectExistingChasersOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AFFECTEXISTINGCHASERS"), Settings.AffectExistingChasers, false)
                    .Change(b => Settings.AffectExistingChasers = b);
                badelineLagOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINELAG"),
                    i => i == 0 ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT") : multiplierFormatter(i).Replace("x", "s"),
                    0, multiplierScale.Length - 1, indexFromMultiplier(Settings.BadelineLag), 0).Change(i => Settings.BadelineLag = multiplierScale[i]);
                delayBetweenBadelinesOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DELAYBETWEENBADELINES"),
                    i => multiplierFormatter(i).Replace("x", "s"), 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.DelayBetweenBadelines), 4)
                    .Change(i => Settings.DelayBetweenBadelines = multiplierScale[i]);

                // Badeline Bosses
                badelineBossesEverywhereOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINEBOSSESEVERYWHERE"), Settings.BadelineBossesEverywhere, false)
                    .Change(b => {
                        Settings.BadelineBossesEverywhere = b;
                        refreshOptionMenuEnabledStatus();
                    });
                badelineAttackPatternOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINEATTACKPATTERN"),
                    i => {
                        if (i == 0) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_WINDEVERYWHERE_RANDOM");
                        return Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_BADELINEPATTERN_{badelineBossesPatternsOptions[i]}");
                    }, 0, badelineBossesPatternsOptions.Length - 1, indexFromPatternValue(Settings.BadelineAttackPattern), 0)
                    .Change(i => Settings.BadelineAttackPattern = badelineBossesPatternsOptions[i]);
                changePatternOfExistingBossesOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHANGEPATTERNSOFEXISTINGBOSSES"), Settings.ChangePatternsOfExistingBosses, false)
                    .Change(b => Settings.ChangePatternsOfExistingBosses = b);
                firstBadelineSpawnRandomOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FIRSTBADELINESPAWNRANDOM"),
                    i => i == 1 ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FIRSTBADELINESPAWNRANDOM_ON") : Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FIRSTBADELINESPAWNRANDOM_OFF"),
                    0, 1, Settings.FirstBadelineSpawnRandom ? 1 : 0, 0).Change(i => Settings.FirstBadelineSpawnRandom = (i != 0));
                badelineBossCountOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINEBOSSCOUNT"),
                    i => i.ToString(), 1, 5, Settings.BadelineBossCount, 0).Change(i => Settings.BadelineBossCount = i);
                badelineBossNodeCountOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BADELINEBOSSNODECOUNT"),
                    i => i.ToString(), 1, 5, Settings.BadelineBossNodeCount, 0).Change(i => Settings.BadelineBossNodeCount = i);

                // Oshiro
                oshiroEverywhereOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_OSHIROEVERYWHERE"), Settings.OshiroEverywhere, false)
                    .Change(b => {
                        Settings.OshiroEverywhere = b;
                        refreshOptionMenuEnabledStatus();
                    });
                oshiroCountOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_OSHIROCOUNT"),
                    i => i.ToString(), 0, 5, Settings.OshiroCount, 1).Change(i => Settings.OshiroCount = i);
                reverseOshiroCountOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_REVERSEOSHIROCOUNT"),
                    i => i.ToString(), 0, 5, Settings.ReverseOshiroCount, 0).Change(i => Settings.ReverseOshiroCount = i);
                disableOshiroSlowdownOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEOSHIROSLOWDOWN"), Settings.DisableOshiroSlowdown, false)
                    .Change(b => Settings.DisableOshiroSlowdown = b);

                // Other elements
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
                disableSeekerSlowdownOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLESEEKERSLOWDOWN"), Settings.DisableSeekerSlowdown, false)
                    .Change(b => {
                        Settings.DisableSeekerSlowdown = b;
                        if (b && Engine.Scene is Level level && level.Tracker.CountEntities<Seeker>() != 0) {
                            // since we are in a map with seekers and we are killing slowdown, set speed to 1 to be sure we aren't making the current slowdown permanent. :maddyS:
                            Engine.TimeRate = 1f;
                        }
                    });
                theoCrystalsEverywhereOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_THEOCRYSTALSEVERYWHERE"), Settings.TheoCrystalsEverywhere, false)
                    .Change(b => Settings.TheoCrystalsEverywhere = b);
                jellyfishEverywhereOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_JELLYFISHEVERYWHERE"), i => i.ToString(), 0, 3, Settings.JellyfishEverywhere, 0)
                    .Change(i => Settings.JellyfishEverywhere = i);
                risingLavaEverywhereOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RISINGLAVAEVERYWHERE"), Settings.RisingLavaEverywhere, false)
                    .Change(b => Settings.RisingLavaEverywhere = b);
                risingLavaSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RISINGLAVASPEED"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.RisingLavaSpeed), indexFromMultiplier(10)).Change(i => Settings.RisingLavaSpeed = multiplierScale[i]);
            }

            // ======
            if (category == VariantCategory.All || category == VariantCategory.Visual) {
                upsideDownOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_UPSIDEDOWN"), Settings.UpsideDown, false)
                    .Change(b => Settings.UpsideDown = b);

                roomLightingOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ROOMLIGHTING"),
                    i => {
                        if (i == -1) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                        return $"{i * 10}%";
                    }, -1, 10, Settings.RoomLighting, 0).Change(i => {
                        Settings.RoomLighting = i;
                        if (Engine.Scene.GetType() == typeof(Level)) {
                            // currently in level, change lighting right away
                            Level lvl = (Engine.Scene as Level);
                            lvl.Lighting.Alpha = (i == -1 ? lvl.BaseLightingAlpha + lvl.Session.LightingAlphaAdd : 1 - (i / 10f));
                        }
                    });

                backgroundBrightnessOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BACKGROUNDBRIGHTNESS"),
                    i => $"{i * 10}%", 0, 10, Settings.BackgroundBrightness, 10).Change(i => Settings.BackgroundBrightness = i);

                foregroundEffectOpacityOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FOREGROUNDEFFECTOPACITY"),
                    i => $"{i * 10}%", 0, 10, Settings.ForegroundEffectOpacity, 10).Change(i => Settings.ForegroundEffectOpacity = i);

                disableMadelineSpotlightOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEMADELINESPOTLIGHT"), Settings.DisableMadelineSpotlight, false)
                    .Change(b => Settings.DisableMadelineSpotlight = b);
                madelineIsSilhouetteOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MADELINEISSILHOUETTE"), Settings.MadelineIsSilhouette, false)
                    .Change(b => Settings.MadelineIsSilhouette = b);

                roomBloomOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ROOMBLOOM"),
                    i => {
                        if (i == -1) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                        if (i > 10) return $"{(i - 9) * 100}%";
                        return $"{i * 10}%";
                    }, -1, 14, Settings.RoomBloom, 0).Change(i => Settings.RoomBloom = i);

                glitchEffectOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_GLITCHEFFECT"),
                    i => {
                        if (i == -1) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                        return $"{i * 5}%";
                    }, -1, 20, Settings.GlitchEffect, 0).Change(i => Settings.GlitchEffect = i);

                anxietyEffectOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ANXIETYEFFECT"),
                    i => {
                        if (i == -1) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                        return $"{i * 5}%";
                    }, -1, 20, Settings.AnxietyEffect, 0).Change(i => Settings.AnxietyEffect = i);

                blurLevelOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BLURLEVEL"),
                    i => $"{i * 10}%", 0, 20, Settings.BlurLevel, 0).Change(i => Settings.BlurLevel = i);

                zoomLevelOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ZOOMLEVEL"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.ZoomLevel), indexFromMultiplier(10)).Change(i => Settings.ZoomLevel = multiplierScale[i]);

                colorGradingOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_COLORGRADING"),
                    i => {
                        if (i == -1) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");

                        // "none" => read MODOPTIONS_EXTENDEDVARIANTS_CG_none
                        // "celsius/tetris" => read MODOPTIONS_EXTENDEDVARIANTS_CG_tetris
                        string resourceName = ColorGrading.ExistingColorGrades[i];
                        if (resourceName.Contains("/")) resourceName = resourceName.Substring(resourceName.LastIndexOf("/") + 1);
                        return Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_CG_{resourceName}");

                    }, -1, ColorGrading.ExistingColorGrades.Count - 1, Settings.ColorGrading, 0)
                    .Change(i => Settings.ColorGrading = i);

                screenShakeIntensityOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SCREENSHAKEINTENSITY"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.ScreenShakeIntensity), indexFromMultiplier(10)).Change(i => Settings.ScreenShakeIntensity = multiplierScale[i]);
            }

            // ======
            if (category == VariantCategory.All || category == VariantCategory.GameplayTweaks) {
                // ex-"Other" category
                gameSpeedOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_GAMESPEED"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.GameSpeed), indexFromMultiplier(10)).Change(i => Settings.GameSpeed = multiplierScale[i]);

                everythingIsUnderwaterOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_EVERYTHINGISUNDERWATER"), Settings.EverythingIsUnderwater, false)
                    .Change(b => Settings.EverythingIsUnderwater = b);

                staminaOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_STAMINA"),
                    i => $"{i * 10}", 0, 50, Settings.Stamina, 11).Change(i => Settings.Stamina = i);
                regularHiccupsOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_REGULARHICCUPS"),
                    i => i == 0 ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLED") : multiplierFormatter(i).Replace("x", "s"),
                    0, multiplierScale.Length - 1, indexFromMultiplier(Settings.RegularHiccups), 0).Change(i => {
                        Settings.RegularHiccups = multiplierScale[i];
                        (ExtendedVariantsModule.Instance.VariantHandlers[ExtendedVariantsModule.Variant.RegularHiccups] as RegularHiccups).UpdateTimerFromSettings();
                    });

                hiccupStrengthOption = new TextMenuExt.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HICCUPSTRENGTH"),
                    multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.HiccupStrength), indexFromMultiplier(10)).Change(i => Settings.HiccupStrength = multiplierScale[i]);

                allStrawberriesAreGoldensOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ALLSTRAWBERRIESAREGOLDENS"), Settings.AllStrawberriesAreGoldens, false)
                    .Change(b => Settings.AllStrawberriesAreGoldens = b);

                // ex-"Troll" category
                forceDuckOnGroundOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FORCEDUCKONGROUND"), Settings.ForceDuckOnGround, false)
                .Change(b => Settings.ForceDuckOnGround = b);
                invertDashesOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INVERTDASHES"), Settings.InvertDashes, false)
                    .Change(b => Settings.InvertDashes = b);
                invertGrabOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INVERTGRAB"), Settings.InvertGrab, false)
                    .Change(b => Settings.InvertGrab = b);
                invertHorizontalControlsOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INVERTHORIZONTALCONTROLS"), Settings.InvertHorizontalControls, false)
                    .Change(b => Settings.InvertHorizontalControls = b);
                bounceEverywhereOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_BOUNCEEVERYWHERE"), Settings.BounceEverywhere, false)
                    .Change(b => Settings.BounceEverywhere = b);
            }

            if (includeRandomizer) {
                changeVariantsRandomlyOption = new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHANGEVARIANTSRANDOMLY"), Settings.ChangeVariantsRandomly, false)
                    .Change(b => {
                        Settings.ChangeVariantsRandomly = b;
                        refreshOptionMenuEnabledStatus();
                    });

                randomizerOptions = AbstractSubmenu.BuildOpenMenuButton<OuiRandomizerOptions>(menu, inGame, submenuBackAction, new object[0]);
            }

            TextMenu.SubHeader verticalSpeedTitle = null, jumpingTitle = null, dashingTitle = null, movingTitle = null, chasersTitle = null, bossesTitle = null,
                oshiroTitle = null, everywhereTitle = null, visualTitle = null, otherTitle = null, trollTitle = null, randomizerTitle = null, submenusTitle = null;

            if (category == VariantCategory.All || category == VariantCategory.Movement) {
                verticalSpeedTitle = buildHeading(menu, "VERTICALSPEED");
                jumpingTitle = buildHeading(menu, "JUMPING");
                dashingTitle = buildHeading(menu, "DASHING");
                movingTitle = buildHeading(menu, "MOVING");
            }

            if (category == VariantCategory.All || category == VariantCategory.GameElements) {
                chasersTitle = buildHeading(menu, "CHASERS");
                bossesTitle = buildHeading(menu, "BOSSES");
                oshiroTitle = buildHeading(menu, "OSHIRO");
                everywhereTitle = buildHeading(menu, "EVERYWHERE");
            }

            if (category == VariantCategory.All || category == VariantCategory.Visual) {
                visualTitle = buildHeading(menu, "VISUAL");
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
                    Variant.DisableNeutralJumping, Variant.JumpCount, Variant.DashSpeed, Variant.DashLength, Variant.DashDirection, Variant.HyperdashSpeed, Variant.DashCount, Variant.HeldDash,
                        Variant.DontRefillDashOnGround, Variant.SpeedX, Variant.Friction, Variant.AirFriction, Variant.ExplodeLaunchSpeed, Variant.SuperdashSteeringSpeed }
                        .Exists(variant => Instance.VariantHandlers[variant].GetValue() != Instance.VariantHandlers[variant].GetDefaultValue())
                        || Settings.RefillJumpsOnDashRefill || Settings.LegacyDashSpeedBehavior;

                gameElementsSubmenu.GetHighlight = () =>
                    new List<Variant> { Variant.BadelineChasersEverywhere, Variant.BadelineBossesEverywhere, Variant.OshiroEverywhere, Variant.WindEverywhere,
                        Variant.SnowballsEverywhere, Variant.AddSeekers, Variant.TheoCrystalsEverywhere, Variant.JellyfishEverywhere, Variant.RisingLavaEverywhere }
                        .Exists(variant => Instance.VariantHandlers[variant].GetValue() != Instance.VariantHandlers[variant].GetDefaultValue())
                        || Settings.ChaserCount != 1 || Settings.AffectExistingChasers || Settings.BadelineLag != 0 || Settings.DelayBetweenBadelines != 4
                        || Settings.BadelineAttackPattern != 0 || Settings.ChangePatternsOfExistingBosses || Settings.FirstBadelineSpawnRandom || Settings.BadelineBossCount != 1
                        || Settings.BadelineBossNodeCount != 1 || Settings.OshiroCount != 1 || Settings.ReverseOshiroCount != 0 || Settings.DisableOshiroSlowdown || Settings.SnowballDelay != 8
                        || Settings.DisableSeekerSlowdown || Settings.RisingLavaSpeed != 10;

                visualSubmenu.GetHighlight = () =>
                    new List<Variant> { Variant.UpsideDown, Variant.RoomLighting, Variant.BackgroundBrightness, Variant.ForegroundEffectOpacity, Variant.DisableMadelineSpotlight, Variant.RoomBloom,
                        Variant.GlitchEffect, Variant.AnxietyEffect, Variant.BlurLevel, Variant.ZoomLevel, Variant.ColorGrading, Variant.ScreenShakeIntensity, Variant.MadelineIsSilhouette }
                        .Exists(variant => Instance.VariantHandlers[variant].GetValue() != Instance.VariantHandlers[variant].GetDefaultValue());

                gameplayTweaksSubmenu.GetHighlight = () =>
                    new List<Variant> { Variant.GameSpeed, Variant.EverythingIsUnderwater, Variant.Stamina, Variant.RegularHiccups, Variant.AllStrawberriesAreGoldens,
                        Variant.ForceDuckOnGround, Variant.InvertDashes, Variant.InvertGrab, Variant.InvertHorizontalControls, Variant.BounceEverywhere }
                        .Exists(variant => Instance.VariantHandlers[variant].GetValue() != Instance.VariantHandlers[variant].GetDefaultValue())
                        || Settings.HiccupStrength != 10;
            }

            TextMenu.Item openSubmenuButton = null;
            if (includeOpenSubmenuButton) {
                openSubmenuButton = AbstractSubmenu.BuildOpenMenuButton<OuiExtendedVariantsSubmenu>(menu, inGame,
                        () => OuiModOptions.Instance.Overworld.Goto<OuiModOptions>(), new object[] { inGame });
            }

            allOptions = new List<TextMenu.Item>() {
                // all sub-headers
                verticalSpeedTitle, jumpingTitle, dashingTitle, movingTitle, chasersTitle, bossesTitle, oshiroTitle, everywhereTitle, visualTitle, otherTitle, trollTitle, randomizerTitle, submenusTitle,
                // all submenus
                movementSubmenu, gameElementsSubmenu, visualSubmenu, gameplayTweaksSubmenu,
                // all options excluding the master switch
                optionsOutOfModOptionsMenuOption, submenusForEachCategoryOption, automaticallyResetVariantsOption, openSubmenuButton,
                gravityOption, fallSpeedOption, jumpHeightOption, speedXOption, staminaOption, dashSpeedOption, dashCountOption, legacyDashSpeedBehaviorOption,
                heldDashOption, frictionOption, airFrictionOption, disableWallJumpingOption, disableClimbJumpingOption, jumpCountOption, refillJumpsOnDashRefillOption, upsideDownOption, hyperdashSpeedOption,
                wallBouncingSpeedOption, dashLengthOption, forceDuckOnGroundOption, invertDashesOption, invertGrabOption, disableNeutralJumpingOption, changeVariantsRandomlyOption, badelineChasersEverywhereOption,
                chaserCountOption, affectExistingChasersOption, regularHiccupsOption, hiccupStrengthOption, roomLightingOption, roomBloomOption, glitchEffectOption, oshiroEverywhereOption, oshiroCountOption,
                reverseOshiroCountOption, everythingIsUnderwaterOption, disableOshiroSlowdownOption, windEverywhereOption, snowballsEverywhereOption, snowballDelayOption, addSeekersOption,
                disableSeekerSlowdownOption, theoCrystalsEverywhereOption, badelineLagOption, delayBetweenBadelinesOption, allStrawberriesAreGoldensOption, dontRefillDashOnGroundOption, gameSpeedOption,
                colorGradingOption, resetToDefaultOption, randomizerOptions, badelineBossesEverywhereOption, badelineAttackPatternOption, changePatternOfExistingBossesOption, firstBadelineSpawnRandomOption,
                badelineBossCountOption, badelineBossNodeCountOption, jellyfishEverywhereOption, explodeLaunchSpeedOption, risingLavaEverywhereOption, risingLavaSpeedOption, invertHorizontalControlsOption,
                bounceEverywhereOption, superdashSteeringSpeedOption, screenShakeIntensityOption, anxietyEffectOption, blurLevelOption, zoomLevelOption, dashDirectionOption, backgroundBrightnessOption,
                disableMadelineSpotlightOption, foregroundEffectOpacityOption, madelineIsSilhouetteOption};

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
                menu.Add(resetToDefaultOption);
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
                menu.Add(wallBouncingSpeedOption);
                menu.Add(disableWallJumpingOption);
                menu.Add(disableClimbJumpingOption);
                menu.Add(disableNeutralJumpingOption);
                menu.Add(jumpCountOption);
                menu.Add(refillJumpsOnDashRefillOption);

                menu.Add(dashingTitle);
                menu.Add(dashSpeedOption);
                menu.Add(legacyDashSpeedBehaviorOption);
                legacyDashSpeedBehaviorOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_LEGACYDASHSPEEDBEHAVIOR_HINT"));
                menu.Add(dashLengthOption);
                menu.Add(dashDirectionOption);
                menu.Add(hyperdashSpeedOption);
                menu.Add(superdashSteeringSpeedOption);
                menu.Add(dashCountOption);
                menu.Add(heldDashOption);
                menu.Add(dontRefillDashOnGroundOption);

                menu.Add(movingTitle);
                menu.Add(speedXOption);
                menu.Add(frictionOption);
                menu.Add(airFrictionOption);
                menu.Add(explodeLaunchSpeedOption);
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

                menu.Add(everywhereTitle);
                menu.Add(windEverywhereOption);
                menu.Add(snowballsEverywhereOption);
                menu.Add(snowballDelayOption);
                menu.Add(addSeekersOption);
                menu.Add(disableSeekerSlowdownOption);
                menu.Add(theoCrystalsEverywhereOption);
                menu.Add(jellyfishEverywhereOption);
                menu.Add(risingLavaEverywhereOption);
                menu.Add(risingLavaSpeedOption);
            }

            if (category == VariantCategory.All || category == VariantCategory.Visual) {
                menu.Add(visualTitle);
                menu.Add(upsideDownOption);
                menu.Add(roomLightingOption);
                menu.Add(backgroundBrightnessOption);
                menu.Add(foregroundEffectOpacityOption);
                menu.Add(disableMadelineSpotlightOption);
                menu.Add(roomBloomOption);
                menu.Add(glitchEffectOption);
                menu.Add(anxietyEffectOption);
                menu.Add(blurLevelOption);
                menu.Add(zoomLevelOption);
                menu.Add(colorGradingOption);
                menu.Add(screenShakeIntensityOption);
                if (Instance.MaxHelpingHandInstalled || Instance.SpringCollab2020Installed) menu.Add(madelineIsSilhouetteOption);
            }

            if (category == VariantCategory.All || category == VariantCategory.GameplayTweaks) {
                menu.Add(otherTitle);
                menu.Add(gameSpeedOption);
                menu.Add(everythingIsUnderwaterOption);
                menu.Add(staminaOption);
                menu.Add(regularHiccupsOption);
                menu.Add(hiccupStrengthOption);
                menu.Add(allStrawberriesAreGoldensOption);

                menu.Add(trollTitle);
                menu.Add(forceDuckOnGroundOption);
                menu.Add(invertDashesOption);
                menu.Add(invertGrabOption);
                menu.Add(invertHorizontalControlsOption);
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

        private void refreshOptionMenuValues() {
            setValue(gravityOption, 0, indexFromMultiplier(Settings.Gravity));
            setValue(fallSpeedOption, 0, indexFromMultiplier(Settings.FallSpeed));
            setValue(jumpHeightOption, 0, indexFromMultiplier(Settings.JumpHeight));
            setValue(speedXOption, 0, indexFromMultiplier(Settings.SpeedX));
            setValue(staminaOption, 0, Settings.Stamina);
            setValue(dashSpeedOption, 0, indexFromMultiplier(Settings.DashSpeed));
            setValue(legacyDashSpeedBehaviorOption, Settings.LegacyDashSpeedBehavior);
            setValue(dashCountOption, -1, Settings.DashCount);
            setValue(frictionOption, -1, Settings.Friction == -1 ? -1 : indexFromMultiplier(Settings.Friction));
            setValue(airFrictionOption, -1, Settings.AirFriction == -1 ? -1 : indexFromMultiplier(Settings.AirFriction));
            setValue(disableWallJumpingOption, Settings.DisableWallJumping);
            setValue(disableClimbJumpingOption, Settings.DisableClimbJumping);
            setValue(jumpCountOption, 0, Settings.JumpCount);
            setValue(refillJumpsOnDashRefillOption, Settings.RefillJumpsOnDashRefill);
            setValue(upsideDownOption, Settings.UpsideDown);
            setValue(hyperdashSpeedOption, 0, indexFromMultiplier(Settings.HyperdashSpeed));
            setValue(explodeLaunchSpeedOption, 0, indexFromMultiplier(Settings.ExplodeLaunchSpeed));
            setValue(wallBouncingSpeedOption, 0, indexFromMultiplier(Settings.WallBouncingSpeed));
            setValue(dashLengthOption, 0, indexFromMultiplier(Settings.DashLength));
            setValue(dashDirectionOption, 0, Settings.DashDirection);
            setValue(forceDuckOnGroundOption, Settings.ForceDuckOnGround);
            setValue(invertDashesOption, Settings.InvertDashes);
            setValue(invertGrabOption, Settings.InvertGrab);
            setValue(invertHorizontalControlsOption, Settings.InvertHorizontalControls);
            setValue(heldDashOption, Settings.HeldDash);
            setValue(disableNeutralJumpingOption, Settings.DisableNeutralJumping);
            setValue(badelineChasersEverywhereOption, Settings.BadelineChasersEverywhere);
            setValue(chaserCountOption, 1, Settings.ChaserCount);
            setValue(affectExistingChasersOption, Settings.AffectExistingChasers);
            setValue(badelineBossesEverywhereOption, Settings.BadelineBossesEverywhere);
            setValue(badelineAttackPatternOption, 0, Settings.BadelineAttackPattern);
            setValue(changePatternOfExistingBossesOption, Settings.ChangePatternsOfExistingBosses);
            setValue(firstBadelineSpawnRandomOption, 0, Settings.FirstBadelineSpawnRandom ? 1 : 0);
            setValue(badelineBossCountOption, 1, Settings.BadelineBossCount);
            setValue(badelineBossNodeCountOption, 1, Settings.BadelineBossNodeCount);
            setValue(changeVariantsRandomlyOption, Settings.ChangeVariantsRandomly);
            setValue(regularHiccupsOption, 0, indexFromMultiplier(Settings.RegularHiccups));
            setValue(hiccupStrengthOption, 0, indexFromMultiplier(Settings.HiccupStrength));
            setValue(roomLightingOption, -1, Settings.RoomLighting);
            setValue(backgroundBrightnessOption, 0, Settings.BackgroundBrightness);
            setValue(foregroundEffectOpacityOption, 0, Settings.ForegroundEffectOpacity);
            setValue(disableMadelineSpotlightOption, Settings.DisableMadelineSpotlight);
            setValue(roomBloomOption, -1, Settings.RoomBloom);
            setValue(glitchEffectOption, -1, Settings.GlitchEffect);
            setValue(anxietyEffectOption, -1, Settings.AnxietyEffect);
            setValue(blurLevelOption, 0, Settings.BlurLevel);
            setValue(zoomLevelOption, 0, indexFromMultiplier(Settings.ZoomLevel));
            setValue(everythingIsUnderwaterOption, Settings.EverythingIsUnderwater);
            setValue(oshiroEverywhereOption, Settings.OshiroEverywhere);
            setValue(oshiroCountOption, 0, Settings.OshiroCount);
            setValue(reverseOshiroCountOption, 0, Settings.ReverseOshiroCount);
            setValue(disableOshiroSlowdownOption, Settings.DisableOshiroSlowdown);
            setValue(windEverywhereOption, 0, Settings.WindEverywhere);
            setValue(snowballsEverywhereOption, Settings.SnowballsEverywhere);
            setValue(snowballDelayOption, 0, Settings.SnowballDelay);
            setValue(addSeekersOption, 0, Settings.AddSeekers);
            setValue(disableSeekerSlowdownOption, Settings.DisableSeekerSlowdown);
            setValue(theoCrystalsEverywhereOption, Settings.TheoCrystalsEverywhere);
            setValue(risingLavaEverywhereOption, Settings.RisingLavaEverywhere);
            setValue(risingLavaSpeedOption, 0, indexFromMultiplier(Settings.RisingLavaSpeed));
            setValue(badelineLagOption, 0, Settings.BadelineLag);
            setValue(delayBetweenBadelinesOption, 0, Settings.DelayBetweenBadelines);
            setValue(allStrawberriesAreGoldensOption, Settings.AllStrawberriesAreGoldens);
            setValue(dontRefillDashOnGroundOption, Settings.DontRefillDashOnGround);
            setValue(gameSpeedOption, 0, indexFromMultiplier(Settings.GameSpeed));
            setValue(colorGradingOption, -1, Settings.ColorGrading);
            setValue(jellyfishEverywhereOption, 0, Settings.JellyfishEverywhere);
            setValue(bounceEverywhereOption, Settings.BounceEverywhere);
            setValue(superdashSteeringSpeedOption, 0, indexFromMultiplier(Settings.SuperdashSteeringSpeed));
            setValue(screenShakeIntensityOption, 0, Settings.ScreenShakeIntensity);
            setValue(madelineIsSilhouetteOption, Settings.MadelineIsSilhouette);
        }

        private void refreshOptionMenuEnabledStatus() {
            // hide everything if the master switch is off, show everything if it is on.
            foreach (TextMenu.Item item in allOptions) {
                if (item != null) item.Visible = Settings.MasterSwitch;
            }

            // special graying-out rules for some variant options
            if (refillJumpsOnDashRefillOption != null) refillJumpsOnDashRefillOption.Disabled = Settings.JumpCount < 2;
            if (oshiroCountOption != null) oshiroCountOption.Disabled = !Settings.OshiroEverywhere;
            if (reverseOshiroCountOption != null) reverseOshiroCountOption.Disabled = !Settings.OshiroEverywhere;
            if (randomizerOptions != null) randomizerOptions.Disabled = !Settings.ChangeVariantsRandomly;
            if (firstBadelineSpawnRandomOption != null) firstBadelineSpawnRandomOption.Disabled = !Settings.BadelineBossesEverywhere;
            if (badelineBossCountOption != null) badelineBossCountOption.Disabled = !Settings.BadelineBossesEverywhere;
            if (badelineBossNodeCountOption != null) badelineBossNodeCountOption.Disabled = !Settings.BadelineBossesEverywhere;
        }

        private void setValue(TextMenu.Option<int> option, int min, int newValue) {
            if (option == null) return;

            newValue -= min;

            if (newValue != option.Index) {
                // replicate the vanilla behaviour
                option.PreviousIndex = option.Index;
                option.Index = newValue;
                option.ValueWiggler.Start();
            }
        }

        private void setValue(TextMenu.Option<bool> option, bool newValue) {
            if (option == null) return;

            if (newValue != (option.Index == 1)) {
                // replicate the vanilla behaviour
                option.PreviousIndex = option.Index;
                option.Index = newValue ? 1 : 0;
                option.ValueWiggler.Start();
            }
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
