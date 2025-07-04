using Celeste;
using Celeste.Mod;
using Celeste.Mod.UI;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
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

        private TextMenuExt.OnOff correctedMirrorModeOption;
        private TextMenuOptionExt<int> oshiroCountOption;
        private TextMenuOptionExt<int> reverseOshiroCountOption;
        private TextMenu.Item randomizerOptions;
        private TextMenuOptionExt<int> firstBadelineSpawnRandomOption;
        private TextMenuOptionExt<int> badelineBossCountOption;
        private TextMenuOptionExt<int> badelineBossNodeCountOption;
        private TextMenuOptionExt<bool> allowThrowingTheoOffscreenOption;
        private TextMenuOptionExt<bool> allowLeavingTheoBehindOption;
        private TextMenuOptionExt<bool> noFreezeFramesAdvanceCassetteBlocksOption;
        private TextMenuOptionExt<int> dashDirection;
        private Celeste.TextMenuExt.SubMenu dashDirectionsSubMenu;
        private List<TextMenu.Item> elementsToHideOnToggle;

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

        private static float[] waterSpeed80 = new float[] {
            0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75,
            80,
            85, 90, 95, 100, 105, 110, 115, 120, 125, 130, 135, 140, 145, 150, 155, 160,
            170, 180, 190, 200, 210, 220, 230, 240,
            260, 280, 300, 320, 340, 360, 380, 400,
            440, 480, 520, 560, 600, 640, 680, 720, 760, 800
        };

        private static float[] waterSpeed60 = new float[] {
            0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55,
            60,
            65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 115, 120,
            130, 140, 150, 160, 170, 180,
            200, 220, 240, 260, 280, 300,
            340, 380, 420, 460, 500, 540, 580, 600
        };

        private TextMenuOptionExt<int> getScaleOption<T>(Variant variant, string suffix, T[] scale, Func<T, string> formatter = null) where T : IComparable {
            return getNonVariantScaleOption<T>(variant.ToString(), suffix, scale,
                currentValue: (T) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(variant),
                mapDefinedValue: (T) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(variant),
                defaultValue: (T) ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue(),
                valueSetter: newValue => setVariantValue(variant, newValue),
                formatter);
        }

        private TextMenuOptionExt<int> getNonVariantScaleOption<T>(string label, string suffix, T[] scale,
            T currentValue, T mapDefinedValue, T defaultValue, Action<T> valueSetter, Func<T, string> formatter = null) where T : IComparable {

            List<T> choices = new List<T>(scale);

            // we run valueToIndex a first time in order to insert values in the choices if necessary.
            // the second run will then be able to get the definitive indices of the values without risking being thrown off by a later insert.
            valueToIndex(currentValue, choices);
            valueToIndex(mapDefinedValue, choices);
            valueToIndex(defaultValue, choices);

            TextMenuExt.Slider slider = new TextMenuExt.Slider(Dialog.Clean($"modoptions_extendedvariants_{label}"),
                i => {
                    T valueToFormat = choices[i];
                    if (formatter != null) {
                        return formatter(valueToFormat);
                    }

                    if (valueToFormat is float f) {
                        string formatted = $"{f:n3}";
                        // trim trailing zeroes
                        while (formatted.EndsWith("0")) formatted = formatted.Substring(0, formatted.Length - 1);
                        // if we trimmed all zeroes, trim the dot as well
                        if (formatted.EndsWith(".")) formatted = formatted.Substring(0, formatted.Length - 1);
                        return formatted + suffix;
                    }

                    return valueToFormat + suffix;
                },
                0, choices.Count - 1, valueToIndex(currentValue, choices), valueToIndex(defaultValue, choices), valueToIndex(mapDefinedValue, choices));

            slider.Change(i => valueSetter(choices[i]));

            return slider;
        }

        private int valueToIndex<T>(T value, List<T> choices) where T : IComparable {
            // if the value is on the scale, simply return its index.
            if (choices.Contains(value)) {
                return choices.IndexOf(value);
            }

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
            }

            return position;
        }

        private TextMenuExt.OnOff getToggleOption(Variant variant) {
            return (TextMenuExt.OnOff) new TextMenuExt.OnOff(
                    Dialog.Clean($"modoptions_extendedvariants_{variant}"),
                    (bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(variant),
                    (bool) ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue(),
                    (bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(variant))
                .Change(b => setVariantValue(variant, b));
        }

        public static void SetVariantValue(Variant variantChange, object newValue) {
            bool mapDefaultValue = false;

            if (Engine.Scene is Level) {
                if (ExtendedVariantTriggerManager.AreValuesIdentical(newValue, ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(variantChange))) {
                    Logger.Log("ExtendedVariantsModule/ModOptionsEntries", $"Variant value {variantChange} = {newValue} was equal to the map-defined value, so it was removed from the overrides and from the settings.");
                    ExtendedVariantsModule.Session.VariantsOverridenByUser.Remove(variantChange);
                    ExtendedVariantsModule.Settings.EnabledVariants.Remove(variantChange);
                    mapDefaultValue = true;
                } else {
                    Logger.Log("ExtendedVariantsModule/ModOptionsEntries", $"Variant value {variantChange} = {newValue} was added to the overrides.");
                    ExtendedVariantsModule.Session.VariantsOverridenByUser.Add(variantChange);
                }
            }

            if (!mapDefaultValue) {
                if (ExtendedVariantTriggerManager.AreValuesIdentical(newValue, ExtendedVariantTriggerManager.GetDefaultValueForVariant(variantChange))) {
                    Logger.Log("ExtendedVariantsModule/ModOptionsEntries", $"Variant value {variantChange} = {newValue} was equal to the default value, so it was removed from the settings.");
                    ExtendedVariantsModule.Settings.EnabledVariants.Remove(variantChange);
                } else {
                    Logger.Log("ExtendedVariantsModule/ModOptionsEntries", $"Variant value {variantChange} = {newValue} was set.");
                    ExtendedVariantsModule.Settings.EnabledVariants[variantChange] = newValue;
                }
            }

            ExtendedVariantsModule.Instance.VariantHandlers[variantChange].VariantValueChanged();
            ExtendedVariantsModule.Instance.Randomizer.RefreshEnabledVariantsDisplayList();
        }

        private void setVariantValue(Variant variantChange, object newValue) {
            SetVariantValue(variantChange, newValue);
            refreshOptionMenuEnabledStatus();
        }

        private T[] getEnumValues<T>() where T : Enum {
            Array enumValues = Enum.GetValues(typeof(T));
            T[] result = new T[enumValues.Length];
            int i = 0;
            foreach (T enumValue in enumValues) {
                result[i++] = enumValue;
            }
            return result;
        }

        private Color getColorForVariantSubmenu(List<Variant> variants) {
            bool hasMapDefinedVariants = false;

            foreach (Variant variant in variants) {
                if (!ExtendedVariantsModule.Instance.VariantHandlers.ContainsKey(variant)) {
                    continue;
                }

                if (!ExtendedVariantTriggerManager.AreValuesIdentical(
                    ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(variant),
                    ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(variant))) {

                    return Color.Goldenrod;
                }
                if (!ExtendedVariantTriggerManager.AreValuesIdentical(
                    ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue(),
                    ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(variant))) {

                    hasMapDefinedVariants = true;
                }
            }

            return hasMapDefinedVariants ? Color.DeepSkyBlue : Color.White;
        }

        public enum VariantCategory {
            Movement, GameElements, Visual, GameplayTweaks, QualityOfLife, None
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
        public void CreateAllOptions(VariantCategory category, bool includeMasterSwitch, bool includeCategorySubmenus, bool includeRandomizer,
            Action submenuBackAction, TextMenu menu, bool inGame, bool forceEnabled) {

            if (includeMasterSwitch) {
                // create the "master switch" option with specific enable/disable handling.
                menu.Add(new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MASTERSWITCH"), Settings.MasterSwitch) {
                        Disabled = forceEnabled // if variants are force-enabled, you can't disable them, so you have to disable the master switch.
                    }
                    .Change(v => {
                        Settings.MasterSwitch = v;
                        refreshOptionMenuEnabledStatus();

                        // (de)activate all hooks!
                        if (v) ExtendedVariantsModule.Instance.HookStuff();
                        else ExtendedVariantsModule.Instance.UnhookStuff();
                    }));
                ExtendedVariantsModule.Instance.CreateKeyBindingsMenu(menu, inGame, null);
            }

            // ======

            if (includeCategorySubmenus) {
                TextMenu.Button resetExtendedVariants, resetVanillaVariants = null;

                // Add buttons to easily revert to default values (vanilla and extended variants)
                if (inGame) {
                    menu.Add((resetVanillaVariants = new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESETTODEFAULT_VANILLA"))).Pressed(() => {
                        ExtendedVariantsModule.Instance.ResetVanillaVariantsToDefaultSettings();
                    }));
                }

                menu.Add((resetExtendedVariants = new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESETTODEFAULT_EXTENDED"))).Pressed(() => {
                    ExtendedVariantsModule.Instance.ResetExtendedVariantsToDefaultSettings();
                }));

                TextMenuButtonExt movementSubmenu, gameElementsSubmenu, visualSubmenu, gameplayTweaksSubmenu, qualityOfLifeSubmenu;
                TextMenu.SubHeader title;

                menu.Add(title = new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HEADING")));
                menu.Add(movementSubmenu = AbstractSubmenu.BuildOpenMenuButton<OuiCategorySubmenu>(menu, inGame, submenuBackAction, new object[] { VariantCategory.Movement }));
                menu.Add(gameElementsSubmenu = AbstractSubmenu.BuildOpenMenuButton<OuiCategorySubmenu>(menu, inGame, submenuBackAction, new object[] { VariantCategory.GameElements }));
                menu.Add(visualSubmenu = AbstractSubmenu.BuildOpenMenuButton<OuiCategorySubmenu>(menu, inGame, submenuBackAction, new object[] { VariantCategory.Visual }));
                menu.Add(gameplayTweaksSubmenu = AbstractSubmenu.BuildOpenMenuButton<OuiCategorySubmenu>(menu, inGame, submenuBackAction, new object[] { VariantCategory.GameplayTweaks }));
                menu.Add(qualityOfLifeSubmenu = AbstractSubmenu.BuildOpenMenuButton<OuiCategorySubmenu>(menu, inGame, submenuBackAction, new object[] { VariantCategory.QualityOfLife }));

                // each submenu entry should be highlighted if one of the options in it has a non-default value.

                movementSubmenu.GetHighlightColor = () => getColorForVariantSubmenu(new List<Variant> {
                    Variant.Gravity, Variant.FallSpeed, Variant.JumpHeight, Variant.WallBouncingSpeed, Variant.DisableWallJumping, Variant.DisableClimbJumping,
                    Variant.DisableNeutralJumping, Variant.JumpCount, Variant.DashSpeed, Variant.DashLength, Variant.HyperdashSpeed, Variant.DashCount, Variant.HeldDash,
                    Variant.DontRefillDashOnGround, Variant.DashRestriction, Variant.SpeedX, Variant.UnderwaterSpeedX, Variant.UnderwaterSpeedY,
                    Variant.WaterSurfaceSpeedX, Variant.WaterSurfaceSpeedY, Variant.Friction, Variant.AirFriction, Variant.ExplodeLaunchSpeed,
                    Variant.SuperdashSteeringSpeed, Variant.DisableClimbingUpOrDown, Variant.BoostMultiplier, Variant.LiftboostCapX, Variant.LiftboostCapUp, Variant.LiftboostCapDown, Variant.DisableRefillsOnScreenTransition, Variant.RestoreDashesOnRespawn,
                    Variant.EveryJumpIsUltra, Variant.CoyoteTime, Variant.PreserveExtraDashesUnderwater, Variant.RefillJumpsOnDashRefill, Variant.LegacyDashSpeedBehavior,
                    Variant.DisableSuperBoosts, Variant.DontRefillStaminaOnGround, Variant.WallSlidingSpeed, Variant.DisableJumpingOutOfWater, Variant.DisableDashCooldown,
                    Variant.CornerCorrection, Variant.PickupDuration, Variant.MinimumDelayBeforeThrowing, Variant.DelayBeforeRegrabbing, Variant.DashTimerMultiplier,
                    Variant.JumpDuration, Variant.HorizontalWallJumpDuration, Variant.HorizontalSpringBounceDuration, Variant.ResetJumpCountOnGround, Variant.UltraSpeedMultiplier,
                    Variant.DashDirection, Variant.JumpCooldown, Variant.WallJumpDistance, Variant.WallBounceDistance, Variant.FastFallAcceleration, Variant.TrueNoGrabbing,
                    Variant.WalllessWallbounce, Variant.MidairTech, Variant.PreserveWallbounceSpeed, Variant.StretchUpDashes, Variant.DisableJumpGravityLowering, Variant.DisableAutoJumpGravityLowering,
                    Variant.SlowfallGravityMultiplier, Variant.SlowfallSpeedThreshold
                });

                gameElementsSubmenu.GetHighlightColor = () => getColorForVariantSubmenu(new List<Variant> {
                    Variant.BadelineChasersEverywhere, Variant.BadelineBossesEverywhere, Variant.OshiroEverywhere, Variant.WindEverywhere,
                    Variant.SnowballsEverywhere, Variant.AddSeekers, Variant.TheoCrystalsEverywhere, Variant.JellyfishEverywhere, Variant.RisingLavaEverywhere,
                    Variant.ChaserCount, Variant.AffectExistingChasers, Variant.BadelineLag, Variant.DelayBetweenBadelines, Variant.BadelineAttackPattern,
                    Variant.ChangePatternsOfExistingBosses, Variant.FirstBadelineSpawnRandom, Variant.BadelineBossCount, Variant.BadelineBossNodeCount, Variant.OshiroCount,
                    Variant.ReverseOshiroCount, Variant.DisableOshiroSlowdown, Variant.SnowballDelay, Variant.DisableSeekerSlowdown, Variant.RisingLavaSpeed,
                    Variant.AllowThrowingTheoOffscreen, Variant.AllowLeavingTheoBehind, Variant.JungleSpidersEverywhere
                });

                visualSubmenu.GetHighlightColor = () => getColorForVariantSubmenu(new List<Variant> {
                    Variant.UpsideDown, Variant.RoomLighting, Variant.BackgroundBrightness, Variant.ForegroundEffectOpacity, Variant.DisableMadelineSpotlight, Variant.RoomBloom,
                    Variant.GlitchEffect, Variant.AnxietyEffect, Variant.BlurLevel, Variant.ZoomLevel, Variant.ColorGrading, Variant.ScreenShakeIntensity, Variant.MadelineIsSilhouette,
                    Variant.DashTrailAllTheTime, Variant.FriendlyBadelineFollower, Variant.DisplayDashCount, Variant.MadelineHasPonytail, Variant.MadelineBackpackMode, Variant.BackgroundBlurLevel,
                    Variant.DisplaySpeedometer, Variant.DisableKeysSpotlight, Variant.SpinnerColor
                });

                gameplayTweaksSubmenu.GetHighlightColor = () => getColorForVariantSubmenu(new List<Variant> {
                    Variant.GameSpeed, Variant.NoFreezeFrames, Variant.EverythingIsUnderwater, Variant.AlwaysFeather, Variant.Stamina, Variant.RegularHiccups, Variant.AllStrawberriesAreGoldens,
                    Variant.ForceDuckOnGround, Variant.InvertDashes, Variant.InvertGrab, Variant.InvertHorizontalControls, Variant.InvertVerticalControls, Variant.BounceEverywhere,
                    Variant.AlwaysInvisible, Variant.HiccupStrength, Variant.CorrectedMirrorMode, Variant.PermanentDashAttack, Variant.PermanentBinoStorage, Variant.NoFreezeFramesAdvanceCassetteBlocks, Variant.AutoJump, Variant.AutoDash
                });

                qualityOfLifeSubmenu.GetHighlightColor = () => getColorForVariantSubmenu(new List<Variant> {
                    Variant.UltraProtection, Variant.LiftboostProtection, Variant.CornerboostProtection, Variant.CrouchDashFix, Variant.AlternativeBuffering, Variant.MultiBuffering,
                    Variant.SaferDiagonalSmuggle, Variant.DashBeforePickup, Variant.ThrowIgnoresForcedMove
                });

                elementsToHideOnToggle = new List<TextMenu.Item>() { resetVanillaVariants, resetExtendedVariants, title, movementSubmenu, gameElementsSubmenu, visualSubmenu, gameplayTweaksSubmenu, qualityOfLifeSubmenu };
            } else {
                elementsToHideOnToggle = new List<TextMenu.Item>();
            }

            if (category != VariantCategory.None) {
                menu.Add(new Celeste.TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_COLOR_ORANGE")) { TextColor = Color.Goldenrod });
                menu.Add(new Celeste.TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_COLOR_BLUE")) { TextColor = Color.DeepSkyBlue, HeightExtra = 0f });
            }

            // ======

            if (category == VariantCategory.Movement) {
                menu.Add(buildHeading(menu, "VERTICALSPEED"));
                menu.Add(getScaleOption(Variant.Gravity, "x", multiplierScale));
                menu.Add(getScaleOption(Variant.FallSpeed, "x", multiplierScale));

                menu.Add(buildHeading(menu, "JUMPING"));
                menu.Add(getScaleOption(Variant.JumpHeight, "x", multiplierScale));
                menu.Add(getScaleOption(Variant.JumpDuration, "x", multiplierScale));
                menu.Add(getScaleOption(Variant.WallBouncingSpeed, "x", multiplierScale));
                menu.Add(getToggleOption(Variant.DisableWallJumping));
                menu.Add(getToggleOption(Variant.DisableClimbJumping));
                menu.Add(getToggleOption(Variant.DisableJumpingOutOfWater));
                menu.Add(getToggleOption(Variant.DisableNeutralJumping));
                menu.Add(getScaleOption(Variant.WallJumpDistance, "px", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }));
                menu.Add(getScaleOption(Variant.WallBounceDistance, "px", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }));

                TextMenuExt.OnOff walllessWallbounceOption;
                menu.Add(walllessWallbounceOption = getToggleOption(Variant.WalllessWallbounce));
                walllessWallbounceOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_WALLLESSWALLBOUNCE_HINT"));

                TextMenuExt.OnOff preserveWallbounceSpeedToggle;
                menu.Add(preserveWallbounceSpeedToggle = getToggleOption(Variant.PreserveWallbounceSpeed));
                preserveWallbounceSpeedToggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_PRESERVEWALLBOUNCESPEED_HINT"));

                TextMenuOptionExt<int> horizontalWallJumpDurationOption;
                menu.Add(horizontalWallJumpDurationOption = getScaleOption(Variant.HorizontalWallJumpDuration, "x", multiplierScale));
                horizontalWallJumpDurationOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HORIZONTALWALLJUMPDURATION_HINT"));

                menu.Add(getScaleOption(Variant.JumpCount, "", new int[] { 0, 1, 2, 3, 4, 5, int.MaxValue }, i => {
                    if (i == int.MaxValue) {
                        return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INFINITE");
                    }
                    return i.ToString();
                }));

                menu.Add(getToggleOption(Variant.RefillJumpsOnDashRefill));
                menu.Add(getToggleOption(Variant.ResetJumpCountOnGround));
                menu.Add(getScaleOption(Variant.JumpCooldown, "s", multiplierScale));

                TextMenuExt.OnOff everyJumpIsUltraOption;
                menu.Add(everyJumpIsUltraOption = getToggleOption(Variant.EveryJumpIsUltra));
                everyJumpIsUltraOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_EVERYJUMPISULTRA_DESC"));

                TextMenuOptionExt<int> coyoteTimeOption;
                menu.Add(coyoteTimeOption = getScaleOption(Variant.CoyoteTime, "x", multiplierScale));
                coyoteTimeOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_COYOTETIME_DESC"));

                TextMenuExt.OnOff disableJumpGravityLoweringOption;
                menu.Add(disableJumpGravityLoweringOption = getToggleOption(Variant.DisableJumpGravityLowering));
                disableJumpGravityLoweringOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEJUMPGRAVITYLOWERING_HINT"));

                TextMenuExt.OnOff disableAutoJumpGravityLoweringOption;
                menu.Add(disableAutoJumpGravityLoweringOption = getToggleOption(Variant.DisableAutoJumpGravityLowering));
                disableAutoJumpGravityLoweringOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEAUTOJUMPGRAVITYLOWERING_HINT_2"));
                disableAutoJumpGravityLoweringOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEAUTOJUMPGRAVITYLOWERING_HINT_1"));

                TextMenuOptionExt<int> slowfallGravityMult;
                menu.Add(slowfallGravityMult = getScaleOption(Variant.SlowfallGravityMultiplier, "x", multiplierScale));
                slowfallGravityMult.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SLOWFALLGRAVITYMULTIPLIER_HINT"));

                TextMenuOptionExt<int> slowfallSpeedThreshold;
                menu.Add(slowfallSpeedThreshold = getScaleOption(Variant.SlowfallSpeedThreshold, "px/s", new float[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 150, 200, 250, 300, 350, 400, 450, 500, 600, 700, 800, 900, 1000 }));
                slowfallSpeedThreshold.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SLOWFALLSPEEDTHRESHOLD_HINT"));

                menu.Add(buildHeading(menu, "DASHING"));
                menu.Add(getScaleOption(Variant.DashSpeed, "x", multiplierScale));

                TextMenuExt.OnOff legacyDashSpeedBehaviorOption;
                menu.Add(legacyDashSpeedBehaviorOption = getToggleOption(Variant.LegacyDashSpeedBehavior));
                legacyDashSpeedBehaviorOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_LEGACYDASHSPEEDBEHAVIOR_HINT"));

                menu.Add(getScaleOption(Variant.DashLength, "x", multiplierScale));

                TextMenuOptionExt<int> dashTimerMultiplierOption;
                menu.Add(dashTimerMultiplierOption = getScaleOption(Variant.DashTimerMultiplier, "x", multiplierScale));
                dashTimerMultiplierOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHTIMERMULTIPLIER_DESCRIPTION"));

                menu.Add(dashDirection = (TextMenuExt.Slider) new TextMenuExt.Slider(
                        Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION"),
                        i => Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION_{i}"),
                        0, 3,
                        GetDashDirectionIndex((bool[][]) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(Variant.DashDirection)),
                        0, // default
                        GetDashDirectionIndex((bool[][]) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(Variant.DashDirection)))
                    .Change(i => {
                        switch (i) {
                            case 1:
                                setVariantValue(Variant.DashDirection, new bool[][] {
                                    new bool[] { false, true, false },
                                    new bool[] { true, true, true },
                                    new bool[] { false, true, false }
                                });
                                break;
                            case 2:
                                setVariantValue(Variant.DashDirection, new bool[][] {
                                    new bool[] { true, false, true },
                                    new bool[] { false, true, false },
                                    new bool[] { true, false, true }
                                });
                                break;
                            default:
                                setVariantValue(Variant.DashDirection, new bool[][] {
                                    new bool[] { true, true, true },
                                    new bool[] { true, true, true },
                                    new bool[] { true, true, true }
                                });
                                break;
                        }
                    }));

                // build the dash direction submenu.
                dashDirectionsSubMenu = new Celeste.TextMenuExt.SubMenu(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION_ALLOWED"), enterOnSelect: false);
                string[,] directionNames = new string[,] { { "TOPLEFT", "TOP", "TOPRIGHT" }, { "LEFT", "CENTER", "RIGHT" }, { "BOTTOMLEFT", "BOTTOM", "BOTTOMRIGHT" } };
                bool[][] allowedDashDirections = (bool[][]) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(Variant.DashDirection);

                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) {
                        if (i == 1 && j == 1) continue;

                        int a = i, b = j;

                        TextMenu.OnOff toggle = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION_" + directionNames[i, j]), allowedDashDirections[i][j]);
                        toggle.Change(value => {
                            allowedDashDirections[a][b] = value;
                            setVariantValue(Variant.DashDirection, allowedDashDirections);
                        });
                        dashDirectionsSubMenu.Add(toggle);
                    }
                }

                menu.Add(dashDirectionsSubMenu);

                menu.Add(getScaleOption(Variant.HyperdashSpeed, "x", multiplierScale));
                menu.Add(getScaleOption(Variant.SuperdashSteeringSpeed, "x", multiplierScale));

                TextMenuOptionExt<int> ultraSpeedMultiplierOption;
                menu.Add(ultraSpeedMultiplierOption = getScaleOption(Variant.UltraSpeedMultiplier, "x", multiplierScale));
                ultraSpeedMultiplierOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ULTRASPEEDMULTIPLIER_DESC"));

                TextMenuExt.OnOff stretchUpDashesToggle;
                menu.Add(stretchUpDashesToggle = getToggleOption(Variant.StretchUpDashes));
                stretchUpDashesToggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_STRETCHUPDASHES_HINT_2"));
                stretchUpDashesToggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_STRETCHUPDASHES_HINT_1"));

                menu.Add(getScaleOption(Variant.DashCount, "", new int[] { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, i => {
                    if (i == -1) {
                        return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                    }
                    return i.ToString();
                }));
                menu.Add(getToggleOption(Variant.HeldDash));
                menu.Add(getScaleOption(Variant.DontRefillDashOnGround, "", getEnumValues<DontRefillDashOnGround.DashRefillOnGroundConfiguration>(),
                    i => new string[] { Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT"), Dialog.Clean("OPTIONS_ON"), Dialog.Clean("OPTIONS_OFF") }[(int) i]));

                menu.Add(getScaleOption(Variant.DashRestriction, "", getEnumValues<DashRestriction.DashRestrictionType>(),
                    i => new string[] {
                        Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHRESTRICTION_NONE"),
                        Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHRESTRICTION_GROUNDEDONLY"),
                        Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHRESTRICTION_AIRBORNEONLY")
                    }[(int) i]
                ));
                menu.Add(getToggleOption(Variant.DisableRefillsOnScreenTransition));
                menu.Add(getToggleOption(Variant.DontRefillStaminaOnGround));

                TextMenuExt.OnOff restoreDashesOnRespawnOption;
                menu.Add(restoreDashesOnRespawnOption = getToggleOption(Variant.RestoreDashesOnRespawn));
                restoreDashesOnRespawnOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESTOREDASHESONRESPAWN_NOTE"));

                menu.Add(getToggleOption(Variant.PreserveExtraDashesUnderwater));
                menu.Add(getToggleOption(Variant.DisableDashCooldown));
                menu.Add(getScaleOption(Variant.CornerCorrection, "px", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }));

                TextMenuExt.OnOff midairTechToggle;
                menu.Add(midairTechToggle = getToggleOption(Variant.MidairTech));
                if (Dialog.Has("MODOPTIONS_EXTENDEDVARIANTS_MIDAIRTECH_HINT_3")) {
                    // thank the French for that one
                    midairTechToggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MIDAIRTECH_HINT_3"));
                }
                midairTechToggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MIDAIRTECH_HINT_2"));
                midairTechToggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MIDAIRTECH_HINT_1"));

                menu.Add(buildHeading(menu, "MOVING"));
                menu.Add(getScaleOption(Variant.SpeedX, "x", multiplierScale));
                menu.Add(getScaleOption(Variant.UnderwaterSpeedX, "px/s", waterSpeed60));
                menu.Add(getScaleOption(Variant.UnderwaterSpeedY, "px/s", waterSpeed80));
                menu.Add(getScaleOption(Variant.WaterSurfaceSpeedX, "px/s", waterSpeed80));
                menu.Add(getScaleOption(Variant.WaterSurfaceSpeedY, "px/s", waterSpeed80));
                menu.Add(getScaleOption(Variant.Friction, "x", multiplierScaleFriction));
                menu.Add(getScaleOption(Variant.AirFriction, "x", multiplierScaleFriction));
                menu.Add(getScaleOption(Variant.ExplodeLaunchSpeed, "x", multiplierScale));
                menu.Add(getScaleOption(Variant.WallSlidingSpeed, "x", multiplierScale));

                TextMenuExt.OnOff disableSuperBoostsOption;
                menu.Add(disableSuperBoostsOption = getToggleOption(Variant.DisableSuperBoosts));
                disableSuperBoostsOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLESUPERBOOSTS_NOTE"));

                menu.Add(getScaleOption(Variant.BoostMultiplier, "x", multiplierScaleWithNegatives));

                menu.Add(getScaleOption(Variant.LiftboostCapX, "", new float[] { 0.0f, 10.0f, 20.0f, 30.0f, 40.0f, 50.0f, 75.0f, 100.0f, 150.0f, 200.0f, 225.0f, 250.0f, 275.0f, 300.0f, 400.0f, 500.0f, -1.0f }, i => {
                    if (i < 0.0f) {
                        return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_LIFTBOOSTUNCAPPED");
                    }
                    return $"{i:F1}px/s";
                }));
                menu.Add(getScaleOption(Variant.LiftboostCapUp, "", new float[] { 0.0f, -10.0f, -20.0f, -30.0f, -40.0f, -50.0f, -75.0f, -100.0f, -110.0f, -120.0f, -130.0f, -140.0f, -150.0f, -200.0f, -250.0f, -300.0f, -400.0f, -500.0f, 1.0f }, i => {
                    if (i > 0.0f) {
                        return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_LIFTBOOSTUNCAPPED");
                    }
                    return $"{i:F1}px/s";
                }));
                menu.Add(getScaleOption(Variant.LiftboostCapDown, "", new float[] { 0.0f, 10.0f, 20.0f, 30.0f, 40.0f, 50.0f, 75.0f, 100.0f, 150.0f, 200.0f, 240.0f, 250.0f, 300.0f, 400.0f, 500.0f, -1.0f }, i => {
                    if (i < 0.0f) {
                        return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_LIFTBOOSTUNCAPPED");
                    }
                    return $"{i:F1}px/s";
                }));

                menu.Add(getScaleOption(Variant.DisableClimbingUpOrDown, "", getEnumValues<DisableClimbingUpOrDown.ClimbUpOrDownOptions>(),
                    i => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLECLIMBINGUPORDOWN_" + i)));
                menu.Add(getScaleOption(Variant.HorizontalSpringBounceDuration, "x", multiplierScale));
                menu.Add(getScaleOption(Variant.FastFallAcceleration, "x", multiplierScale));

                TextMenuExt.OnOff trueNoGrabbingOption;
                menu.Add(trueNoGrabbingOption = getToggleOption(Variant.TrueNoGrabbing));
                trueNoGrabbingOption.AddDescription(menu, Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_TRUENOGRABBING_HINT_2"));
                trueNoGrabbingOption.AddDescription(menu, Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_TRUENOGRABBING_HINT_1"));

                menu.Add(buildHeading(menu, "HOLDABLES"));
                menu.Add(getScaleOption(Variant.PickupDuration, "x", multiplierScale));
                menu.Add(getScaleOption(Variant.MinimumDelayBeforeThrowing, "x", multiplierScale));
                menu.Add(getScaleOption(Variant.DelayBeforeRegrabbing, "x", multiplierScale));
            }

            // ======

            if (category == VariantCategory.GameElements) {
                menu.Add(buildHeading(menu, "CHASERS"));
                menu.Add(getToggleOption(Variant.BadelineChasersEverywhere));
                menu.Add(getScaleOption(Variant.ChaserCount, "", new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }));
                menu.Add(getToggleOption(Variant.AffectExistingChasers));
                menu.Add(getScaleOption(Variant.BadelineLag, "s", multiplierScaleBadelineLag));
                menu.Add(getScaleOption(Variant.DelayBetweenBadelines, "s", multiplierScale));

                menu.Add(buildHeading(menu, "BOSSES"));
                menu.Add(getToggleOption(Variant.BadelineBossesEverywhere));
                menu.Add(getScaleOption(Variant.BadelineAttackPattern, "", badelineBossesPatternsOptions, i => {
                    if (i == 0) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_WINDEVERYWHERE_RANDOM");
                    return Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_BADELINEPATTERN_{i}");
                }));
                menu.Add(getToggleOption(Variant.ChangePatternsOfExistingBosses));
                menu.Add(firstBadelineSpawnRandomOption = getScaleOption(Variant.FirstBadelineSpawnRandom, "", new bool[] { false, true },
                    b => b ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FIRSTBADELINESPAWNRANDOM_ON") : Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FIRSTBADELINESPAWNRANDOM_OFF")));
                menu.Add(badelineBossCountOption = getScaleOption(Variant.BadelineBossCount, "", new int[] { 1, 2, 3, 4, 5 }));
                menu.Add(badelineBossNodeCountOption = getScaleOption(Variant.BadelineBossNodeCount, "", new int[] { 1, 2, 3, 4, 5 }));

                menu.Add(buildHeading(menu, "OSHIRO"));
                menu.Add(getToggleOption(Variant.OshiroEverywhere));
                menu.Add(oshiroCountOption = getScaleOption(Variant.OshiroCount, "", new int[] { 0, 1, 2, 3, 4, 5 }));
                if (Instance.DJMapHelperInstalled) menu.Add(reverseOshiroCountOption = getScaleOption(Variant.ReverseOshiroCount, "", new int[] { 0, 1, 2, 3, 4, 5 }));
                menu.Add(getToggleOption(Variant.DisableOshiroSlowdown));

                menu.Add(buildHeading(menu, "THEO"));
                menu.Add(getToggleOption(Variant.TheoCrystalsEverywhere));

                menu.Add(allowThrowingTheoOffscreenOption = getToggleOption(Variant.AllowThrowingTheoOffscreen));
                allowThrowingTheoOffscreenOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AllowThrowingTheoOffscreen_desc2"));
                allowThrowingTheoOffscreenOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AllowThrowingTheoOffscreen_desc1"));

                menu.Add(allowLeavingTheoBehindOption = getToggleOption(Variant.AllowLeavingTheoBehind));

                menu.Add(buildHeading(menu, "EVERYWHERE"));
                menu.Add(getScaleOption(Variant.WindEverywhere, "", getEnumValues<WindEverywhere.WindPattern>(),
                    e => Dialog.Clean(e == WindEverywhere.WindPattern.Default ? "MODOPTIONS_EXTENDEDVARIANTS_DISABLED" : "MODOPTIONS_EXTENDEDVARIANTS_WINDEVERYWHERE_" + e)));
                menu.Add(getToggleOption(Variant.SnowballsEverywhere));
                menu.Add(getScaleOption(Variant.SnowballDelay, "s", multiplierScale));
                menu.Add(getScaleOption(Variant.AddSeekers, "", new int[] { 0, 1, 2, 3, 4, 5 }));
                menu.Add(getToggleOption(Variant.DisableSeekerSlowdown));
                menu.Add(getScaleOption(Variant.JellyfishEverywhere, "", new int[] { 0, 1, 2, 3 }));
                menu.Add(getToggleOption(Variant.RisingLavaEverywhere));
                menu.Add(getScaleOption(Variant.RisingLavaSpeed, "x", multiplierScale));
                if (Instance.JungleHelperInstalled)
                    menu.Add(getScaleOption(Variant.JungleSpidersEverywhere, "", getEnumValues<JungleSpidersEverywhere.SpiderType>(),
                        e => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_JUNGLESPIDERSEVERYWHERE_" + e)));
            }

            // ======

            if (category == VariantCategory.Visual) {
                menu.Add(buildHeading(menu, "MADELINE"));
                menu.Add(getToggleOption(Variant.DisableMadelineSpotlight));
                if (Instance.MaxHelpingHandInstalled || Instance.SpringCollab2020Installed) menu.Add(getToggleOption(Variant.MadelineIsSilhouette));
                menu.Add(getToggleOption(Variant.DashTrailAllTheTime));
                if (Instance.MaxHelpingHandInstalled) menu.Add(getToggleOption(Variant.MadelineHasPonytail));
                menu.Add(getToggleOption(Variant.DisplayDashCount));
                menu.Add(getScaleOption(Variant.DisplaySpeedometer, "", getEnumValues<DisplaySpeedometer.SpeedometerConfiguration>(),
                    e => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISPLAYSPEEDOMETER_" + e)));
                menu.Add(getScaleOption(Variant.MadelineBackpackMode, "", getEnumValues<MadelineBackpackMode.MadelineBackpackModes>(),
                    e => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MADELINEBACKPACKMODE_" + e)));

                menu.Add(buildHeading(menu, "LEVEL"));
                menu.Add(getToggleOption(Variant.UpsideDown));
                menu.Add(getScaleOption(Variant.RoomLighting, "", new float[] { -1f, 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f }, f => {
                    if (f == -1) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                    return $"{f * 100}%";
                }));
                menu.Add(getScaleOption(Variant.BackgroundBrightness, "", new float[] { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f }, f => $"{f * 100}%"));
                menu.Add(getScaleOption(Variant.ForegroundEffectOpacity, "", new float[] { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f }, f => $"{f * 100}%"));
                menu.Add(getScaleOption(Variant.RoomBloom, "", new float[] { -1f, 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 2f, 3f, 4f, 5f },
                    f => f == -1f ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT") : $"{f * 100}%"));
                menu.Add(getScaleOption(Variant.GlitchEffect, "", new float[] { -1f, 0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f },
                    f => f == -1f ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT") : $"{f * 100}%"));
                menu.Add(getScaleOption(Variant.AnxietyEffect, "", new float[] { -1f, 0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f },
                    f => f == -1f ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT") : $"{f * 100}%"));
                menu.Add(getScaleOption(Variant.BlurLevel, "", new float[] { 0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f }, f => $"{f * 100}%"));
                menu.Add(getScaleOption(Variant.BackgroundBlurLevel, "", new float[] { 0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f },
                    f => $"{f * 100}%"));
                menu.Add(getScaleOption(Variant.ZoomLevel, "x", multiplierScale));

                // go through all Everest mod assets, and list all color grades that exist.
                List<string> allColorGrades = new List<string>(ColorGrading.ExistingColorGrades);
                foreach (string colorgrade in Everest.Content.Map.Values
                    .Where(asset => asset.Type == typeof(Texture2D) && asset.PathVirtual.StartsWith("Graphics/ColorGrading/"))
                    .Select(asset => asset.PathVirtual.Substring("Graphics/ColorGrading/".Length))) {

                    if (!allColorGrades.Contains(colorgrade)) {
                        allColorGrades.Add(colorgrade);
                    }
                }
                allColorGrades.Insert(0, "");

                menu.Add(getScaleOption(Variant.ColorGrading, "", allColorGrades.ToArray(), colorGradeName => {
                    if (string.IsNullOrEmpty(colorGradeName)) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");

                    if (!ColorGrading.ExistingColorGrades.Contains(colorGradeName)) {
                        // mod color grade - try formatting it somewhat nicely.
                        if (colorGradeName.Contains("/")) colorGradeName = colorGradeName.Substring(colorGradeName.LastIndexOf("/") + 1);
                        if (colorGradeName.Length > 15) colorGradeName = colorGradeName.Substring(0, 15) + "...";
                        return "Mod - " + colorGradeName.SpacedPascalCase();
                    }

                    // "none" => read MODOPTIONS_EXTENDEDVARIANTS_CG_none
                    // "celsius/tetris" => read MODOPTIONS_EXTENDEDVARIANTS_CG_tetris
                    if (colorGradeName.Contains("/")) colorGradeName = colorGradeName.Substring(colorGradeName.LastIndexOf("/") + 1);
                    return Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_CG_{colorGradeName}");
                }));

                menu.Add(getToggleOption(Variant.DisableKeysSpotlight));
                menu.Add(getScaleOption(Variant.SpinnerColor, "", new SpinnerColor.Color[] {
                    SpinnerColor.Color.Default, SpinnerColor.Color.Blue, SpinnerColor.Color.Red, SpinnerColor.Color.Purple, SpinnerColor.Color.Rainbow
                }, i => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SPINNERCOLOR_" + i)));
                menu.Add(getScaleOption(Variant.ScreenShakeIntensity, "x", multiplierScale));
                menu.Add(getToggleOption(Variant.FriendlyBadelineFollower));
            }

            // ======

            if (category == VariantCategory.GameplayTweaks) {
                menu.Add(buildHeading(menu, "OTHER"));
                menu.Add(getScaleOption(Variant.GameSpeed, "x", multiplierScale));

                TextMenuExt.OnOff noFreezeFramesOption;
                menu.Add(noFreezeFramesOption = (TextMenuExt.OnOff) getToggleOption(Variant.NoFreezeFrames)
                    .Change(newValue => {
                        setVariantValue(Variant.NoFreezeFrames, newValue);
                        refreshOptionMenuEnabledStatus();
                    }));
                noFreezeFramesOption.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_NOFREEZEFRAMES_DESC"));

                menu.Add(noFreezeFramesAdvanceCassetteBlocksOption = getToggleOption(Variant.NoFreezeFramesAdvanceCassetteBlocks));

                menu.Add(getToggleOption(Variant.EverythingIsUnderwater));
                menu.Add(getToggleOption(Variant.AlwaysFeather));
                menu.Add(getToggleOption(Variant.PermanentDashAttack));

                TextMenuExt.OnOff autoJumpToggle;
                menu.Add(autoJumpToggle = getToggleOption(Variant.AutoJump));
                autoJumpToggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AUTOJUMP_HINT"));

                TextMenuExt.OnOff autoDashToggle;
                menu.Add(autoDashToggle = getToggleOption(Variant.AutoDash));
                autoDashToggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_AUTODASH_HINT"));

                TextMenuExt.OnOff binoStorageToggle;
                menu.Add(binoStorageToggle = getToggleOption(Variant.PermanentBinoStorage));
                binoStorageToggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_PERMANENTBINOSTORAGE_HINT"));

                menu.Add(getScaleOption(Variant.Stamina, "", new int[] {
                    0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250, 260, 270, 280, 290, 300,
                    310, 320, 330, 340, 350, 360, 370, 380, 390, 400, 410, 420, 430, 440, 450, 460, 470, 480, 490, 500
                }));
                menu.Add(getScaleOption(Variant.RegularHiccups, "", multiplierScale, f => f == 0f ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLED") : $"{f}s"));
                menu.Add(getScaleOption(Variant.HiccupStrength, "x", multiplierScale));
                menu.Add(getToggleOption(Variant.AllStrawberriesAreGoldens));
                menu.Add(getToggleOption(Variant.AlwaysInvisible));
                menu.Add(correctedMirrorModeOption = getToggleOption(Variant.CorrectedMirrorMode));

                menu.Add(buildHeading(menu, "TROLL"));
                menu.Add(getToggleOption(Variant.ForceDuckOnGround));
                menu.Add(getToggleOption(Variant.InvertDashes));
                menu.Add(getToggleOption(Variant.InvertGrab));
                menu.Add(getToggleOption(Variant.InvertHorizontalControls));
                menu.Add(getToggleOption(Variant.InvertVerticalControls));
                menu.Add(getToggleOption(Variant.BounceEverywhere));
            }

            // ======

            if (category == VariantCategory.QualityOfLife) {
                menu.Add(buildHeading(menu, "QUALITYOFLIFE"));

                foreach (Variant variant in new Variant[] { Variant.UltraProtection, Variant.LiftboostProtection, Variant.CornerboostProtection, Variant.CrouchDashFix, Variant.AlternativeBuffering }) {
                    TextMenuExt.OnOff option = getToggleOption(variant);
                    menu.Add(option);
                    option.AddDescription(menu, Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_{variant}_HINT_2"));
                    option.AddDescription(menu, Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_{variant}_HINT_1"));

                    if (variant == Variant.UltraProtection) {
                        option.OnEnter.Invoke();
                    }
                }

                TextMenuOptionExt<int> multiBufferingOption = getScaleOption(Variant.MultiBuffering, "", new int[] { 1, 2, 3 },
                    value => value == 1 ? Dialog.Clean("options_off") : value.ToString());
                menu.Add(multiBufferingOption);
                multiBufferingOption.AddDescription(menu, Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_MULTIBUFFERING_HINT_2"));
                multiBufferingOption.AddDescription(menu, Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_MULTIBUFFERING_HINT_1"));

                foreach (Variant variant in new Variant[] { Variant.SaferDiagonalSmuggle, Variant.DashBeforePickup, Variant.ThrowIgnoresForcedMove }) {
                    TextMenuExt.OnOff option = getToggleOption(variant);
                    menu.Add(option);
                    option.AddDescription(menu, Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_{variant}_HINT_2"));
                    option.AddDescription(menu, Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_{variant}_HINT_1"));
                }
            }

            if (includeRandomizer) {
                menu.Add(new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_OSD")));

                menu.Add(new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_DISPLAYONSCREEN"), ExtendedVariantsModule.Settings.DisplayEnabledVariantsToScreen)
                    .Change(newValue => ExtendedVariantsModule.Settings.DisplayEnabledVariantsToScreen = newValue));

                menu.Add(getNonVariantScaleOption("OSD_TEXTOPACITY", "", new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f },
                    Settings.OnScreenDisplayTextOpacity, 1f, 1f, newValue => Settings.OnScreenDisplayTextOpacity = newValue, v => (int) (v * 100) + "%"));
                menu.Add(getNonVariantScaleOption("OSD_BGOPACITY", "", new float[] { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f },
                    Settings.OnScreenDisplayBackgroundOpacity, 0.8f, 0.8f, newValue => Settings.OnScreenDisplayBackgroundOpacity = newValue, v => "" + (int) (v * 100) + "%"));


                menu.Add(buildHeading(menu, "RANDOMIZER"));

                menu.Add((TextMenuExt.OnOff) new TextMenuExt.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHANGEVARIANTSRANDOMLY"), Settings.ChangeVariantsRandomly, false, false)
                    .Change(b => {
                        Settings.ChangeVariantsRandomly = b;
                        refreshOptionMenuEnabledStatus();
                    }));

                menu.Add(randomizerOptions = AbstractSubmenu.BuildOpenMenuButton<OuiRandomizerOptions>(menu, inGame, submenuBackAction, new object[0]));
            }

            InputSearchUI.Instance?.RegisterMenuEvents(menu, showSearchUI: category != VariantCategory.None);
            refreshOptionMenuEnabledStatus();
        }

        private TextMenu.SubHeader buildHeading(TextMenu menu, string headingNameResource) {
            return new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HEADING") + " - " + Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HEADING_" + headingNameResource));
        }

        private void refreshOptionMenuEnabledStatus() {
            // hide everything if the master switch is off, show everything if it is on.
            foreach (TextMenu.Item item in elementsToHideOnToggle) {
                if (item != null) item.Visible = Settings.MasterSwitch;
            }

            ExtendedVariantTriggerManager triggerManager = ExtendedVariantsModule.Instance.TriggerManager;

            // special graying-out rules for some variant options
            if (oshiroCountOption != null) oshiroCountOption.Disabled = !((bool) triggerManager.GetCurrentVariantValue(Variant.OshiroEverywhere));
            if (reverseOshiroCountOption != null) reverseOshiroCountOption.Disabled = !((bool) triggerManager.GetCurrentVariantValue(Variant.OshiroEverywhere));
            if (randomizerOptions != null) randomizerOptions.Disabled = !Settings.ChangeVariantsRandomly;
            if (firstBadelineSpawnRandomOption != null) firstBadelineSpawnRandomOption.Disabled = !((bool) triggerManager.GetCurrentVariantValue(Variant.BadelineBossesEverywhere));
            if (badelineBossCountOption != null) badelineBossCountOption.Disabled = !((bool) triggerManager.GetCurrentVariantValue(Variant.BadelineBossesEverywhere));
            if (badelineBossNodeCountOption != null) badelineBossNodeCountOption.Disabled = !((bool) triggerManager.GetCurrentVariantValue(Variant.BadelineBossesEverywhere));
            if (allowThrowingTheoOffscreenOption != null) allowThrowingTheoOffscreenOption.Disabled = !((bool) triggerManager.GetCurrentVariantValue(Variant.TheoCrystalsEverywhere));
            if (allowLeavingTheoBehindOption != null) allowLeavingTheoBehindOption.Disabled = !((bool) triggerManager.GetCurrentVariantValue(Variant.TheoCrystalsEverywhere));
            if (dashDirectionsSubMenu != null) dashDirectionsSubMenu.Visible = dashDirection.Index == 3;
            if (correctedMirrorModeOption != null) correctedMirrorModeOption.Disabled = !(Engine.Scene is Level) || !((bool) triggerManager.GetCurrentVariantValue(Variant.MirrorMode) || SaveData.Instance.Assists.MirrorMode);
            if (noFreezeFramesAdvanceCassetteBlocksOption != null) noFreezeFramesAdvanceCassetteBlocksOption.Disabled = !((bool) triggerManager.GetCurrentVariantValue(Variant.NoFreezeFrames));
        }

        // gets the index of the currently selected Dash Direction index, from 0 to 3
        public static int GetDashDirectionIndex(bool[][] settings) {
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
