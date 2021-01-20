using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using MonoMod.Cil;
using MonoMod.Utils;

namespace ExtendedVariants.Variants {
    public class ColorGrading : AbstractExtendedVariant {
        private static FieldInfo everestContentLoaded = typeof(Everest).GetField("_ContentLoaded", BindingFlags.NonPublic | BindingFlags.Static);

        public static List<string> ExistingColorGrades = new List<string> {
            "none", "oldsite", "panicattack", "templevoid", "reflection", "credits", "cold", "hot", "feelingdown", "golden",
            "max480/extendedvariants/celsius/tetris", // thanks 0x0ade!
            "max480/extendedvariants/greyscale", "max480/extendedvariants/sepia", "max480/extendedvariants/inverted",
            "max480/extendedvariants/rgbshift1", "max480/extendedvariants/rgbshift2", "max480/extendedvariants/hollys_randomnoise"
        };

        public override int GetDefaultValue() {
            return -1;
        }

        public override int GetValue() {
            return Settings.ColorGrading;
        }

        public override void SetValue(int value) {
            Settings.ColorGrading = value;
            Settings.ModColorGrade = null;

            // if setting to "trigger", add "trigger" to the list.
            if (value >= ExistingColorGrades.Count && !ExistingColorGrades.Contains("trigger")) {
                ExistingColorGrades.Add("trigger");
            }
        }

        public override void Load() {
            IL.Celeste.Level.Render += modLevelRender;
            Everest.Events.Level.OnExit += onLevelExit;
            On.Celeste.Celeste.LoadContent += onLoadContent;

            // make triple sure the current color grade isn't out of bounds.
            if (Settings.ColorGrading >= ExistingColorGrades.Count) {
                Settings.ColorGrading = -1;
                Settings.ModColorGrade = null;
            }

            // if color grades were loaded, we can check if the mod color grade currently configured still exists.
            if ((bool) everestContentLoaded.GetValue(null)) {
                checkModColorGrade();
            }
        }

        public override void Unload() {
            IL.Celeste.Level.Render -= modLevelRender;
            Everest.Events.Level.OnExit -= onLevelExit;
            On.Celeste.Celeste.LoadContent -= onLoadContent;

            ExistingColorGrades.Remove("trigger");
        }

        private void onLoadContent(On.Celeste.Celeste.orig_LoadContent orig, Celeste.Celeste self) {
            orig(self);
            checkModColorGrade();
        }

        private void checkModColorGrade() {
            if (Settings.ModColorGrade != null && !Everest.Content.Map.ContainsKey("Graphics/ColorGrading/" + Settings.ModColorGrade)) {
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ColorGrading", "Graphics/ColorGrading/" + Settings.ModColorGrade + " doesn't exist! Resetting color grade setting.");
                Settings.ColorGrading = -1;
                Settings.ModColorGrade = null;
            }
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            ExistingColorGrades.Remove("trigger");
            if (Settings.ColorGrading >= ExistingColorGrades.Count) {
                Settings.ColorGrading = -1;
                Settings.ModColorGrade = null;
            }
        }

        public void SetColorGrade(string colorGrade, bool revertOnDeath) {
            // delete "trigger" if present
            ExistingColorGrades.Remove("trigger");

            if (ExistingColorGrades.Contains(colorGrade)) {
                // this is the equivalent of using the extended variant trigger with the Color Grading variant.
                ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(ExtendedVariantsModule.Variant.ColorGrading,
                    ExistingColorGrades.IndexOf(colorGrade), revertOnLeave: false, isFade: false, revertOnDeath);
            } else {
                // add a new special "trigger" entry for that custom color grade.
                ExistingColorGrades.Add("trigger");
                ExtendedVariantsModule.Session.TriggerColorGrade = colorGrade;
                ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(ExtendedVariantsModule.Variant.ColorGrading,
                    ExistingColorGrades.IndexOf("trigger"), revertOnLeave: false, isFade: false, revertOnDeath);
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
            if (Settings.ColorGrading == -1) return vanillaValue;
            if (ExistingColorGrades[Settings.ColorGrading] == "trigger") return ExtendedVariantsModule.Session.TriggerColorGrade;
            if (Settings.ModColorGrade != null) return Settings.ModColorGrade;
            return ExistingColorGrades[Settings.ColorGrading];
        }
    }
}
