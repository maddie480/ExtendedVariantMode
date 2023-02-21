using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using ExtendedVariants.Variants.Vanilla;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.UI {
    public static class VanillaVariantOptions {
        public static void Load() {
            using (new DetourContext {
                Before = { "*" } // we're replacing the menu, so we want to go first.
            }) {
                On.Celeste.Level.VariantMode += buildVariantModeMenu;
                On.Celeste.Level.AssistMode += buildAssistModeMenu;
            }
        }

        public static void Unload() {
            On.Celeste.Level.VariantMode -= buildVariantModeMenu;
            On.Celeste.Level.AssistMode -= buildAssistModeMenu;
        }

        private static void buildVariantModeMenu(On.Celeste.Level.orig_VariantMode orig, Celeste.Level self, int returnIndex, bool minimal) {
            orig(self, returnIndex, minimal);

            TextMenu menu = self.Entities.ToAdd.OfType<TextMenu>().First();
            menu.Clear();

            menu.Add(new TextMenu.Header(Dialog.Clean("MENU_VARIANT_TITLE")));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MENU_VARIANT_SUBTITLE"), topPadding: true));

            addGameSpeed(menu, 16);
            menu.Add(getToggleOption(Variant.MirrorMode, "MENU_VARIANT_MIRROR", SaveData.Instance.Assists.MirrorMode));
            menu.Add(getToggleOption(Variant.ThreeSixtyDashing, "MENU_VARIANT_360DASHING", SaveData.Instance.Assists.ThreeSixtyDashing));
            menu.Add(getToggleOption(Variant.InvisibleMotion, "MENU_VARIANT_INVISMOTION", SaveData.Instance.Assists.InvisibleMotion));
            menu.Add(getToggleOption(Variant.NoGrabbing, "MENU_VARIANT_NOGRABBING", SaveData.Instance.Assists.NoGrabbing));
            menu.Add(getToggleOption(Variant.LowFriction, "MENU_VARIANT_LOWFRICTION", SaveData.Instance.Assists.LowFriction));
            menu.Add(getToggleOption(Variant.SuperDashing, "MENU_VARIANT_SUPERDASHING", SaveData.Instance.Assists.SuperDashing));
            menu.Add(getToggleOption(Variant.Hiccups, "MENU_VARIANT_HICCUPS", SaveData.Instance.Assists.Hiccups));
            menu.Add(getToggleOption(Variant.PlayAsBadeline, "MENU_VARIANT_PLAYASBADELINE", SaveData.Instance.Assists.PlayAsBadeline));

            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MENU_ASSIST_SUBTITLE"), topPadding: true));

            menu.Add(getToggleOption(Variant.InfiniteStamina, "MENU_ASSIST_INFINITE_STAMINA", SaveData.Instance.Assists.InfiniteStamina));
            addAirDashes(menu, self);
            menu.Add(getToggleOption(Variant.DashAssist, "MENU_ASSIST_DASH_ASSIST", SaveData.Instance.Assists.DashAssist));
            menu.Add(getToggleOption(Variant.Invincible, "MENU_ASSIST_INVINCIBLE", SaveData.Instance.Assists.Invincible));
        }

        private static void buildAssistModeMenu(On.Celeste.Level.orig_AssistMode orig, Level self, int returnIndex, bool minimal) {
            orig(self, returnIndex, minimal);

            TextMenu menu = self.Entities.ToAdd.OfType<TextMenu>().First();
            menu.Clear();

            menu.Add(new TextMenu.Header(Dialog.Clean("MENU_ASSIST_TITLE")));
            addGameSpeed(menu, 10);
            menu.Add(getToggleOption(Variant.InfiniteStamina, "MENU_ASSIST_INFINITE_STAMINA", SaveData.Instance.Assists.InfiniteStamina));
            addAirDashes(menu, self);
            menu.Add(getToggleOption(Variant.DashAssist, "MENU_ASSIST_DASH_ASSIST", SaveData.Instance.Assists.DashAssist));
            menu.Add(getToggleOption(Variant.Invincible, "MENU_ASSIST_INVINCIBLE", SaveData.Instance.Assists.Invincible));
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

        private static TextMenuExt.OnOff getToggleOption(Variant variant, string variantName, bool variantValue) {
            return (TextMenuExt.OnOff) new TextMenuExt.OnOff(
                Dialog.Clean(variantName),
                variantValue,
                (bool) ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue(),
                (bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(variant))
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
            ExtendedVariantsModule.Instance.Randomizer.RefreshEnabledVariantsDisplayList();
        }
    }
}
