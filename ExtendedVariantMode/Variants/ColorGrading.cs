using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using MonoMod.Cil;

namespace ExtendedVariants.Variants {
    class ColorGrading : AbstractExtendedVariant {

        public static List<string> ExistingColorGrades = new List<string> {
            "none", "oldsite", "panicattack", "templevoid", "reflection", "credits", "cold", "hot", "feelingdown", "golden",
            "max480/extendedvariants/greyscale", "max480/extendedvariants/sepia", "max480/extendedvariants/inverted",
            "max480/extendedvariants/rgbshift1", "max480/extendedvariants/rgbshift2"
        };

        public override int GetDefaultValue() {
            return -1;
        }

        public override int GetValue() {
            return Settings.ColorGrading;
        }

        public override void SetValue(int value) {
            Settings.ColorGrading = value;
        }

        public override void Load() {
            IL.Celeste.Level.Render += modLevelRender;
        }

        public override void Unload() {
            IL.Celeste.Level.Render -= modLevelRender;

            // yes, this is apparently necessary. TODO: report this.
            Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ColorGrading", "Unhook workaround trick for Level.Render!");
            IL.Celeste.Level.Render += doNothing;
            IL.Celeste.Level.Render -= doNothing;
        }

        private void doNothing(ILContext il) {}

        public void Initialize() {
            // called if Celsius (a map by 0x0ade featuring a Tetris-style color-grade) is installed.
            // if so, we should find this color grade.
            if (GFX.ColorGrades.Has("celsius/tetris")) {
                // insert it right after the vanilla ones.
                Logger.Log("ExtendedVariantMode/ColorGrading", "Celsius was detected, registering Tetris color grade");
                ExistingColorGrades.Insert(10, "celsius/tetris");
            }

            // check if the setting value is in bounds (this can get out of bounds if Celsius is uninstalled and the Celsius color grade is selected).
            if(Settings.ColorGrading >= ExistingColorGrades.Count) {
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ColorGrading", $"ColorGrading setting was out of bounds, disabled it.");
                Settings.ColorGrading = -1;
            }
        }

        private void modLevelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // when the variant is enabled, replace the values of both lastColorGrade and Session.ColorGrade when the Render method checks them.
            // this way, 
            // 1/ the game thinks this is both the current and previous color grades, and that there is no fade to make
            // 2/ we don't touch the session itself, so it will behave like normal
            // 3/ when we disable the variant, everything goes back to normal again.
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Level>("lastColorGrade"))) {
                Logger.Log("ExtendedVariantMode/ColorGrading", $"Modding color grading at {cursor.Index} in IL code for Level.Render");
                cursor.EmitDelegate<Func<string, string>>(modColorGrading);
            }
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Session>("ColorGrade"))) {
                Logger.Log("ExtendedVariantMode/ColorGrading", $"Modding color grading at {cursor.Index} in IL code for Level.Render");
                cursor.EmitDelegate<Func<string, string>>(modColorGrading);
            }
        }

        private string modColorGrading(string vanillaValue) {
            if (Settings.ColorGrading == -1) return vanillaValue;
            return ExistingColorGrades[Settings.ColorGrading];
        }
    }
}
