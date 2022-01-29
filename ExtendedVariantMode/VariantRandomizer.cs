using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using ExtendedVariants.UI;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedVariants {
    public class VanillaVariant {
        public string Name { get; private set; }
        public string Label { get; private set; }

        private VanillaVariant(string name, string label) {
            Name = name;
            Label = label;
        }

        public static bool operator ==(VanillaVariant one, VanillaVariant other) {
            return one.Name == other.Name;
        }

        public static bool operator !=(VanillaVariant one, VanillaVariant other) {
            return one.Name != other.Name;
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj != null && obj.GetType() == typeof(VanillaVariant) && (obj as VanillaVariant).Name == Name;
        }

        public static readonly VanillaVariant GameSpeed = new VanillaVariant("GameSpeed", "MENU_ASSIST_GAMESPEED");
        public static readonly VanillaVariant MirrorMode = new VanillaVariant("MirrorMode", "MENU_VARIANT_MIRROR");
        public static readonly VanillaVariant ThreeSixtyDashing = new VanillaVariant("ThreeSixtyDashing", "MENU_VARIANT_360DASHING");
        public static readonly VanillaVariant InvisibleMotion = new VanillaVariant("InvisibleMotion", "MENU_VARIANT_INVISMOTION");
        public static readonly VanillaVariant NoGrabbing = new VanillaVariant("NoGrabbing", "MENU_VARIANT_NOGRABBING");
        public static readonly VanillaVariant LowFriction = new VanillaVariant("LowFriction", "MENU_VARIANT_LOWFRICTION");
        public static readonly VanillaVariant SuperDashing = new VanillaVariant("SuperDashing", "MENU_VARIANT_SUPERDASHING");
        public static readonly VanillaVariant Hiccups = new VanillaVariant("Hiccups", "MENU_VARIANT_HICCUPS");
        public static readonly VanillaVariant PlayAsBadeline = new VanillaVariant("PlayAsBadeline", "MENU_VARIANT_PLAYASBADELINE");
        public static readonly VanillaVariant InfiniteStamina = new VanillaVariant("InfiniteStamina", "MENU_ASSIST_INFINITE_STAMINA");
        public static readonly VanillaVariant DashMode = new VanillaVariant("DashMode", "MENU_ASSIST_AIR_DASHES");
        public static readonly VanillaVariant Invincible = new VanillaVariant("Invincible", "MENU_ASSIST_INVINCIBLE");
        public static readonly VanillaVariant DashAssist = new VanillaVariant("DashAssist", "MENU_ASSIST_DASH_ASSIST");
    }

    public class VariantRandomizer {
        private Random randomGenerator = new Random();

        private float variantChangeTimer = -1f;
        private float vanillafyTimer = -1f;

        private InfoPanel infoPanel = new InfoPanel();

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

            if ((
                    (ExtendedVariantsModule.Settings.ChangeVariantsRandomly && ExtendedVariantsModule.Settings.DisplayEnabledVariantsToScreen) ||
                    (ExtendedVariantsModule.Session != null && ExtendedVariantsModule.Session.ExtendedVariantsDisplayedOnScreenViaTrigger)
                )
                && !((scene as Level)?.Paused ?? false)) {

                Draw.SpriteBatch.Begin();
                infoPanel.Render();
                Draw.SpriteBatch.End();
            }
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
                    variantChangeTimer -= Engine.DeltaTime;
                    if (variantChangeTimer <= 0f) {
                        // variant timer is over => change variant now!
                        changeVariantNow();

                        variantChangeTimer = ExtendedVariantsModule.Settings.ChangeVariantsInterval;
                        Logger.Log("ExtendedVariantMode/VariantRandomizer", $"variantChangeTimer reset to {variantChangeTimer}");
                    }
                }


                if (ExtendedVariantsModule.Settings.Vanillafy != 0) {
                    vanillafyTimer -= Engine.DeltaTime;
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

        private static readonly IEnumerable<VanillaVariant> allVanillaVariants = new List<VanillaVariant>() {
            VanillaVariant.GameSpeed, VanillaVariant.MirrorMode, VanillaVariant.ThreeSixtyDashing, VanillaVariant.InvisibleMotion, VanillaVariant.NoGrabbing,
            VanillaVariant.LowFriction, VanillaVariant.SuperDashing, VanillaVariant.Hiccups, VanillaVariant.PlayAsBadeline, VanillaVariant.InfiniteStamina,
            VanillaVariant.DashMode, VanillaVariant.Invincible, VanillaVariant.DashAssist
        };

        private void changeVariantNow(bool disableOnly = false) {
            // get filtered lists for changeable variants, and those which are enabled
            IEnumerable<VanillaVariant> changeableVanillaVariants = new List<VanillaVariant>();
            if (SaveData.Instance.VariantMode && ExtendedVariantsModule.Settings.VariantSet % 2 == 1)
                changeableVanillaVariants = allVanillaVariants.Where(variant => ExtendedVariantsModule.Settings.RandomizerEnabledVariants.TryGetValue(variant.Name, out bool enabled) ? enabled : true);

            IEnumerable<ExtendedVariantsModule.Variant> changeableExtendedVariants = new List<ExtendedVariantsModule.Variant>();
            if (ExtendedVariantsModule.Settings.VariantSet / 2 == 1)
                changeableExtendedVariants = ExtendedVariantsModule.Instance.VariantHandlers.Keys
                    .Where(variant => ExtendedVariantsModule.Settings.RandomizerEnabledVariants.TryGetValue(variant.ToString(), out bool enabled) ? enabled : true);

            IEnumerable<VanillaVariant> enabledVanillaVariants = changeableVanillaVariants.Where(variant => !isDefaultValue(variant));
            IEnumerable<ExtendedVariantsModule.Variant> enabledExtendedVariants = changeableExtendedVariants.Where(variant => !isDefaultValue(variant));

            if (!disableOnly && ExtendedVariantsModule.Settings.RerollMode) {
                Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Rerolling: disabling all variants");
                foreach (VanillaVariant variant in enabledVanillaVariants) disableVariant(variant);
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
                foreach (VanillaVariant variant in changeableVanillaVariants)
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
                    foreach (VanillaVariant variant in enabledVanillaVariants)
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
                    foreach (VanillaVariant variant in changeableVanillaVariants)
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

        private bool isDefaultValue(VanillaVariant variant) {
            if (variant == VanillaVariant.DashMode) return SaveData.Instance.Assists.DashMode == Assists.DashModes.Normal;
            if (variant == VanillaVariant.GameSpeed) return SaveData.Instance.Assists.GameSpeed == 10;
            if (variant == VanillaVariant.Hiccups) return !SaveData.Instance.Assists.Hiccups;
            if (variant == VanillaVariant.PlayAsBadeline) return !SaveData.Instance.Assists.PlayAsBadeline;
            if (variant == VanillaVariant.InfiniteStamina) return !SaveData.Instance.Assists.InfiniteStamina;
            if (variant == VanillaVariant.Invincible) return !SaveData.Instance.Assists.Invincible;
            if (variant == VanillaVariant.InvisibleMotion) return !SaveData.Instance.Assists.InvisibleMotion;
            if (variant == VanillaVariant.LowFriction) return !SaveData.Instance.Assists.LowFriction;
            if (variant == VanillaVariant.MirrorMode) return !SaveData.Instance.Assists.MirrorMode;
            if (variant == VanillaVariant.NoGrabbing) return !SaveData.Instance.Assists.NoGrabbing;
            if (variant == VanillaVariant.SuperDashing) return !SaveData.Instance.Assists.SuperDashing;
            if (variant == VanillaVariant.ThreeSixtyDashing) return !SaveData.Instance.Assists.ThreeSixtyDashing;
            if (variant == VanillaVariant.DashAssist) return !SaveData.Instance.Assists.DashAssist;

            Logger.Log(LogLevel.Error, "ExtendedVariantMode/VariantRandomizer", $"Requesting default value check for non-existent vanilla variant {variant.Name}");
            return false;
        }

        private bool isDefaultValue(ExtendedVariantsModule.Variant variant) {
            // Equals() doesn't work on 2D boolean arrays, surprisingly
            if (variant == ExtendedVariantsModule.Variant.DashDirection) return ModOptionsEntries.GetDashDirectionIndex() == 0;

            AbstractExtendedVariant variantHandler = ExtendedVariantsModule.Instance.VariantHandlers[variant];
            return variantHandler.GetVariantValue().Equals(variantHandler.GetDefaultVariantValue());
        }

        private void disableVariant(VanillaVariant variant) {
            Logger.Log(LogLevel.Info, "ExtendedVariantMode/VariantRandomizer", $"Disabling variant {variant.Name}");

            if (variant == VanillaVariant.DashMode) SaveData.Instance.Assists.DashMode = Assists.DashModes.Normal;
            else if (variant == VanillaVariant.GameSpeed) SaveData.Instance.Assists.GameSpeed = 10;
            else toggleVanillaVariant(variant, false);
        }

        private void disableVariant(ExtendedVariantsModule.Variant variant) {
            Logger.Log(LogLevel.Info, "ExtendedVariantMode/VariantRandomizer", $"Disabling variant {variant.ToString()}");

            AbstractExtendedVariant variantHandler = ExtendedVariantsModule.Instance.VariantHandlers[variant];
            variantHandler.SetVariantValue(variantHandler.GetDefaultVariantValue());
        }

        private void enableVariant(VanillaVariant variant) {
            Logger.Log(LogLevel.Info, "ExtendedVariantMode/VariantRandomizer", $"Enabling variant {variant.Name}");

            if (variant == VanillaVariant.DashMode) SaveData.Instance.Assists.DashMode = new Assists.DashModes[] { Assists.DashModes.Two, Assists.DashModes.Infinite }[randomGenerator.Next(2)];
            else if (variant == VanillaVariant.GameSpeed) SaveData.Instance.Assists.GameSpeed = new int[] { 5, 6, 7, 8, 9, 12, 12, 14, 14, 16 }[randomGenerator.Next(10)];
            else toggleVanillaVariant(variant, true);
        }

        private void enableVariant(ExtendedVariantsModule.Variant variant) {
            Logger.Log(LogLevel.Info, "ExtendedVariantMode/VariantRandomizer", $"Enabling variant {variant.ToString()}");

            AbstractExtendedVariant extendedVariant = ExtendedVariantsModule.Instance.VariantHandlers[variant];

            if (variant == ExtendedVariantsModule.Variant.DashDirection) {
                // random between "diagonals only" and "no diagonals"
                extendedVariant.SetVariantValue(getRandomDashDirection());

            } else if (variant == ExtendedVariantsModule.Variant.BadelineAttackPattern) {
                // random between a set of values
                int[] badelineBossesPatternsOptions = { 1, 2, 3, 4, 5, 9, 10, 14, 15 };
                extendedVariant.SetVariantValue(badelineBossesPatternsOptions[randomGenerator.Next(badelineBossesPatternsOptions.Length)]);

            } else if (variant == ExtendedVariantsModule.Variant.ColorGrading) {
                // random between all color grades shipping with extended variants
                extendedVariant.SetVariantValue(ColorGrading.ExistingColorGrades[randomGenerator.Next(ColorGrading.ExistingColorGrades.Count)]);

            } else if (variant == ExtendedVariantsModule.Variant.JellyfishEverywhere) {
                // random 1-3
                extendedVariant.SetVariantValue(randomGenerator.Next(3) + 1);

            } else if (variant == ExtendedVariantsModule.Variant.JumpCount) {
                // random 0-5
                extendedVariant.SetVariantValue(randomGenerator.Next(6));

            } else if (variant == ExtendedVariantsModule.Variant.Stamina) {
                // random 0-220 (so 0x to 2x the vanilla value)
                extendedVariant.SetVariantValue(randomGenerator.Next(220));

            } else if (variant == ExtendedVariantsModule.Variant.BoostMultiplier) {
                // same scale as other multiplier variants... except it can be negative as well!
                float[] multiplierScale = new float[] {
                    0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2, 2.5f, 3
                };
                float result = multiplierScale[randomGenerator.Next(multiplierScale.Length)];
                if (randomGenerator.Next() > 0.5) result *= -1;

                extendedVariant.SetVariantValue(result);

            } else if (variant == ExtendedVariantsModule.Variant.GameSpeed) {
                // same scale as other multiplier variants, but without 0 for obvious reasons.
                float[] multiplierScale = new float[] {
                    0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2, 2.5f, 3
                };
                extendedVariant.SetVariantValue(multiplierScale[randomGenerator.Next(multiplierScale.Length)]);

            } else if (new ExtendedVariantsModule.Variant[] { ExtendedVariantsModule.Variant.RoomLighting, ExtendedVariantsModule.Variant.BackgroundBrightness, ExtendedVariantsModule.Variant.ForegroundEffectOpacity,
                ExtendedVariantsModule.Variant.GlitchEffect, ExtendedVariantsModule.Variant.AnxietyEffect, ExtendedVariantsModule.Variant.BlurLevel, ExtendedVariantsModule.Variant.BackgroundBlurLevel }
                .Contains(variant)) {
                // percentage variants: 0-100% by steps of 10%
                extendedVariant.SetVariantValue(randomGenerator.Next(11) / 10f);

            } else if (extendedVariant.GetVariantType() == typeof(bool)) {
                // to toggle the value, well... just toggle it. Easy!
                extendedVariant.SetVariantValue(!((bool) extendedVariant.GetDefaultVariantValue()));

            } else if (extendedVariant.GetVariantType() == typeof(int)) {
                // 1-5 is good for most variants.
                extendedVariant.SetVariantValue(randomGenerator.Next(5) + 1);

            } else if (extendedVariant.GetVariantType() == typeof(float)) {
                // this is for multiplier variants!
                float[] multiplierScale = new float[] {
                    0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2, 2.5f, 3
                };

                extendedVariant.SetVariantValue(multiplierScale[randomGenerator.Next(multiplierScale.Length)]);

            } else if (extendedVariant.GetVariantType().IsEnum) {
                // enum variants
                Array vals = Enum.GetValues(extendedVariant.GetVariantType());
                extendedVariant.SetVariantValue(vals.GetValue(randomGenerator.Next(vals.Length)));

            } else {
                throw new NotImplementedException("Cannot randomize variant " + variant + "!");
            }

            if (isDefaultValue(variant)) {
                // we randomly generated a default value so the variant isn't enabled - try again!
                enableVariant(variant);
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

        private void toggleVanillaVariant(VanillaVariant variant, bool enabled) {
            if (variant == VanillaVariant.Hiccups) SaveData.Instance.Assists.Hiccups = enabled;
            else if (variant == VanillaVariant.InfiniteStamina) SaveData.Instance.Assists.InfiniteStamina = enabled;
            else if (variant == VanillaVariant.Invincible) SaveData.Instance.Assists.Invincible = enabled;
            else if (variant == VanillaVariant.InvisibleMotion) SaveData.Instance.Assists.InvisibleMotion = enabled;
            else if (variant == VanillaVariant.LowFriction) SaveData.Instance.Assists.LowFriction = enabled;
            else if (variant == VanillaVariant.MirrorMode) {
                SaveData.Instance.Assists.MirrorMode = enabled;
                Input.Aim.InvertedX = enabled;
                Input.MoveX.Inverted = enabled;
                Input.Feather.InvertedX = enabled;
            } else if (variant == VanillaVariant.NoGrabbing) SaveData.Instance.Assists.NoGrabbing = enabled;
            else if (variant == VanillaVariant.SuperDashing) SaveData.Instance.Assists.SuperDashing = enabled;
            else if (variant == VanillaVariant.ThreeSixtyDashing) SaveData.Instance.Assists.ThreeSixtyDashing = enabled;
            else if (variant == VanillaVariant.PlayAsBadeline) {
                SaveData.Instance.Assists.PlayAsBadeline = enabled;
                Player entity = Engine.Scene.Tracker.GetEntity<Player>();
                if (entity != null) {
                    PlayerSpriteMode mode = SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : entity.DefaultSpriteMode;
                    if (entity.Active) {
                        entity.ResetSpriteNextFrame(mode);
                        return;
                    }
                    entity.ResetSprite(mode);
                }
            } else if (variant == VanillaVariant.DashAssist) SaveData.Instance.Assists.DashAssist = enabled;
        }

        public void RefreshEnabledVariantsDisplayList() {
            List<string> enabledVariantsToDisplay = new List<string>();

            IEnumerable<VanillaVariant> enabledVanillaVariants = allVanillaVariants.Where(variant => !isDefaultValue(variant));
            IEnumerable<ExtendedVariantsModule.Variant> enabledExtendedVariants = ExtendedVariantsModule.Instance.VariantHandlers.Keys.Where(variant => !isDefaultValue(variant));

            foreach (VanillaVariant variant in enabledVanillaVariants) {
                if (variant == VanillaVariant.DashMode) enabledVariantsToDisplay.Add($"{Dialog.Clean(variant.Label)}: " + Dialog.Clean($"MENU_ASSIST_AIR_DASHES_{SaveData.Instance.Assists.DashMode.ToString()}"));
                else if (variant == VanillaVariant.GameSpeed) enabledVariantsToDisplay.Add($"{Dialog.Clean(variant.Label)}: {SaveData.Instance.Assists.GameSpeed / 10f}x");
                // the rest are toggles: if enabled, display the name.
                else enabledVariantsToDisplay.Add(Dialog.Clean(variant.Label));
            }

            foreach (ExtendedVariantsModule.Variant variant in enabledExtendedVariants) {
                string variantName = Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_{variant}");
                Type variantType = ExtendedVariantsModule.Instance.VariantHandlers[variant].GetVariantType();
                object variantValue = ExtendedVariantsModule.Instance.VariantHandlers[variant].GetVariantValue();
                object defaultValue = ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue();

                if (variant == ExtendedVariantsModule.Variant.DashDirection) {
                    enabledVariantsToDisplay.Add($"{variantName}: {Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_DASHDIRECTION_{ModOptionsEntries.GetDashDirectionIndex()}")}");

                } else if (variant == ExtendedVariantsModule.Variant.JumpCount) {
                    string displayValue = ExtendedVariantsModule.Settings.JumpCount == int.MaxValue ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INFINITE") : ExtendedVariantsModule.Settings.JumpCount.ToString();
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
    }
}
