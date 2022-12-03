using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using MonoMod.Cil;

namespace ExtendedVariants.Variants {
    public class ColorGrading : AbstractExtendedVariant {
        private static FieldInfo everestContentLoaded = typeof(Everest).GetField("_ContentLoaded", BindingFlags.NonPublic | BindingFlags.Static);

        public static List<string> ExistingColorGrades = new List<string> {
            "none", "oldsite", "panicattack", "templevoid", "reflection", "credits", "cold", "hot", "feelingdown", "golden",
            "max480/extendedvariants/celsius/tetris", // thanks 0x0ade!
            "max480/extendedvariants/greyscale", "max480/extendedvariants/sepia", "max480/extendedvariants/inverted",
            "max480/extendedvariants/rgbshift1", "max480/extendedvariants/rgbshift2", "max480/extendedvariants/hollys_randomnoise"
        };

        public override Type GetVariantType() {
            return typeof(string);
        }

        public override object GetDefaultVariantValue() {
            return "";
        }

        public override object GetVariantValue() {
            return Settings.ColorGrading;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.ColorGrading = (string) value;
        }

        public override void SetLegacyVariantValue(int value) {
            if (value == -1 || ExistingColorGrades.Count <= value) {
                Settings.ColorGrading = "";
            } else {
                Settings.ColorGrading = ExistingColorGrades[value];
            }
        }

        public override void Load() {
            IL.Celeste.Level.Render += modLevelRender;
            On.Celeste.Celeste.LoadContent += onLoadContent;

            // if color grades were loaded, we can check if the color grade currently configured still exists.
            if ((bool) everestContentLoaded.GetValue(null)) {
                checkColorGrade();
            }
        }

        public override void Unload() {
            IL.Celeste.Level.Render -= modLevelRender;
            On.Celeste.Celeste.LoadContent -= onLoadContent;
        }

        private void onLoadContent(On.Celeste.Celeste.orig_LoadContent orig, Celeste.Celeste self) {
            orig(self);
            checkColorGrade();
        }

        private void checkColorGrade() {
            if (!string.IsNullOrEmpty(Settings.ColorGrading) && !Everest.Content.Map.ContainsKey("Graphics/ColorGrading/" + Settings.ColorGrading)) {
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ColorGrading", "Graphics/ColorGrading/" + Settings.ColorGrading + " doesn't exist! Resetting color grade setting.");
                Settings.ColorGrading = "";
            }
        }

        private void modLevelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // when the variant is enabled, replace the values of both lastColorGrade and Session.ColorGrade when the Render method checks them.
            // this way,
            // 1/ the game thinks this is both the current and previous color grades, and that there is no fade to make
            // 2/ we don't touch the session itself, so it will behave like normal
            // 3/ when we disable the variant, everything goes back to normal again.
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Level>("lastColorGrade"))) {
                Logger.Log("ExtendedVariantMode/ColorGrading", $"Modding color grading at {cursor.Index} in IL code for Level.Render");
                cursor.EmitDelegate<Func<string, string>>(modColorGrading);
            }
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Session>("ColorGrade"))) {
                Logger.Log("ExtendedVariantMode/ColorGrading", $"Modding color grading at {cursor.Index} in IL code for Level.Render");
                cursor.EmitDelegate<Func<string, string>>(modColorGrading);
            }
        }

        private string modColorGrading(string vanillaValue) {
            if (Settings.ColorGrading == "") {
                return vanillaValue;
            }

            return Settings.ColorGrading;
        }
    }
}
