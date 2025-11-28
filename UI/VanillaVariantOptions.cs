using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using ExtendedVariants.Variants.Vanilla;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;
using IsaVariant = Celeste.Mod.IsaGrabBag.Variant;
using IsaVariantState = Celeste.Mod.IsaGrabBag.VariantState;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.UI {
    public static class VanillaVariantOptions {
        private static Hook isaGrabBagHookSetVariant = null;
        private static Hook isaGrabBagHookSetVariantInGame = null;
        private static bool?[] activeIsaVariants = new bool?[11];

        public static void Load() {
            // we're replacing the menu, so we want to go first.
            using (new DetourConfigContext(new DetourConfig("ExtendedVariantMode_BeforeAll").WithPriority(int.MinValue)).Use()) {
                On.Celeste.Level.VariantMode += buildVariantModeMenu;
                On.Celeste.Level.AssistMode += buildAssistModeMenu;
            }
        }

        public static void Unload() {
            On.Celeste.Level.VariantMode -= buildVariantModeMenu;
            On.Celeste.Level.AssistMode -= buildAssistModeMenu;

            isaGrabBagHookSetVariant?.Dispose();
            isaGrabBagHookSetVariant = null;
            isaGrabBagHookSetVariantInGame?.Dispose();
            isaGrabBagHookSetVariantInGame = null;
            updateIsaDefault = null;
        }

        public static void Initialize() {
            if (isaGrabBagHookSetVariant == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "IsaGrabBag", Version = new Version(1, 6, 10) })) {
                IsaContainmentChamber.hookIsaGrabBag();
            }
        }

        private static Action<Variant, bool> updateIsaDefault = null;

        // we need the inner class because the actions and such are getting stored together in a '<>c' inner class, and
        // Mono freaks out on Mac/Linux if there is a field with an unknown type *next* to one that is getting used.
        // So yeah, containment chamber it is.
        private static class IsaContainmentChamber {
            public static void hookIsaGrabBag() {
                Action<Action<IsaVariant, IsaVariantState>, IsaVariant, IsaVariantState> setVariantHook = (orig, variant, variantState) => {
                    orig(variant, variantState);

                    // set to default => set to null
                    if (variantState == IsaVariantState.SetToDefault) {
                        Logger.Log("ExtendedVariantMode/VanillaVariantOptions", $"Intercepted Isa's Grab Bag resetting {variant} to default");
                        activeIsaVariants[(int) variant] = null;
                    }
                };

                MethodInfo setVariantMethod = typeof(Celeste.Mod.IsaGrabBag.ForceVariants).GetMethod("SetVariant", new Type[] { typeof(IsaVariant), typeof(IsaVariantState) });
                TryDisableInlining(setVariantMethod);
                isaGrabBagHookSetVariant = new Hook(setVariantMethod, setVariantHook);

                Action<Action<IsaVariant, bool>, IsaVariant, bool> setVariantInGameHook = (orig, variant, value) => {
                    orig(variant, value);

                    Logger.Log("ExtendedVariantMode/VanillaVariantOptions", $"Intercepted Isa's Grab Bag setting {variant} to {value}");
                    activeIsaVariants[(int) variant] = value;
                };

                MethodInfo setVariantInGameMethod = typeof(Celeste.Mod.IsaGrabBag.ForceVariants).GetMethod("SetVariantInGame", BindingFlags.NonPublic | BindingFlags.Static);
                TryDisableInlining(setVariantInGameMethod);
                isaGrabBagHookSetVariantInGame = new Hook(setVariantInGameMethod, setVariantInGameHook);

                PropertyInfo isaVariantsDefault = typeof(Celeste.Mod.IsaGrabBag.ForceVariants).GetProperty("Variants_Default", BindingFlags.NonPublic | BindingFlags.Static);
                if (isaVariantsDefault == null) {
                    Logger.Log(LogLevel.Error, "ExtendedVariantMode/VanillaVariantOptions", "Couldn't find ForceVariants.Variants_Default in IsaGrabBag! Expect your changes to vanilla variants to be reverted!");
                } else {
                    updateIsaDefault = (variant, value) => {
                        bool[] variantsDefault = (bool[]) isaVariantsDefault.GetValue(null);

                        IsaVariant? isaVariant = variant switch {
                            Variant.MirrorMode => IsaVariant.MirrorMode,
                            Variant.ThreeSixtyDashing => IsaVariant.ThreeSixtyDashing,
                            Variant.InvisibleMotion => IsaVariant.InvisibleMotion,
                            Variant.NoGrabbing => IsaVariant.NoGrabbing,
                            Variant.LowFriction => IsaVariant.LowFriction,
                            Variant.SuperDashing => IsaVariant.SuperDashing,
                            Variant.Hiccups => IsaVariant.Hiccups,
                            Variant.PlayAsBadeline => IsaVariant.PlayAsBadeline,
                            Variant.InfiniteStamina => IsaVariant.InfiniteStamina,
                            Variant.DashAssist => IsaVariant.DashAssist,
                            Variant.Invincible => IsaVariant.Invincible,
                            _ => null
                        };

                        if (!isaVariant.HasValue) {
                            Logger.Log(LogLevel.Warn, "ExtendedVariantMode/VanillaVariantOptions", $"{variant} isn't supported by Isa's Grab Bag, cannot change its default!");
                            return;
                        }

                        Logger.Log(LogLevel.Debug, "ExtendedVariantMode/VanillaVariantOptions", $"Changing default value of Isa's Grab Bag variant {isaVariant} to {value} following change by the player");
                        variantsDefault[(int) isaVariant] = value;

                        isaVariantsDefault.SetValue(null, variantsDefault);
                    };
                }
            }
        }

        private static void buildVariantModeMenu(On.Celeste.Level.orig_VariantMode orig, Celeste.Level self, int returnIndex, bool minimal) {
            orig(self, returnIndex, minimal);

            TextMenu menu = self.Entities.ToAdd.OfType<TextMenu>().First();
            menu.Clear();
            menu.CompactWidthMode = true;

            menu.Add(new TextMenu.Header(Dialog.Clean("MENU_VARIANT_TITLE")));

            menu.Add(new Celeste.TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_COLOR_ORANGE")) { TextColor = Color.Goldenrod });
            menu.Add(new Celeste.TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_COLOR_BLUE")) { TextColor = Color.DeepSkyBlue, HeightExtra = 0f });

            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MENU_VARIANT_SUBTITLE"), topPadding: true));

            addGameSpeed(menu, 16);
            menu.Add(getToggleOption(Variant.MirrorMode, "MENU_VARIANT_MIRROR", SaveData.Instance.Assists.MirrorMode, (int) IsaVariant.MirrorMode));
            menu.Add(getToggleOption(Variant.ThreeSixtyDashing, "MENU_VARIANT_360DASHING", SaveData.Instance.Assists.ThreeSixtyDashing, (int) IsaVariant.ThreeSixtyDashing));
            menu.Add(getToggleOption(Variant.InvisibleMotion, "MENU_VARIANT_INVISMOTION", SaveData.Instance.Assists.InvisibleMotion, (int) IsaVariant.InvisibleMotion));
            menu.Add(getToggleOption(Variant.NoGrabbing, "MENU_VARIANT_NOGRABBING", SaveData.Instance.Assists.NoGrabbing, (int) IsaVariant.NoGrabbing));
            menu.Add(getToggleOption(Variant.LowFriction, "MENU_VARIANT_LOWFRICTION", SaveData.Instance.Assists.LowFriction, (int) IsaVariant.LowFriction));
            menu.Add(getToggleOption(Variant.SuperDashing, "MENU_VARIANT_SUPERDASHING", SaveData.Instance.Assists.SuperDashing, (int) IsaVariant.SuperDashing));
            menu.Add(getToggleOption(Variant.Hiccups, "MENU_VARIANT_HICCUPS", SaveData.Instance.Assists.Hiccups, (int) IsaVariant.Hiccups));
            menu.Add(getToggleOption(Variant.PlayAsBadeline, "MENU_VARIANT_PLAYASBADELINE", SaveData.Instance.Assists.PlayAsBadeline, (int) IsaVariant.PlayAsBadeline));

            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MENU_ASSIST_SUBTITLE"), topPadding: true));

            menu.Add(getToggleOption(Variant.InfiniteStamina, "MENU_ASSIST_INFINITE_STAMINA", SaveData.Instance.Assists.InfiniteStamina, (int) IsaVariant.InfiniteStamina));
            addAirDashes(menu, self);
            menu.Add(getToggleOption(Variant.DashAssist, "MENU_ASSIST_DASH_ASSIST", SaveData.Instance.Assists.DashAssist, (int) IsaVariant.DashAssist));
            menu.Add(getToggleOption(Variant.Invincible, "MENU_ASSIST_INVINCIBLE", SaveData.Instance.Assists.Invincible, (int) IsaVariant.Invincible));

            menu.Selection = menu.FirstPossibleSelection;
        }

        private static void buildAssistModeMenu(On.Celeste.Level.orig_AssistMode orig, Level self, int returnIndex, bool minimal) {
            orig(self, returnIndex, minimal);

            TextMenu menu = self.Entities.ToAdd.OfType<TextMenu>().First();
            menu.Clear();
            menu.CompactWidthMode = true;

            menu.Add(new TextMenu.Header(Dialog.Clean("MENU_ASSIST_TITLE")));

            menu.Add(new Celeste.TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_COLOR_ORANGE")) { TextColor = Color.Goldenrod });
            menu.Add(new Celeste.TextMenuExt.SubHeaderExt(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_COLOR_BLUE") + "\n") { TextColor = Color.DeepSkyBlue, HeightExtra = 0f });

            addGameSpeed(menu, 10);
            menu.Add(getToggleOption(Variant.InfiniteStamina, "MENU_ASSIST_INFINITE_STAMINA", SaveData.Instance.Assists.InfiniteStamina, (int) IsaVariant.InfiniteStamina));
            addAirDashes(menu, self);
            menu.Add(getToggleOption(Variant.DashAssist, "MENU_ASSIST_DASH_ASSIST", SaveData.Instance.Assists.DashAssist, (int) IsaVariant.DashAssist));
            menu.Add(getToggleOption(Variant.Invincible, "MENU_ASSIST_INVINCIBLE", SaveData.Instance.Assists.Invincible, (int) IsaVariant.Invincible));

            menu.Selection = menu.FirstPossibleSelection;
        }

        private static void addGameSpeed(TextMenu menu, int max) {
            TextMenuExt.Slider speed;
            menu.Add(speed = new TextMenuExt.Slider(Dialog.Clean("MENU_ASSIST_GAMESPEED"), (int i) => i * 10 + "%", 5, Math.Max(max, SaveData.Instance.Assists.GameSpeed),
                SaveData.Instance.Assists.GameSpeed, 5,
                (int) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(Variant.VanillaGameSpeed) - 5));

            speed.Change(i => {
                if (i > 10) {
                    i = ((speed.Values[speed.PreviousIndex].Item2 <= i) ? (i + 1) : (i - 1));
                }
                speed.Index = i - 5;
                SetVariantValue(Variant.VanillaGameSpeed, i);
            });
        }

        private static void addAirDashes(TextMenu menu, Level self) {
            TextMenu.Option<int> airDashes;
            menu.Add(airDashes = new TextMenuExt.Slider(Dialog.Clean("MENU_ASSIST_AIR_DASHES"), (int i) => i switch {
                0 => Dialog.Clean("MENU_ASSIST_AIR_DASHES_NORMAL"),
                1 => Dialog.Clean("MENU_ASSIST_AIR_DASHES_TWO"),
                _ => Dialog.Clean("MENU_ASSIST_AIR_DASHES_INFINITE"),
            },
                0, 2,
                (int) SaveData.Instance.Assists.DashMode,
                0,
                (int) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(Variant.AirDashes)
            ).Change(i => SetVariantValue(Variant.AirDashes, (Assists.DashModes) i)));

            if (self.Session.Area.ID == 0) {
                airDashes.Disabled = true;
            }
        }

        private static TextMenuExt.OnOff getToggleOption(Variant variant, string variantName, bool variantValue, int isaVariantIndex) {
            // only read the extended variants-defined value if Isa's grab bag did not set anything on its end.
            bool mapDefinedVariantValue = activeIsaVariants[isaVariantIndex] ??
                (bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(variant);

            return (TextMenuExt.OnOff) new TextMenuExt.OnOff(
                Dialog.Clean(variantName),
                variantValue,
                (bool) ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue(),
                mapDefinedVariantValue)
                    .Change(b => SetVariantValue(variant, b));
        }

        public static void SetVariantValue(Variant variantChange, object newValue) {
            if (Engine.Scene is Level) {
                if (ExtendedVariantTriggerManager.AreValuesIdentical(newValue, ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(variantChange))) {
                    Logger.Log("ExtendedVariantsModule/ModOptionsEntries", $"Variant value {variantChange} = {newValue} was equal to the map-defined value, so it was removed from the overrides.");
                    ExtendedVariantsModule.Session.VariantsOverridenByUser.Remove(variantChange);
                } else {
                    Logger.Log("ExtendedVariantsModule/ModOptionsEntries", $"Variant value {variantChange} = {newValue} was added to the overrides.");
                    ExtendedVariantsModule.Session.VariantsOverridenByUser.Add(variantChange);
                }
            }

            (ExtendedVariantsModule.Instance.VariantHandlers[variantChange] as AbstractVanillaVariant).VariantValueChangedByPlayer(newValue);
            ExtendedVariantsModule.Instance.VariantHandlers[variantChange].VariantValueChanged();
            VariantRandomizer.RefreshEnabledVariantsDisplayList();
            if (newValue is bool value) updateIsaDefault?.Invoke(variantChange, value);
        }
    }
}
