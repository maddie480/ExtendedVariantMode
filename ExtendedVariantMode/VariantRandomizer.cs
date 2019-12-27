using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtendedVariants {
    public class VanillaVariant {
        public string Name { get; private set; }
        public string Label { get; private set; }

        private VanillaVariant(string name, string label) {
            Name = name;
            Label = label;
        }

        public static bool operator == (VanillaVariant one, VanillaVariant other) {
            return one.Name == other.Name;
        }

        public static bool operator != (VanillaVariant one, VanillaVariant other) {
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

        /// <summary>
        /// List of options shown for multipliers.
        /// </summary>
        private static int[] multiplierScale = new int[] {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100, 250, 500, 1000
        };

        public void Load() {
            Everest.Events.Level.OnEnter += onLevelEnter;
            On.Celeste.Level.TransitionRoutine += onRoomChange;
            On.Celeste.Player.Update += onUpdate;
            Everest.Events.Level.OnExit += clearFile;
            On.Celeste.Level.EndPauseEffects += modEndPauseEffects;

            if(Engine.Scene != null && Engine.Scene.GetType() == typeof(Level)) {
                // we're late, catch up!
                onLevelEnter();
            }
        }

        public void Unload() {
            Everest.Events.Level.OnEnter -= onLevelEnter;
            On.Celeste.Level.TransitionRoutine -= onRoomChange;
            On.Celeste.Player.Update -= onUpdate;
            Everest.Events.Level.OnExit -= clearFile;
            On.Celeste.Level.EndPauseEffects -= modEndPauseEffects;

            // clear file right away, since Extended Variants are no more.
            clearFile();
        }

        private void onLevelEnter(Session session, bool fromSaveData) {
            onLevelEnter();
        }

        private void onLevelEnter() {
            UpdateCountersFromSettings();
            spitOutEnabledVariantsInFile();
        }

        private void modEndPauseEffects(On.Celeste.Level.orig_EndPauseEffects orig, Level self) {
            orig(self);

            // rewrite the file in case the player changed anything in the pause menu
            spitOutEnabledVariantsInFile();
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
            
            IEnumerator origEnum = orig(self, next, direction);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

            yield break;
        }

        private void onUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            if (ExtendedVariantsModule.Settings.ChangeVariantsRandomly) {
                if (ExtendedVariantsModule.Settings.ChangeVariantsInterval != 0) {
                    variantChangeTimer -= Engine.DeltaTime;
                    if(variantChangeTimer <= 0f) {
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
            if(SaveData.Instance.VariantMode && ExtendedVariantsModule.Settings.VariantSet % 2 == 1)
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
                foreach (ExtendedVariantsModule.Variant variant in enabledExtendedVariants) disableVariant(variant);

                Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Rerolling: enabling {ExtendedVariantsModule.Settings.MaxEnabledVariants} variants");

                // give numbers to all variants
                List<int> variantNumbers = new List<int>();
                for (int i = 0; i < changeableVanillaVariants.Count() + changeableExtendedVariants.Count(); i++) variantNumbers.Add(i);

                // remove numbers until there are few enough left
                while(variantNumbers.Count() > ExtendedVariantsModule.Settings.MaxEnabledVariants) {
                    variantNumbers.RemoveAt(randomGenerator.Next(variantNumbers.Count()));
                }

                // and enable those specific variants
                int index = 0;
                foreach (VanillaVariant variant in changeableVanillaVariants)
                    if (variantNumbers.Contains(index++)) enableVariant(variant);
                foreach (ExtendedVariantsModule.Variant variant in changeableExtendedVariants)
                    if (variantNumbers.Contains(index++)) enableVariant(variant);
            } else {
                // pick a random variant (if disableOnly or the max variant count has been reached, pick from the enabled ones)
                if(disableOnly || (enabledVanillaVariants.Count() + enabledExtendedVariants.Count() >= ExtendedVariantsModule.Settings.MaxEnabledVariants)) {
                    Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Randomizing: picking a variant in disableOnly mode " +
                        $"({enabledVanillaVariants.Count() + enabledExtendedVariants.Count()} enabled, {ExtendedVariantsModule.Settings.MaxEnabledVariants} max)");

                    if(enabledVanillaVariants.Count() + enabledExtendedVariants.Count() == 0) {
                        Logger.Log(LogLevel.Warn, "ExtendedVariantMode/VariantRandomizer", "ESCAPE: We are in disableOnly mode and no variant is enabled.");
                        return;
                    }

                    // pick a random variant from the enabled ones, and disable it
                    int drawnVariant = randomGenerator.Next(enabledVanillaVariants.Count() + enabledExtendedVariants.Count());
                    foreach (VanillaVariant variant in enabledVanillaVariants)
                        if (drawnVariant-- == 0) disableVariant(variant);
                    foreach (ExtendedVariantsModule.Variant variant in enabledExtendedVariants)
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
                    foreach (ExtendedVariantsModule.Variant variant in changeableExtendedVariants)
                        if (drawnVariant-- == 0) {
                            if (isDefaultValue(variant)) enableVariant(variant);
                            else disableVariant(variant);
                        }
                }
            }

            spitOutEnabledVariantsInFile();
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
            AbstractExtendedVariant variantHandler = ExtendedVariantsModule.Instance.VariantHandlers[variant];
            return variantHandler.GetValue() == variantHandler.GetDefaultValue();
        }

        private void disableVariant(VanillaVariant variant) {
            Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Disabling variant {variant.Name}");

            if (variant == VanillaVariant.DashMode) SaveData.Instance.Assists.DashMode = Assists.DashModes.Normal;
            else if (variant == VanillaVariant.GameSpeed) SaveData.Instance.Assists.GameSpeed = 10;
            else toggleVanillaVariant(variant, false);
        }

        private void disableVariant(ExtendedVariantsModule.Variant variant) {
            Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Disabling variant {variant.ToString()}");

            AbstractExtendedVariant variantHandler = ExtendedVariantsModule.Instance.VariantHandlers[variant];
            variantHandler.SetValue(variantHandler.GetDefaultValue());
        }

        private void enableVariant(VanillaVariant variant) {
            Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Enabling variant {variant.Name}");

            if (variant == VanillaVariant.DashMode) SaveData.Instance.Assists.DashMode = new Assists.DashModes[] {Assists.DashModes.Two, Assists.DashModes.Infinite }[randomGenerator.Next(2)];
            else if (variant == VanillaVariant.GameSpeed) SaveData.Instance.Assists.GameSpeed = new int[] { 5, 6, 7, 8, 9, 12, 12, 14, 14, 16 }[randomGenerator.Next(10)];
            else toggleVanillaVariant(variant, true);
        }

        private void enableVariant(ExtendedVariantsModule.Variant variant) {
            Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Enabling variant {variant.ToString()}");

            // toggle-style variants (enable)
            if (variant == ExtendedVariantsModule.Variant.DisableWallJumping) ExtendedVariantsModule.Settings.DisableWallJumping = true;
            else if (variant == ExtendedVariantsModule.Variant.DisableClimbJumping) ExtendedVariantsModule.Settings.DisableClimbJumping = true;
            else if (variant == ExtendedVariantsModule.Variant.UpsideDown) ExtendedVariantsModule.Settings.UpsideDown = true;
            else if (variant == ExtendedVariantsModule.Variant.ForceDuckOnGround) ExtendedVariantsModule.Settings.ForceDuckOnGround = true;
            else if (variant == ExtendedVariantsModule.Variant.InvertDashes) ExtendedVariantsModule.Settings.InvertDashes = true;
            else if (variant == ExtendedVariantsModule.Variant.InvertGrab) ExtendedVariantsModule.Settings.InvertGrab = true;
            else if (variant == ExtendedVariantsModule.Variant.DisableNeutralJumping) ExtendedVariantsModule.Settings.DisableNeutralJumping = true;
            else if (variant == ExtendedVariantsModule.Variant.BadelineChasersEverywhere) ExtendedVariantsModule.Settings.BadelineChasersEverywhere = true;
            else if (variant == ExtendedVariantsModule.Variant.SnowballsEverywhere) ExtendedVariantsModule.Settings.SnowballsEverywhere = true;
            else if (variant == ExtendedVariantsModule.Variant.TheoCrystalsEverywhere) ExtendedVariantsModule.Settings.TheoCrystalsEverywhere = true;
            else if (variant == ExtendedVariantsModule.Variant.HeldDash) ExtendedVariantsModule.Settings.HeldDash = true;
            else if (variant == ExtendedVariantsModule.Variant.AllStrawberriesAreGoldens) ExtendedVariantsModule.Settings.AllStrawberriesAreGoldens = true;
            else if (variant == ExtendedVariantsModule.Variant.DontRefillDashOnGround) ExtendedVariantsModule.Settings.DontRefillDashOnGround = true;
            else if (variant == ExtendedVariantsModule.Variant.EverythingIsUnderwater) ExtendedVariantsModule.Settings.EverythingIsUnderwater = true;
            else if (variant == ExtendedVariantsModule.Variant.BadelineBossesEverywhere) ExtendedVariantsModule.Settings.BadelineBossesEverywhere = true;
            else if (variant == ExtendedVariantsModule.Variant.OshiroEverywhere) ExtendedVariantsModule.Settings.OshiroEverywhere = true;
            else if (variant == ExtendedVariantsModule.Variant.JellyfishEverywhere) ExtendedVariantsModule.Settings.JellyfishEverywhere = 1; // 1 jellyfish
            // multiplier-style variants (random 0~3x)
            else if (variant == ExtendedVariantsModule.Variant.Gravity) ExtendedVariantsModule.Settings.Gravity = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.FallSpeed) ExtendedVariantsModule.Settings.FallSpeed = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.JumpHeight) ExtendedVariantsModule.Settings.JumpHeight = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.DashSpeed) ExtendedVariantsModule.Settings.DashSpeed = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.DashLength) ExtendedVariantsModule.Settings.DashLength = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.HyperdashSpeed) ExtendedVariantsModule.Settings.HyperdashSpeed = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.ExplodeLaunchSpeed) ExtendedVariantsModule.Settings.ExplodeLaunchSpeed = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.WallBouncingSpeed) ExtendedVariantsModule.Settings.WallBouncingSpeed = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.SpeedX) ExtendedVariantsModule.Settings.SpeedX = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.Friction) ExtendedVariantsModule.Settings.Friction = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.AirFriction) ExtendedVariantsModule.Settings.AirFriction = multiplierScale[randomGenerator.Next(23)];
            else if (variant == ExtendedVariantsModule.Variant.GameSpeed) ExtendedVariantsModule.Settings.GameSpeed = multiplierScale[randomGenerator.Next(22) + 1]; // don't set game speed to 0x for obvious reasons
            // more specific variants
            else if (variant == ExtendedVariantsModule.Variant.JumpCount) ExtendedVariantsModule.Settings.JumpCount = randomGenerator.Next(7); // random 0~infinite
            else if (variant == ExtendedVariantsModule.Variant.DashCount) ExtendedVariantsModule.Settings.DashCount = randomGenerator.Next(6); // random 0~5
            else if (variant == ExtendedVariantsModule.Variant.Stamina) ExtendedVariantsModule.Settings.Stamina = randomGenerator.Next(23); // random 0~220
            else if (variant == ExtendedVariantsModule.Variant.RegularHiccups) ExtendedVariantsModule.Settings.RegularHiccups = multiplierScale[randomGenerator.Next(13) + 10]; // random 1~3 seconds
            else if (variant == ExtendedVariantsModule.Variant.RoomLighting) ExtendedVariantsModule.Settings.RoomLighting = randomGenerator.Next(11); // random 0~100%
            else if (variant == ExtendedVariantsModule.Variant.RoomBloom) ExtendedVariantsModule.Settings.RoomBloom = randomGenerator.Next(11); // random 0~100%
            else if (variant == ExtendedVariantsModule.Variant.WindEverywhere) ExtendedVariantsModule.Settings.WindEverywhere = 13; // 13 is the random setting
            else if (variant == ExtendedVariantsModule.Variant.AddSeekers) ExtendedVariantsModule.Settings.AddSeekers = randomGenerator.Next(3) + 1; // random 1~3 seekers
            else if (variant == ExtendedVariantsModule.Variant.ColorGrading) ExtendedVariantsModule.Settings.ColorGrading = randomGenerator.Next(ColorGrading.ExistingColorGrades.Count); // random color grade
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
            }
            else if (variant == VanillaVariant.NoGrabbing) SaveData.Instance.Assists.NoGrabbing = enabled;
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
            }
            else if (variant == VanillaVariant.DashAssist) SaveData.Instance.Assists.DashAssist = enabled;
        }

        private void spitOutEnabledVariantsInFile() {
            if(ExtendedVariantsModule.Settings.FileOutput) {
                Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Writing enabled variants to enabled-variants.txt");

                using (Stream fileStream = new FileStream("enabled-variants.txt", File.Exists("enabled-variants.txt") ? FileMode.Truncate : FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                using (StreamWriter fileWriter = new StreamWriter(fileStream, new UTF8Encoding())) {
                    IEnumerable<VanillaVariant> enabledVanillaVariants = allVanillaVariants.Where(variant => !isDefaultValue(variant));
                    IEnumerable<ExtendedVariantsModule.Variant> enabledExtendedVariants = ExtendedVariantsModule.Instance.VariantHandlers.Keys.Where(variant => !isDefaultValue(variant));

                    foreach(VanillaVariant variant in enabledVanillaVariants) {
                        if (variant == VanillaVariant.DashMode) fileWriter.WriteLine($"{Dialog.Clean(variant.Label)}: " + Dialog.Clean($"MENU_ASSIST_AIR_DASHES_{SaveData.Instance.Assists.DashMode.ToString()}"));
                        else if (variant == VanillaVariant.GameSpeed) fileWriter.WriteLine($"{Dialog.Clean(variant.Label)}: {multiplierFormatter(SaveData.Instance.Assists.GameSpeed)}");
                        // the rest are toggles: if enabled, display the name.
                        else fileWriter.WriteLine(Dialog.Clean(variant.Label));
                    }

                    foreach(ExtendedVariantsModule.Variant variant in enabledExtendedVariants) {
                        string variantName = Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_{variant.ToString().ToUpperInvariant()}");

                        // "just print the raw value" variants
                        if (variant == ExtendedVariantsModule.Variant.DashCount || variant == ExtendedVariantsModule.Variant.AddSeekers || variant == ExtendedVariantsModule.Variant.JellyfishEverywhere)
                            fileWriter.WriteLine($"{variantName}: {ExtendedVariantsModule.Instance.VariantHandlers[variant].GetValue()}");
                        // variants that require a bit more formatting
                        else if (variant == ExtendedVariantsModule.Variant.Stamina) fileWriter.WriteLine($"{variantName}: {ExtendedVariantsModule.Settings.Stamina * 10}");
                        else if (variant == ExtendedVariantsModule.Variant.Friction) {
                            if(ExtendedVariantsModule.Settings.Friction == 0) fileWriter.WriteLine($"{variantName}: 0.05x");
                            else if (ExtendedVariantsModule.Settings.Friction == -1) fileWriter.WriteLine($"{variantName}: 0x");
                            else fileWriter.WriteLine($"{variantName}: {multiplierFormatter(ExtendedVariantsModule.Settings.Friction)}");
                        }
                        else if (variant == ExtendedVariantsModule.Variant.AirFriction) {
                            if(ExtendedVariantsModule.Settings.AirFriction == 0) fileWriter.WriteLine($"{variantName}: 0.05x");
                            else if (ExtendedVariantsModule.Settings.AirFriction == -1) fileWriter.WriteLine($"{variantName}: 0x");
                            else fileWriter.WriteLine($"{variantName}: {multiplierFormatter(ExtendedVariantsModule.Settings.AirFriction)}");
                        }
                        else if (variant == ExtendedVariantsModule.Variant.JumpCount)
                            fileWriter.WriteLine($"{variantName}: {(ExtendedVariantsModule.Settings.JumpCount == 6 ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INFINITE") : ExtendedVariantsModule.Settings.JumpCount.ToString())}");
                        else if (variant == ExtendedVariantsModule.Variant.Stamina) fileWriter.WriteLine($"{variantName}: {ExtendedVariantsModule.Settings.Stamina * 10}");
                        else if (variant == ExtendedVariantsModule.Variant.RegularHiccups) fileWriter.WriteLine($"{variantName}: {multiplierFormatter(ExtendedVariantsModule.Settings.RegularHiccups).Replace("x", "s")}");
                        else if (variant == ExtendedVariantsModule.Variant.RoomLighting) fileWriter.WriteLine($"{variantName}: {ExtendedVariantsModule.Settings.RoomLighting * 10}%");
                        else if (variant == ExtendedVariantsModule.Variant.RoomBloom) fileWriter.WriteLine($"{variantName}: {ExtendedVariantsModule.Settings.RoomBloom * 10}%");
                        else if (variant == ExtendedVariantsModule.Variant.ColorGrading) {
                            string resourceName = ColorGrading.ExistingColorGrades[ExtendedVariantsModule.Settings.ColorGrading];
                            if(resourceName.Contains("/")) resourceName = resourceName.Substring(resourceName.LastIndexOf("/") + 1);
                            string formattedValue =  Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_CG_{resourceName}");

                            fileWriter.WriteLine($"{variantName}: {formattedValue}");
                        }
                        // multiplier-style variants
                        else if ((ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultValue() == 10))
                            fileWriter.WriteLine($"{variantName}: {multiplierFormatter(ExtendedVariantsModule.Instance.VariantHandlers[variant].GetValue())}");
                        // toggle-style variants: print out the name
                        else fileWriter.WriteLine(variantName);
                    }
                }
            }
        }

        private void clearFile(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            clearFile();
        }

        private void clearFile() {
            if(ExtendedVariantsModule.Settings.FileOutput) {
                Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Clearing enabled-variants.txt");

                using (Stream fileStream = new FileStream("enabled-variants.txt", File.Exists("enabled-variants.txt") ? FileMode.Truncate : FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                using (StreamWriter fileWriter = new StreamWriter(fileStream, new UTF8Encoding())) {
                }
            }
        }

        private string multiplierFormatter(int multiplier) {
            if (multiplier % 10 == 0) {
                return $"{multiplier / 10f:n0}x";
            }
            return $"{multiplier / 10f:n1}x";
        }
    }
}
