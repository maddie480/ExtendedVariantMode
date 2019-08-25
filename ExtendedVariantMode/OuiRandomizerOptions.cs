using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.ExtendedVariants {
    // That's just me trying to implement a mod options submenu. Don't mind me
    // Heavily based off the OuiModOptions from Everest: https://github.com/EverestAPI/Everest/blob/master/Celeste.Mod.mm/Mod/UI/OuiModOptions.cs
    class OuiRandomizerOptions : Oui {

        private TextMenu menu;

        private const float onScreenX = 960f;
        private const float offScreenX = 2880f;

        private float alpha = 0f;

        private class OptionItems {
            public HashSet<TextMenu.Item> VanillaVariantOptions = new HashSet<TextMenu.Item>();
            public HashSet<TextMenu.Item> ExtendedVariantOptions = new HashSet<TextMenu.Item>();
        }

        public static TextMenu BuildMenu() {
            TextMenu menu = new TextMenu();

            OptionItems items = new OptionItems();

            menu.Add(new TextMenu.Header(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZERTITLE")));

            // build the toggles to individually enable or disable all vanilla variants
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_ENABLED_VANILLA")));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "GameSpeed", "MENU_ASSIST_GAMESPEED"));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "MirrorMode", "MENU_VARIANT_MIRROR"));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "ThreeSixtyDashing", "MENU_VARIANT_360DASHING"));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "InvisibleMotion", "MENU_VARIANT_INVISMOTION"));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "NoGrabbing", "MENU_VARIANT_NOGRABBING"));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "LowFriction", "MENU_VARIANT_LOWFRICTION"));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "SuperDashing", "MENU_VARIANT_SUPERDASHING"));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "Hiccups", "MENU_VARIANT_HICCUPS"));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "InfiniteStamina", "MENU_ASSIST_INFINITE_STAMINA"));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "DashMode", "MENU_ASSIST_AIR_DASHES"));
            items.VanillaVariantOptions.Add(addToggleOptionToMenu(menu, "Invincible", "MENU_ASSIST_INVINCIBLE"));

            // and do the same with extended ones
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RANDOMIZER_ENABLED_EXTENDED")));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.Gravity));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.FallSpeed));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.JumpHeight));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.WallBouncingSpeed));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.DisableWallJumping));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.JumpCount));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.DashSpeed));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.DashLength));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.HyperdashSpeed));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.DashCount));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.SpeedX));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.Friction));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.BadelineChasersEverywhere));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.OshiroEverywhere));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.WindEverywhere));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.SnowballsEverywhere));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.AddSeekers));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.Stamina));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.UpsideDown));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.DisableNeutralJumping));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.RegularHiccups));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.RoomLighting));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.ForceDuckOnGround));
            items.ExtendedVariantOptions.Add(addToggleOptionToMenu(menu, Variant.InvertDashes));

            return menu;
        }
        
        private static TextMenu.Item addToggleOptionToMenu(TextMenu menu, Variant variant, string label = null) {
            if(label == null) {
                label = "MODOPTIONS_EXTENDEDVARIANTS_" + variant.ToString().ToUpperInvariant();
            }
            return addToggleOptionToMenu(menu, variant.ToString(), label);
        }

        private static TextMenu.Item addToggleOptionToMenu(TextMenu menu, string keyName, string label) {
            TextMenu.Option<bool> toggle = new TextMenuExt.OnOff(Dialog.Clean(label),
                ExtendedVariantsModule.Settings.RandomizerEnabledVariants.TryGetValue(keyName, out bool val) ? val : true, false)
                .Change(newValue => ExtendedVariantsModule.Settings.RandomizerEnabledVariants[keyName] = newValue);
            menu.Add(toggle);
            return toggle;
        }

        public override IEnumerator Enter(Oui from) {
            menu = BuildMenu();
            Scene.Add(menu);

            menu.Visible = Visible = true;
            menu.Focused = false;

            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f) {
                menu.X = offScreenX + -1920f * Ease.CubeOut(p);
                alpha = Ease.CubeOut(p);
                yield return null;
            }

            menu.Focused = true;
        }

        public override IEnumerator Leave(Oui next) {
            Audio.Play(Sfxs.ui_main_whoosh_large_out);
            menu.Focused = false;

            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f) {
                menu.X = onScreenX + 1920f * Ease.CubeIn(p);
                alpha = 1f - Ease.CubeIn(p);
                yield return null;
            }

            menu.Visible = Visible = false;
            menu.RemoveSelf();
            menu = null;
        }

        public override void Update() {
            if (menu != null && menu.Focused && Selected && Input.MenuCancel.Pressed) {
                Audio.Play(Sfxs.ui_main_button_back);
                Overworld.Goto<OuiModOptions>();
            }

            base.Update();
        }

        public override void Render() {
            if (alpha > 0f) {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * alpha * 0.4f);
            }
            base.Render();
        }
    }
}
