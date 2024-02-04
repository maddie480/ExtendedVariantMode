using Celeste;
using Celeste.Mod;
using Celeste.Mod.UI;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using ExtendedVariants.Variants.Vanilla;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ExtendedVariants.UI {
    /// <summary>
    /// The randomizer options submenu. Parameters = none.
    /// </summary>
    public class OuiRandomizerOptions : AbstractSubmenu {

        public OuiRandomizerOptions() : base("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZERTITLE", "MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER") { }

        private int returnIndex = -1;

        /// <summary>
        /// List of options shown for Change Variants Interval.
        /// </summary>
        private static int[] changeVariantsIntervalScale = new int[] {
            0, 1, 2, 5, 10, 15, 30, 60
        };

        /// <summary>
        /// Finds out the index of an interval in the changeVariantsIntervalScale table.
        /// If it is not present, will return the previous option.
        /// (For example, 26s will return the index for 15s.)
        /// </summary>
        /// <param name="option">The interval</param>
        /// <returns>The index of the interval in the changeVariantsIntervalScale table</returns>
        private static int indexFromChangeVariantsInterval(int option) {
            for (int index = 0; index < changeVariantsIntervalScale.Length - 1; index++) {
                if (changeVariantsIntervalScale[index + 1] > option) {
                    return index;
                }
            }

            return changeVariantsIntervalScale.Length - 1;
        }


        private static int[] vanillafyScale = new int[] {
            0, 15, 30, 60, 120, 300, 600
        };

        private static int indexFromVanillafyScale(int option) {
            for (int index = 0; index < vanillafyScale.Length - 1; index++) {
                if (vanillafyScale[index + 1] > option) {
                    return index;
                }
            }

            return vanillafyScale.Length - 1;
        }

        private class OptionItems {
            public HashSet<TextMenu.Item> VanillaVariantOptions = new HashSet<TextMenu.Item>();
            public HashSet<TextMenu.Item> ExtendedVariantOptions = new HashSet<TextMenu.Item>();
            public TextMenu.Option<int> VanillafyOption;
        }

        protected override void addOptionsToMenu(TextMenu menu, bool inGame, object[] parameters) {
            OptionItems items = new OptionItems();

            // Add the general settings
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_GENERALSETTINGS")));
            menu.Add(new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHANGEVARIANTSINTERVAL"),
                i => {
                    if (i == 0) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_ONSCREENTRANSITION");
                    return $"{Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_EVERY")} {changeVariantsIntervalScale[i]}s";
                }, 0, changeVariantsIntervalScale.Length - 1, indexFromChangeVariantsInterval(ExtendedVariantsModule.Settings.ChangeVariantsInterval))
                .Change(i => {
                    ExtendedVariantsModule.Settings.ChangeVariantsInterval = changeVariantsIntervalScale[i];
                    refreshOptionMenuEnabledStatus(items);
                    ExtendedVariantsModule.Instance.Randomizer.UpdateCountersFromSettings();
                }));

            menu.Add(new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_VARIANTSET"),
                i => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_" + new string[] { "OFF", "VANILLA", "EXTENDED", "BOTH" }[i]), 1, 3, ExtendedVariantsModule.Settings.VariantSet)
                .Change(i => {
                    ExtendedVariantsModule.Settings.VariantSet = i;
                    refreshOptionMenuEnabledStatus(items);
                }));

            TextMenu.Option<int> maxEnabledVariants = new TextMenu.Slider(
                Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_MAXENABLEDVARIANTS" + (ExtendedVariantsModule.Settings.RerollMode ? "_REROLL" : "")),
                i => i.ToString(), 0, ExtendedVariantsModule.Instance.VariantHandlers.Count, ExtendedVariantsModule.Settings.MaxEnabledVariants)
                .Change(newValue => ExtendedVariantsModule.Settings.MaxEnabledVariants = newValue);

            menu.Add(new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_REROLLMODE"), ExtendedVariantsModule.Settings.RerollMode)
                .Change(newValue => {
                    ExtendedVariantsModule.Settings.RerollMode = newValue;
                    maxEnabledVariants.Label = Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_MAXENABLEDVARIANTS" + (newValue ? "_REROLL" : ""));
                }));

            menu.Add(maxEnabledVariants);

            menu.Add(items.VanillafyOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_VANILLAFY"), i => {
                if (i == 0) return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLED");
                i = vanillafyScale[i];
                if (i < 60) return $"{i.ToString()}s";
                return $"{(i / 60).ToString()} min";
            }, 0, vanillafyScale.Length - 1, indexFromVanillafyScale(ExtendedVariantsModule.Settings.Vanillafy))
                .Change(newValue => {
                    ExtendedVariantsModule.Settings.Vanillafy = vanillafyScale[newValue];
                    ExtendedVariantsModule.Instance.Randomizer.UpdateCountersFromSettings();
                }));

            if (!inGame) {
                TextMenu.Button seedButton = new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_SEEDINPUT") + " " + ExtendedVariantsModule.Settings.RandoSetSeed);
                seedButton.Pressed(() => {
                    returnIndex = menu.Selection;
                    Audio.Play(SFX.ui_main_savefile_rename_start);
                    menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiRandomizerOptions>(
                        ExtendedVariantsModule.Settings.RandoSetSeed,
                        v => ExtendedVariantsModule.Settings.RandoSetSeed = v,
                        25
                    );
                });

                TextMenu.Option<bool> toggle = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_SETSEED"), ExtendedVariantsModule.Settings.RandoSetSeed != null)
                    .Change(newValue => {
                        ExtendedVariantsModule.Settings.RandoSetSeed = (newValue ? "seed" : null);

                        seedButton.Visible = newValue;
                        seedButton.Label = Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_SEEDINPUT") + " seed";
                    });

                seedButton.Visible = ExtendedVariantsModule.Settings.RandoSetSeed != null;

                menu.Add(toggle);
                toggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_SEEDDESCRIPTION2"));
                toggle.AddDescription(menu, Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_SEEDDESCRIPTION1"));
                menu.Add(seedButton);
            }


            // build the toggles to individually enable or disable all vanilla variants
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_ENABLED_VANILLA")));
            foreach (ExtendedVariantsModule.Variant variant in ExtendedVariantsModule.Instance.VariantHandlers.Keys) {
                if (ExtendedVariantsModule.Instance.VariantHandlers[variant] is AbstractVanillaVariant) {
                    items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, variant));
                }
            }

            // and do the same with extended ones
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_ENABLED_EXTENDED")));
            foreach (ExtendedVariantsModule.Variant variant in ExtendedVariantsModule.Instance.VariantHandlers.Keys) {
                if (ExtendedVariantsModule.Instance.VariantHandlers[variant] is not AbstractVanillaVariant) {
                    items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, variant));
                }
            }

            refreshOptionMenuEnabledStatus(items);

            if (returnIndex >= 0) {
                menu.Selection = returnIndex;
                returnIndex = -1;
            }
        }

        private static void refreshOptionMenuEnabledStatus(OptionItems items) {
            foreach (TextMenu.Item item in items.VanillaVariantOptions) item.Disabled = (ExtendedVariantsModule.Settings.VariantSet % 2 == 0);
            foreach (TextMenu.Item item in items.ExtendedVariantOptions) item.Disabled = (ExtendedVariantsModule.Settings.VariantSet / 2 == 0);
            items.VanillafyOption.Disabled = ExtendedVariantsModule.Settings.ChangeVariantsInterval != 0;

            if (ExtendedVariantsModule.Settings.ChangeVariantsInterval != 0 && ExtendedVariantsModule.Settings.Vanillafy != 0) {
                // vanillafy is impossible, so set its value to 0
                ExtendedVariantsModule.Settings.Vanillafy = 0;
                items.VanillafyOption.PreviousIndex = items.VanillafyOption.Index;
                items.VanillafyOption.Index = 0;
                items.VanillafyOption.ValueWiggler.Start();
            }
        }

        private static TextMenu.Item addToggleOptionToMenu(TextMenu menu, ExtendedVariantsModule.Variant variant) {
            string keyName = variant.ToString();
            string label;
            if (ExtendedVariantsModule.Instance.VariantHandlers[variant] is AbstractVanillaVariant) {
                label = VariantRandomizer.GetVanillaVariantLabel(variant);
            } else {
                label = Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_" + variant.ToString().ToUpperInvariant());
            }

            TextMenu.Option<bool> toggle = new TextMenuExt.OnOff(label,
                ExtendedVariantsModule.Settings.RandomizerEnabledVariants.TryGetValue(keyName, out bool val) ? val : true, false, false)
                .Change(newValue => ExtendedVariantsModule.Settings.RandomizerEnabledVariants[keyName] = newValue);
            menu.Add(toggle);
            return toggle;
        }

        protected override void gotoMenu(Overworld overworld) {
            overworld.Goto<OuiRandomizerOptions>();
        }
    }
}
