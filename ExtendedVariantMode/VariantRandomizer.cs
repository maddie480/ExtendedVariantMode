using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Celeste.Mod.ExtendedVariants {
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
        public static readonly VanillaVariant InfiniteStamina = new VanillaVariant("InfiniteStamina", "MENU_ASSIST_INFINITE_STAMINA");
        public static readonly VanillaVariant DashMode = new VanillaVariant("DashMode", "MENU_ASSIST_AIR_DASHES");
        public static readonly VanillaVariant Invincible = new VanillaVariant("Invincible", "MENU_ASSIST_INVINCIBLE");
    }

    public class VariantRandomizer {
        private Random randomGenerator = new Random();

        private float variantChangeTimer = -1f;
        private float vanillafyTimer = -1f;

        public void UpdateCountersFromSettings() {
            variantChangeTimer = ExtendedVariantsModule.Settings.ChangeVariantsInterval;
            vanillafyTimer = ExtendedVariantsModule.Settings.Vanillafy;

            Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Updated variables from settings: variantChangeTimer = {variantChangeTimer}, vanillafyTimer = {vanillafyTimer}");
        }

        public void OnRoomChange() {
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
        }

        public void OnUpdate() {
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
        }

        private static readonly IEnumerable<VanillaVariant> allVanillaVariants = new List<VanillaVariant>() {
            VanillaVariant.GameSpeed, VanillaVariant.MirrorMode, VanillaVariant.ThreeSixtyDashing, VanillaVariant.InvisibleMotion, VanillaVariant.NoGrabbing,
            VanillaVariant.LowFriction, VanillaVariant.SuperDashing, VanillaVariant.Hiccups, VanillaVariant.InfiniteStamina, VanillaVariant.DashMode, VanillaVariant.Invincible
        };

        private static readonly IEnumerable<Variant> allExtendedVariants = new List<Variant>() {
            Variant.Gravity, Variant.FallSpeed, Variant.JumpHeight, Variant.SpeedX, Variant.Stamina, Variant.DashSpeed, Variant.DashCount, Variant.Friction, Variant.DisableWallJumping,
            Variant.JumpCount, Variant.UpsideDown, Variant.HyperdashSpeed, Variant.WallBouncingSpeed, Variant.DashLength, Variant.ForceDuckOnGround, Variant.InvertDashes, Variant.DisableNeutralJumping,
            Variant.BadelineChasersEverywhere, Variant.RegularHiccups, Variant.RoomLighting, Variant.OshiroEverywhere, Variant.WindEverywhere, Variant.SnowballsEverywhere, Variant.AddSeekers
        };

        private void changeVariantNow(bool disableOnly = false) {
            // get filtered lists for changeable variants, and those which are enabled
            IEnumerable<VanillaVariant> changeableVanillaVariants = new List<VanillaVariant>();
            if(SaveData.Instance.VariantMode && ExtendedVariantsModule.Settings.VariantSet % 2 == 1)
                changeableVanillaVariants = allVanillaVariants.Where(variant => ExtendedVariantsModule.Settings.RandomizerEnabledVariants.TryGetValue(variant.Name, out bool enabled) ? enabled : true);

            IEnumerable<Variant> changeableExtendedVariants = new List<Variant>();
            if (ExtendedVariantsModule.Settings.VariantSet / 2 == 1)
                changeableExtendedVariants = allExtendedVariants.Where(variant => ExtendedVariantsModule.Settings.RandomizerEnabledVariants.TryGetValue(variant.ToString(), out bool enabled) ? enabled : true);

            IEnumerable<VanillaVariant> enabledVanillaVariants = changeableVanillaVariants.Where(variant => !isDefaultValue(variant));
            IEnumerable<Variant> enabledExtendedVariants = changeableExtendedVariants.Where(variant => !isDefaultValue(variant));

            if (!disableOnly && ExtendedVariantsModule.Settings.RerollMode) {
                Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Rerolling: disabling all variants");
                foreach (VanillaVariant variant in enabledVanillaVariants) disableVariant(variant);
                foreach (Variant variant in enabledExtendedVariants) disableVariant(variant);

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
                foreach (Variant variant in changeableExtendedVariants)
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
                    foreach (Variant variant in enabledExtendedVariants)
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
                    foreach (Variant variant in changeableExtendedVariants)
                        if (drawnVariant-- == 0) {
                            if (isDefaultValue(variant)) enableVariant(variant);
                            else disableVariant(variant);
                        }
                }
            }

            SpitOutEnabledVariantsInFile();
        }

        private bool isDefaultValue(VanillaVariant variant) {
            if (variant == VanillaVariant.DashMode) return SaveData.Instance.Assists.DashMode == Assists.DashModes.Normal;
            if (variant == VanillaVariant.GameSpeed) return SaveData.Instance.Assists.GameSpeed == 10;
            if (variant == VanillaVariant.Hiccups) return !SaveData.Instance.Assists.Hiccups;
            if (variant == VanillaVariant.InfiniteStamina) return !SaveData.Instance.Assists.InfiniteStamina;
            if (variant == VanillaVariant.Invincible) return !SaveData.Instance.Assists.Invincible;
            if (variant == VanillaVariant.InvisibleMotion) return !SaveData.Instance.Assists.InvisibleMotion;
            if (variant == VanillaVariant.LowFriction) return !SaveData.Instance.Assists.LowFriction;
            if (variant == VanillaVariant.MirrorMode) return !SaveData.Instance.Assists.MirrorMode;
            if (variant == VanillaVariant.NoGrabbing) return !SaveData.Instance.Assists.NoGrabbing;
            if (variant == VanillaVariant.SuperDashing) return !SaveData.Instance.Assists.SuperDashing;
            if (variant == VanillaVariant.ThreeSixtyDashing) return !SaveData.Instance.Assists.ThreeSixtyDashing;

            Logger.Log(LogLevel.Error, "ExtendedVariantMode/VariantRandomizer", $"Requesting default value check for non-existent vanilla variant {variant.Name}");
            return false;
        }

        private bool isDefaultValue(Variant variant) {
            return ExtendedVariantTrigger.SetVariantValue(variant, ExtendedVariantTrigger.NO_CHANGE) == ExtendedVariantTrigger.GetDefaultValueForVariant(variant);
        }

        private void disableVariant(VanillaVariant variant) {
            Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Disabling variant {variant.Name}");

            if (variant == VanillaVariant.DashMode) SaveData.Instance.Assists.DashMode = Assists.DashModes.Normal;
            else if (variant == VanillaVariant.GameSpeed) SaveData.Instance.Assists.GameSpeed = 10;
            else toggleVanillaVariant(variant, false);
        }

        private void disableVariant(Variant variant) {
            Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Disabling variant {variant.ToString()}");

            ExtendedVariantTrigger.SetVariantValue(variant, ExtendedVariantTrigger.GetDefaultValueForVariant(variant));
        }

        private void enableVariant(VanillaVariant variant) {
            Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Enabling variant {variant.Name}");

            if (variant == VanillaVariant.DashMode) SaveData.Instance.Assists.DashMode = new Assists.DashModes[] {Assists.DashModes.Two, Assists.DashModes.Infinite }[randomGenerator.Next(2)];
            else if (variant == VanillaVariant.GameSpeed) SaveData.Instance.Assists.GameSpeed = new int[] { 5, 6, 7, 8, 9, 12, 12, 14, 14, 16 }[randomGenerator.Next(10)];
            else toggleVanillaVariant(variant, true);
        }

        private void enableVariant(Variant variant) {
            Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Enabling variant {variant.ToString()}");
            
            // toggle-style variants (enable)
            if (variant == Variant.DisableWallJumping) ExtendedVariantsModule.Settings.DisableWallJumping = true;
            else if (variant == Variant.UpsideDown) ExtendedVariantsModule.Settings.UpsideDown = true;
            else if (variant == Variant.ForceDuckOnGround) ExtendedVariantsModule.Settings.ForceDuckOnGround = true;
            else if (variant == Variant.InvertDashes) ExtendedVariantsModule.Settings.InvertDashes = true;
            else if (variant == Variant.DisableNeutralJumping) ExtendedVariantsModule.Settings.DisableNeutralJumping = true;
            else if (variant == Variant.BadelineChasersEverywhere) ExtendedVariantsModule.Settings.BadelineChasersEverywhere = true;
            else if (variant == Variant.OshiroEverywhere) ExtendedVariantsModule.Settings.OshiroEverywhere = true;
            else if (variant == Variant.SnowballsEverywhere) ExtendedVariantsModule.Settings.SnowballsEverywhere = true;
            // multiplier-style variants (random 0~3x)
            else if (variant == Variant.Gravity) ExtendedVariantsModule.Settings.Gravity = ExtendedVariantsModule.MultiplierScale[randomGenerator.Next(23)];
            else if (variant == Variant.FallSpeed) ExtendedVariantsModule.Settings.FallSpeed = ExtendedVariantsModule.MultiplierScale[randomGenerator.Next(23)];
            else if (variant == Variant.JumpHeight) ExtendedVariantsModule.Settings.JumpHeight = ExtendedVariantsModule.MultiplierScale[randomGenerator.Next(23)];
            else if (variant == Variant.DashSpeed) ExtendedVariantsModule.Settings.DashSpeed = ExtendedVariantsModule.MultiplierScale[randomGenerator.Next(23)];
            else if (variant == Variant.DashLength) ExtendedVariantsModule.Settings.DashLength = ExtendedVariantsModule.MultiplierScale[randomGenerator.Next(23)];
            else if (variant == Variant.HyperdashSpeed) ExtendedVariantsModule.Settings.HyperdashSpeed = ExtendedVariantsModule.MultiplierScale[randomGenerator.Next(23)];
            else if (variant == Variant.WallBouncingSpeed) ExtendedVariantsModule.Settings.WallBouncingSpeed = ExtendedVariantsModule.MultiplierScale[randomGenerator.Next(23)];
            else if (variant == Variant.SpeedX) ExtendedVariantsModule.Settings.SpeedX = ExtendedVariantsModule.MultiplierScale[randomGenerator.Next(23)];
            else if (variant == Variant.Friction) ExtendedVariantsModule.Settings.Friction = ExtendedVariantsModule.MultiplierScale[randomGenerator.Next(23)];
            // more specific variants
            else if (variant == Variant.JumpCount) ExtendedVariantsModule.Settings.JumpCount = randomGenerator.Next(7); // random 0~infinite
            else if (variant == Variant.DashCount) ExtendedVariantsModule.Settings.DashCount = randomGenerator.Next(6); // random 0~5
            else if (variant == Variant.Stamina) ExtendedVariantsModule.Settings.Stamina = randomGenerator.Next(23); // random 0~220
            else if (variant == Variant.RegularHiccups) ExtendedVariantsModule.Settings.RegularHiccups = ExtendedVariantsModule.MultiplierScale[randomGenerator.Next(13) + 10]; // random 1~3 seconds
            else if (variant == Variant.RoomLighting) ExtendedVariantsModule.Settings.RoomLighting = randomGenerator.Next(11); // random 0~100%
            else if (variant == Variant.WindEverywhere) ExtendedVariantsModule.Settings.WindEverywhere = 6; // 6 is the random setting
            else if (variant == Variant.AddSeekers) ExtendedVariantsModule.Settings.AddSeekers = randomGenerator.Next(3) + 1; // random 1~3 seekers
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
        }

        public void SpitOutEnabledVariantsInFile() {
            if(ExtendedVariantsModule.Settings.FileOutput) {
                Logger.Log("ExtendedVariantMode/VariantRandomizer", $"Writing enabled variants to enabled-variants.txt");

                using (Stream fileStream = new FileStream("enabled-variants.txt", File.Exists("enabled-variants.txt") ? FileMode.Truncate : FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                using (StreamWriter fileWriter = new StreamWriter(fileStream, new UTF8Encoding())) {
                    IEnumerable<VanillaVariant> enabledVanillaVariants = allVanillaVariants.Where(variant => !isDefaultValue(variant));
                    IEnumerable<Variant> enabledExtendedVariants = allExtendedVariants.Where(variant => !isDefaultValue(variant));

                    foreach(VanillaVariant variant in enabledVanillaVariants) {
                        if (variant == VanillaVariant.DashMode) fileWriter.WriteLine($"{Dialog.Clean(variant.Label)}: " + Dialog.Clean($"MENU_ASSIST_AIR_DASHES_{SaveData.Instance.Assists.DashMode.ToString()}"));
                        else if (variant == VanillaVariant.GameSpeed) fileWriter.WriteLine($"{Dialog.Clean(variant.Label)}: {multiplierFormatter(SaveData.Instance.Assists.GameSpeed)}");
                        // the rest are toggles: if enabled, display the name.
                        else fileWriter.WriteLine(Dialog.Clean(variant.Label));
                    }

                    foreach(Variant variant in enabledExtendedVariants) {
                        string variantName = Dialog.Clean($"MODOPTIONS_EXTENDEDVARIANTS_{variant.ToString().ToUpperInvariant()}");

                        // "just print the raw value" variants
                        if (variant == Variant.DashCount || variant == Variant.AddSeekers)
                            fileWriter.WriteLine($"{variantName}: {ExtendedVariantTrigger.SetVariantValue(variant, ExtendedVariantTrigger.NO_CHANGE)}");
                        // variants that require a bit more formatting
                        else if (variant == Variant.Stamina) fileWriter.WriteLine($"{variantName}: {ExtendedVariantsModule.Settings.Stamina * 10}");
                        else if (variant == Variant.Friction) {
                            if(ExtendedVariantsModule.Settings.Friction == 0) fileWriter.WriteLine($"{variantName}: 0.05x");
                            else if (ExtendedVariantsModule.Settings.Friction == -1) fileWriter.WriteLine($"{variantName}: 0x");
                            else fileWriter.WriteLine($"{variantName}: {multiplierFormatter(ExtendedVariantsModule.Settings.Friction)}");
                        }
                        else if (variant == Variant.JumpCount)
                            fileWriter.WriteLine($"{variantName}: {(ExtendedVariantsModule.Settings.JumpCount == 6 ? Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INFINITE") : ExtendedVariantsModule.Settings.JumpCount.ToString())}");
                        else if (variant == Variant.Stamina) fileWriter.WriteLine($"{variantName}: {ExtendedVariantsModule.Settings.Stamina * 10}");
                        else if (variant == Variant.RegularHiccups) fileWriter.WriteLine($"{variantName}: {multiplierFormatter(ExtendedVariantsModule.Settings.RegularHiccups).Replace("x", "s")}");
                        else if (variant == Variant.RoomLighting) fileWriter.WriteLine($"{variantName}: {ExtendedVariantsModule.Settings.RoomLighting * 10}%");
                        // multiplier-style variants
                        else if (ExtendedVariantTrigger.GetDefaultValueForVariant(variant) == 10)
                            fileWriter.WriteLine($"{variantName}: {multiplierFormatter(ExtendedVariantTrigger.SetVariantValue(variant, ExtendedVariantTrigger.NO_CHANGE))}");
                        // toggle-style variants: print out the name
                        else fileWriter.WriteLine(variantName);
                    }
                }
            }
        }
        public void ClearFile() {
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
