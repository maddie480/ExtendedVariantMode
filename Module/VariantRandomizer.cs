using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using ExtendedVariants.UI;
using ExtendedVariants.Variants;
using ExtendedVariants.Variants.Vanilla;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedVariants {
    public class VariantRandomizer {
        private Random randomGenerator = new Random();

        private float variantChangeTimer = -1f;
        private float vanillafyTimer = -1f;

        private InfoPanel infoPanel = new InfoPanel();
        private VariantsIndicator variantsIndicator = new VariantsIndicator();
        private bool mustUpdateDisplayedVariantList = false;


        public void Load() {
            On.Celeste.Level.Begin += onLevelBegin;
            On.Celeste.Level.End += onLevelEnd;
            On.Celeste.Level.TransitionRoutine += onRoomChange;
            On.Celeste.Player.Update += onUpdate;
            On.Celeste.Level.EndPauseEffects += modEndPauseEffects;

            if (Engine.Scene != null && Engine.Scene.GetType() == typeof(Level)) {
                // we're late, catch up!
                onLevelBegin();
            }
        }

        public void Unload() {
            On.Celeste.Level.Begin -= onLevelBegin;
            On.Celeste.Level.End -= onLevelEnd;
            On.Celeste.Level.TransitionRoutine -= onRoomChange;
            On.Celeste.Player.Update -= onUpdate;
            On.Celeste.Level.EndPauseEffects -= modEndPauseEffects;

            // clear up stuff as well
            onLevelEnd();
        }

        private void onLevelBegin(On.Celeste.Level.orig_Begin orig, Level self) {
            orig(self);

            onLevelBegin();

            // check if we are starting a set seed randomizer session.
            if (ExtendedVariantsModule.Settings.ChangeVariantsRandomly && ExtendedVariantsModule.Settings.RandoSetSeed != null) {
                // the seed is actually the set seed + starting level seed.
                // this way, this is consistent when we start the same level, but we don't get the same set of variants all the time either.
                string setSeedString = ExtendedVariantsModule.Settings.RandoSetSeed + "/" + self.Session.LevelData.LoadSeed.ToString();
                int setSeed = setSeedString.GetHashCode();

                Logger.Log(LogLevel.Info, "ExtendedVariantMode/VariantRandomizer", $"Variant randomizer seed is: [{setSeedString}] => {setSeed}");

                randomGenerator = new Random(setSeed);
                foreach (AbstractExtendedVariant variant in ExtendedVariantsModule.Instance.VariantHandlers.Values) {
                    variant.SetRandomSeed(setSeed);
                }
            }
        }

        private void onLevelBegin() {
            UpdateCountersFromSettings();
            RefreshEnabledVariantsDisplayList();

            // inject the code to display the list of enabled variants
            Logger.Log("ExtendedVariantMode/VariantRandomizer", "Hooking HudRenderer to display list of enabled variants");
            On.Celeste.HudRenderer.RenderContent += modRenderContent;
        }

        private void onLevelEnd(On.Celeste.Level.orig_End orig, Level self) {
            orig(self);

            onLevelEnd();
        }

        private void onLevelEnd() {
            // level is being exited, clear up the hook that displays the enabled variants
            Logger.Log("ExtendedVariantMode/VariantRandomizer", "Unhooking HudRenderer to hide list of enabled variants");
            On.Celeste.HudRenderer.RenderContent -= modRenderContent;
        }

        private void modEndPauseEffects(On.Celeste.Level.orig_EndPauseEffects orig, Level self) {
            orig(self);

            // refresh the display in case the player changed anything in the pause menu
            RefreshEnabledVariantsDisplayList();
        }

        private void modRenderContent(On.Celeste.HudRenderer.orig_RenderContent orig, HudRenderer self, Scene scene) {
            orig(self, scene);
            if (!((scene as Level)?.Paused ?? false)) {
                Draw.SpriteBatch.Begin();

                if (shouldDisplayEnabledVariantsOnScreen()) {
                    if (mustUpdateDisplayedVariantList) {
                        Logger.Log("ExtendedVariantMode/VariantRandomizer", "Late update of displayed enabled variants on-screen");
                        RefreshEnabledVariantsDisplayList();
                        mustUpdateDisplayedVariantList = false;
                    }

                    infoPanel.Render();
                } else {
                    variantsIndicator.Render();
                }

                Draw.SpriteBatch.End();
            }
        }

        private bool shouldDisplayEnabledVariantsOnScreen() {
            return ExtendedVariantsModule.Settings.DisplayEnabledVariantsToScreen ||
                (ExtendedVariantsModule.Session != null && ExtendedVariantsModule.Session.ExtendedVariantsDisplayedOnScreenViaTrigger);
        }

        public void UpdateCountersFromSettings() {
            variantChangeTimer = ExtendedVariantsModule.Settings.ChangeVariantsInterval;
            vanillafyTimer = ExtendedVariantsModule.Settings.Vanillafy;

            Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Updated variables from settings: variantChangeTimer = {variantChangeTimer}, vanillafyTimer = {vanillafyTimer}");
        }

        private IEnumerator onRoomChange(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            if (ExtendedVariantsModule.Settings.ChangeVariantsRandomly) {
                if (ExtendedVariantsModule.Settings.ChangeVariantsInterval == 0) {
                    // variants should be changed on room transition => go go
                    changeVariantNow();
                }

                if (ExtendedVariantsModule.Settings.Vanillafy != 0) {
                    // we should also reset the vanillafy timer
                    vanillafyTimer = ExtendedVariantsModule.Settings.Vanillafy;
                    Logger.Log("ExtendedVariantMode/VariantRandomizer", $"vanillafyTimer reset to {vanillafyTimer}");
                }
            }

            yield return new SwapImmediately(orig(self, next, direction));
        }

        private void onUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            if (ExtendedVariantsModule.Settings.ChangeVariantsRandomly) {
                if (ExtendedVariantsModule.Settings.ChangeVariantsInterval != 0) {
                    variantChangeTimer -= Engine.RawDeltaTime;
                    if (variantChangeTimer <= 0f) {
                        // variant timer is over => change variant now!
                        changeVariantNow();

                        variantChangeTimer = ExtendedVariantsModule.Settings.ChangeVariantsInterval;
                        Logger.Log("ExtendedVariantMode/VariantRandomizer", $"variantChangeTimer reset to {variantChangeTimer}");
                    }
                }


                if (ExtendedVariantsModule.Settings.Vanillafy != 0) {
                    vanillafyTimer -= Engine.RawDeltaTime;
                    if (vanillafyTimer <= 0f) {
                        // disable a variant
                        changeVariantNow(true);

                        vanillafyTimer = ExtendedVariantsModule.Settings.Vanillafy;
                        Logger.Log("ExtendedVariantMode/VariantRandomizer", $"vanillafyTimer reset to {vanillafyTimer}");
                    }
                }
            }

            orig(self);
        }

        private void changeVariantNow(bool disableOnly = false) {
            // get filtered lists for changeable variants, and those which are enabled
            IEnumerable<ExtendedVariantsModule.Variant> changeableVanillaVariants = new List<ExtendedVariantsModule.Variant>();
            if (SaveData.Instance.VariantMode && ExtendedVariantsModule.Settings.VariantSet % 2 == 1)
                changeableVanillaVariants = ExtendedVariantsModule.Instance.VariantHandlers.Keys
                    .Where(variant => ExtendedVariantsModule.Instance.VariantHandlers[variant] is AbstractVanillaVariant &&
                        (ExtendedVariantsModule.Settings.RandomizerEnabledVariants.TryGetValue(variant.ToString(), out bool enabled) ? enabled : true));

            IEnumerable<ExtendedVariantsModule.Variant> changeableExtendedVariants = new List<ExtendedVariantsModule.Variant>();
            if (ExtendedVariantsModule.Settings.VariantSet / 2 == 1)
                changeableExtendedVariants = ExtendedVariantsModule.Instance.VariantHandlers.Keys
                    .Where(variant => ExtendedVariantsModule.Instance.VariantHandlers[variant] is not AbstractVanillaVariant &&
                        (ExtendedVariantsModule.Settings.RandomizerEnabledVariants.TryGetValue(variant.ToString(), out bool enabled) ? enabled : true));

            IEnumerable<ExtendedVariantsModule.Variant> enabledVanillaVariants = changeableVanillaVariants.Where(variant => !isDefaultValue(variant));
            IEnumerable<ExtendedVariantsModule.Variant> enabledExtendedVariants = changeableExtendedVariants.Where(variant => !isDefaultValue(variant));

            if (!disableOnly && ExtendedVariantsModule.Settings.RerollMode) {
                Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Rerolling: disabling all variants");
                foreach (ExtendedVariantsModule.Variant variant in enabledVanillaVariants) disableVariant(variant);
                foreach (ExtendedVariantsModule.Variant variant in enabledExtendedVariants.ToList()) disableVariant(variant);

                Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Rerolling: enabling {ExtendedVariantsModule.Settings.MaxEnabledVariants} variants");

                // give numbers to all variants
                List<int> variantNumbers = new List<int>();
                for (int i = 0; i < changeableVanillaVariants.Count() + changeableExtendedVariants.Count(); i++) variantNumbers.Add(i);

                // remove numbers until there are few enough left
                while (variantNumbers.Count() > ExtendedVariantsModule.Settings.MaxEnabledVariants) {
                    variantNumbers.RemoveAt(randomGenerator.Next(variantNumbers.Count()));
                }

                // and enable those specific variants
                int index = 0;
                foreach (ExtendedVariantsModule.Variant variant in changeableVanillaVariants)
                    if (variantNumbers.Contains(index++)) enableVariant(variant);
                foreach (ExtendedVariantsModule.Variant variant in changeableExtendedVariants.ToList())
                    if (variantNumbers.Contains(index++)) enableVariant(variant);
            } else {
                // pick a random variant (if disableOnly or the max variant count has been reached, pick from the enabled ones)
                if (disableOnly || (enabledVanillaVariants.Count() + enabledExtendedVariants.Count() >= ExtendedVariantsModule.Settings.MaxEnabledVariants)) {
                    Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Randomizing: picking a variant in disableOnly mode " +
                        $"({enabledVanillaVariants.Count() + enabledExtendedVariants.Count()} enabled, {ExtendedVariantsModule.Settings.MaxEnabledVariants} max)");

                    if (enabledVanillaVariants.Count() + enabledExtendedVariants.Count() == 0) {
                        Logger.Log(LogLevel.Warn, "ExtendedVariantMode/VariantRandomizer", "ESCAPE: We are in disableOnly mode and no variant is enabled.");
                        return;
                    }

                    // pick a random variant from the enabled ones, and disable it
                    int drawnVariant = randomGenerator.Next(enabledVanillaVariants.Count() + enabledExtendedVariants.Count());
                    foreach (ExtendedVariantsModule.Variant variant in enabledVanillaVariants)
                        if (drawnVariant-- == 0) disableVariant(variant);
                    foreach (ExtendedVariantsModule.Variant variant in enabledExtendedVariants.ToList())
                        if (drawnVariant-- == 0) disableVariant(variant);
                } else {
                    Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Randomizing: picking a variant at random " +
                        $"({enabledVanillaVariants.Count() + enabledExtendedVariants.Count()} enabled, {ExtendedVariantsModule.Settings.MaxEnabledVariants} max)");

                    if (changeableVanillaVariants.Count() + changeableExtendedVariants.Count() == 0) {
                        Logger.Log(LogLevel.Warn, "ExtendedVariantMode/VariantRandomizer", "ESCAPE: We must change a variant, but no variant is changeable.");
                        return;
                    }

                    // pick a random variant from all the variants the randomizer can manipulate, and toggle it
                    int drawnVariant = randomGenerator.Next(changeableVanillaVariants.Count() + changeableExtendedVariants.Count());
                    foreach (ExtendedVariantsModule.Variant variant in changeableVanillaVariants)
                        if (drawnVariant-- == 0) {
                            if (isDefaultValue(variant)) enableVariant(variant);
                            else disableVariant(variant);
                        }
                    foreach (ExtendedVariantsModule.Variant variant in changeableExtendedVariants.ToList())
                        if (drawnVariant-- == 0) {
                            if (isDefaultValue(variant)) enableVariant(variant);
                            else disableVariant(variant);
                        }
                }
            }

            RefreshEnabledVariantsDisplayList();
        }

        private bool isDefaultValue(ExtendedVariantsModule.Variant variant) {
            AbstractExtendedVariant variantHandler = ExtendedVariantsModule.Instance.VariantHandlers[variant];
            if (variantHandler is AbstractVanillaVariant vanillaVariantHandler) {
                return vanillaVariantHandler.IsSetToDefaultByPlayer();
            } else {
                return ExtendedVariantTriggerManager.AreValuesIdentical(
                    ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(variant),
                    variantHandler.GetDefaultVariantValue());
            }
        }

        private void disableVariant(ExtendedVariantsModule.Variant variant) {
            Logger.Log(LogLevel.Info, "ExtendedVariantMode/VariantRandomizer", $"Disabling variant {variant.ToString()}");

            AbstractExtendedVariant variantHandler = ExtendedVariantsModule.Instance.VariantHandlers[variant];
            setVariantValue(variant, variantHandler.GetDefaultVariantValue());
        }

        private void enableVariant(ExtendedVariantsModule.Variant variant) {
            Logger.Log(LogLevel.Info, "ExtendedVariantMode/VariantRandomizer", $"Enabling variant {variant.ToString()}");

            AbstractExtendedVariant extendedVariant = ExtendedVariantsModule.Instance.VariantHandlers[variant];

            if (variant == ExtendedVariantsModule.Variant.DashDirection) {
                // random between "diagonals only" and "no diagonals"
                setVariantValue(variant, getRandomDashDirection());

            } else if (variant == ExtendedVariantsModule.Variant.BadelineAttackPattern) {
                // random between a set of values
                int[] badelineBossesPatternsOptions = { 1, 2, 3, 4, 5, 9, 10, 14, 15 };
                setVariantValue(variant, badelineBossesPatternsOptions[randomGenerator.Next(badelineBossesPatternsOptions.Length)]);

            } else if (variant == ExtendedVariantsModule.Variant.ColorGrading) {
                // random between all color grades shipping with extended variants
                setVariantValue(variant, ColorGrading.ExistingColorGrades[randomGenerator.Next(ColorGrading.ExistingColorGrades.Count)]);

            } else if (variant == ExtendedVariantsModule.Variant.JellyfishEverywhere) {
                // random 1-3
                setVariantValue(variant, randomGenerator.Next(3) + 1);

            } else if (variant == ExtendedVariantsModule.Variant.JumpCount) {
                // random 0-5
                setVariantValue(variant, randomGenerator.Next(6));

            } else if (variant == ExtendedVariantsModule.Variant.Stamina) {
                // random 0-220 (so 0x to 2x the vanilla value)
                setVariantValue(variant, randomGenerator.Next(220));

            } else if (variant == ExtendedVariantsModule.Variant.CornerCorrection) {
                // random 0-9
                setVariantValue(variant, randomGenerator.Next(10));

            } else if (variant == ExtendedVariantsModule.Variant.BoostMultiplier) {
                // same scale as other multiplier variants... except it can be negative as well!
                float[] multiplierScale = new float[] {
                    0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2, 2.5f, 3
                };
                float result = multiplierScale[randomGenerator.Next(multiplierScale.Length)];
                if (randomGenerator.Next() > 0.5) result *= -1;

                setVariantValue(variant, result);

            } else if (variant == ExtendedVariantsModule.Variant.GameSpeed) {
                // same scale as other multiplier variants, but without 0 for obvious reasons.
                float[] multiplierScale = new float[] {
                    0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2, 2.5f, 3
                };
                setVariantValue(variant, multiplierScale[randomGenerator.Next(multiplierScale.Length)]);

            } else if (variant == ExtendedVariantsModule.Variant.VanillaGameSpeed) {
                setVariantValue(variant, Variants.Vanilla.GameSpeed.ValidValues[randomGenerator.Next(Variants.Vanilla.GameSpeed.ValidValues.Length)]);

            } else if (new ExtendedVariantsModule.Variant[] { ExtendedVariantsModule.Variant.RoomLighting, ExtendedVariantsModule.Variant.BackgroundBrightness, ExtendedVariantsModule.Variant.ForegroundEffectOpacity,
                ExtendedVariantsModule.Variant.GlitchEffect, ExtendedVariantsModule.Variant.AnxietyEffect, ExtendedVariantsModule.Variant.BlurLevel, ExtendedVariantsModule.Variant.BackgroundBlurLevel }
                .Contains(variant)) {
                // percentage variants: 0-100% by steps of 10%
                setVariantValue(variant, randomGenerator.Next(11) / 10f);

            } else if (extendedVariant.GetVariantType() == typeof(bool)) {
                // to toggle the value, well... just toggle it. Easy!
                setVariantValue(variant, !((bool) extendedVariant.GetDefaultVariantValue()));

            } else if (extendedVariant.GetVariantType() == typeof(int)) {
                // 1-5 is good for most variants.
                setVariantValue(variant, randomGenerator.Next(5) + 1);

            } else if (extendedVariant.GetVariantType() == typeof(float)) {
                // this is for multiplier variants!
                float[] multiplierScale = new float[] {
                    0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2, 2.5f, 3
                };

                setVariantValue(variant, multiplierScale[randomGenerator.Next(multiplierScale.Length)]);

            } else if (extendedVariant.GetVariantType().IsEnum) {
                // enum variants
                Array vals = Enum.GetValues(extendedVariant.GetVariantType());
                setVariantValue(variant, vals.GetValue(randomGenerator.Next(vals.Length)));

            } else {
                throw new NotImplementedException("Cannot randomize variant " + variant + "!");
            }

            if (isDefaultValue(variant)) {
                // we randomly generated a default value so the variant isn't enabled - try again!
                enableVariant(variant);
            }
        }

        private void setVariantValue(ExtendedVariantsModule.Variant variant, object value) {
            if (ExtendedVariantsModule.Instance.VariantHandlers[variant] is AbstractVanillaVariant) {
                VanillaVariantOptions.SetVariantValue(variant, value);
            } else {
                ModOptionsEntries.SetVariantValue(variant, value);
            }
        }

        private bool[][] getRandomDashDirection() {
            if (randomGenerator.Next(2) == 0) {
                return new bool[][] {
                    new bool[] { false, true, false },
                    new bool[] { true, true, true },
                    new bool[] { false, true, false }
                };
            } else {
                return new bool[][] {
                    new bool[] { true, false, true },
                    new bool[] { false, true, false },
                    new bool[] { true, false, true }
                };
            }
        }

        public void RefreshEnabledVariantsDisplayList() {
            bool listShown = shouldDisplayEnabledVariantsOnScreen();

            IEnumerable<ExtendedVariantsModule.Variant> enabledVanillaVariants = ExtendedVariantsModule.Instance.VariantHandlers.Keys
                .Where(variant => ExtendedVariantsModule.Instance.VariantHandlers[variant] is AbstractVanillaVariant 
                        && (listShown || VariantsIndicator.WatermarkedVariants.Contains(variant))
                        && !isDefaultValue(variant));

            IEnumerable<ExtendedVariantsModule.Variant> enabledExtendedVariants = ExtendedVariantsModule.Instance.VariantHandlers.Keys
                .Where(variant => ExtendedVariantsModule.Instance.VariantHandlers[variant] is not AbstractVanillaVariant
                        && (listShown || VariantsIndicator.WatermarkedVariants.Contains(variant))
                        && !isDefaultValue(variant));

            variantsIndicator.Update(ExtendedVariantsModule.Settings.EnabledVariants.Keys.Concat(enabledVanillaVariants));

            if (!listShown) {
                mustUpdateDisplayedVariantList = true;
                return;
            }

            List<string> enabledVariantsToDisplay = new List<string>();

            foreach (ExtendedVariantsModule.Variant variant in enabledVanillaVariants) {
                if (variant == ExtendedVariantsModule.Variant.AirDashes) enabledVariantsToDisplay.Add($"{GetVanillaVariantLabel(variant)}: " + Dialog.Clean($"MENU_ASSIST_AIR_DASHES_{SaveData.Instance.Assists.DashMode.ToString()}"));
                else if (variant == ExtendedVariantsModule.Variant.VanillaGameSpeed) enabledVariantsToDisplay.Add($"{GetVanillaVariantLabel(variant)}: {SaveData.Instance.Assists.GameSpeed / 10f}x");
                // the rest are toggles: if enabled, display the name.
                else enabledVariantsToDisplay.Add(GetVanillaVariantLabel(variant));
            }

            foreach (ExtendedVariantsModule.Variant variant in enabledExtendedVariants) {
                string variantName = Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_{variant}");
                Type variantType = ExtendedVariantsModule.Instance.VariantHandlers[variant].GetVariantType();
                object variantValue = ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(variant);
                object defaultValue = ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue();

                if (variant == ExtendedVariantsModule.Variant.DashDirection) {
                    enabledVariantsToDisplay.Add($"{variantName}: {Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION_{ModOptionsEntries.GetDashDirectionIndex((bool[][]) variantValue)}")}");

                } else if (variant == ExtendedVariantsModule.Variant.JumpCount) {
                    string displayValue = ((int) variantValue) == int.MaxValue ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INFINITE") : ((int) variantValue).ToString();
                    enabledVariantsToDisplay.Add($"{variantName}: {displayValue}");

                } else if (variant == ExtendedVariantsModule.Variant.BadelineAttackPattern) {
                    enabledVariantsToDisplay.Add($"{variantName}: {Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_BADELINEPATTERN_{variantValue}")}");

                } else if (variant == ExtendedVariantsModule.Variant.DontRefillDashOnGround) {
                    string displayValue;
                    switch ((DontRefillDashOnGround.DashRefillOnGroundConfiguration) variantValue) {
                        case DontRefillDashOnGround.DashRefillOnGroundConfiguration.ON: displayValue = "OPTIONS_ON"; break;
                        case DontRefillDashOnGround.DashRefillOnGroundConfiguration.OFF: displayValue = "OPTIONS_OFF"; break;
                        default: displayValue = "MODOPTIONS_EXTENDEDVARIANTS_DEFAULT"; break;
                    }

                    enabledVariantsToDisplay.Add($"{variantName}: {Dialog.Clean(displayValue)}");

                } else if (variant == ExtendedVariantsModule.Variant.ColorGrading) {
                    string resourceName = variantValue.ToString();
                    if (resourceName.Contains("/")) resourceName = resourceName.Substring(resourceName.LastIndexOf("/") + 1);
                    string formattedValue = Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_CG_{resourceName}");
                    enabledVariantsToDisplay.Add($"{variantName}: {formattedValue}");

                } else if (new ExtendedVariantsModule.Variant[] { ExtendedVariantsModule.Variant.RoomLighting, ExtendedVariantsModule.Variant.BackgroundBrightness, ExtendedVariantsModule.Variant.ForegroundEffectOpacity,
                    ExtendedVariantsModule.Variant.GlitchEffect, ExtendedVariantsModule.Variant.AnxietyEffect, ExtendedVariantsModule.Variant.BlurLevel, ExtendedVariantsModule.Variant.BackgroundBlurLevel }
                    .Contains(variant)) {
                    // percentage variants
                    enabledVariantsToDisplay.Add($"{variantName}: {((float) variantValue) * 100}%");

                } else if (new ExtendedVariantsModule.Variant[] { ExtendedVariantsModule.Variant.BadelineLag, ExtendedVariantsModule.Variant.DelayBetweenBadelines,
                    ExtendedVariantsModule.Variant.SnowballDelay, ExtendedVariantsModule.Variant.RegularHiccups }.Contains(variant)) {
                    // time variants
                    enabledVariantsToDisplay.Add($"{variantName}: {variantValue}s");

                } else if (variant == ExtendedVariantsModule.Variant.CornerCorrection) {
                    enabledVariantsToDisplay.Add($"{variantName}: {variantValue}px");

                } else if (variantType == typeof(bool)) {
                    if ((bool) defaultValue) {
                        enabledVariantsToDisplay.Add($"{variantName}: " + Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLED"));
                    } else {
                        enabledVariantsToDisplay.Add($"{variantName}");
                    }

                } else if (variantType == typeof(int)) {
                    enabledVariantsToDisplay.Add($"{variantName}: {variantValue}");

                } else if (variantType == typeof(float)) {
                    // multiplier variants
                    enabledVariantsToDisplay.Add($"{variantName}: {variantValue}x");

                } else if (variantType.IsEnum) {
                    // enum variants
                    enabledVariantsToDisplay.Add($"{variantName}: " + Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_{variant}_{variantValue}"));

                } else {
                    throw new NotImplementedException("Cannot display name of variant " + variant + "!");
                }
            }

            infoPanel.Update(enabledVariantsToDisplay);
        }

        public static string GetVanillaVariantLabel(ExtendedVariantsModule.Variant variant) {
            switch (variant) {
                case ExtendedVariantsModule.Variant.VanillaGameSpeed:
                    return Dialog.Clean("MENU_ASSIST_GAMESPEED");
                case ExtendedVariantsModule.Variant.Invincible:
                    return Dialog.Clean("MENU_ASSIST_INVINCIBLE");
                case ExtendedVariantsModule.Variant.AirDashes:
                    return Dialog.Clean("MENU_ASSIST_AIR_DASHES");
                case ExtendedVariantsModule.Variant.DashAssist:
                    return Dialog.Clean("MENU_ASSIST_DASH_ASSIST");
                case ExtendedVariantsModule.Variant.InfiniteStamina:
                    return Dialog.Clean("MENU_ASSIST_INFINITE_STAMINA");
                case ExtendedVariantsModule.Variant.MirrorMode:
                    return Dialog.Clean("MENU_VARIANT_MIRROR");
                case ExtendedVariantsModule.Variant.ThreeSixtyDashing:
                    return Dialog.Clean("MENU_VARIANT_360DASHING");
                case ExtendedVariantsModule.Variant.InvisibleMotion:
                    return Dialog.Clean("MENU_VARIANT_INVISMOTION");
                case ExtendedVariantsModule.Variant.PlayAsBadeline:
                    return Dialog.Clean("MENU_VARIANT_PLAYASBADELINE");
                case ExtendedVariantsModule.Variant.NoGrabbing:
                    return Dialog.Clean("MENU_VARIANT_NOGRABBING");
                case ExtendedVariantsModule.Variant.LowFriction:
                    return Dialog.Clean("MENU_VARIANT_LOWFRICTION");
                case ExtendedVariantsModule.Variant.SuperDashing:
                    return Dialog.Clean("MENU_VARIANT_SUPERDASHING");
                case ExtendedVariantsModule.Variant.Hiccups:
                    return Dialog.Clean("MENU_VARIANT_HICCUPS");
                default:
                    throw new Exception("No vanilla variant label for " + variant + "!");
            }
        }
    }
}
