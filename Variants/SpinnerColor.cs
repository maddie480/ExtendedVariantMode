using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class SpinnerColor : AbstractExtendedVariant {
        private static Hook frostHelperHook;
        private static Hook vivHelperHook;

        public enum Color {
            Default = -1,
            Blue = CrystalColor.Blue,
            Red = CrystalColor.Red,
            Purple = CrystalColor.Purple,
            Rainbow = CrystalColor.Rainbow
        }

        public override Type GetVariantType() {
            return typeof(Color);
        }

        public override object GetDefaultVariantValue() {
            return Color.Default;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return (Color) value;
        }

        public override void Load() {
            On.Celeste.CrystalStaticSpinner.ctor_Vector2_bool_CrystalColor += onCrystalSpinnerConstructor;

            Initialize();
        }

        public override void Unload() {
            On.Celeste.CrystalStaticSpinner.ctor_Vector2_bool_CrystalColor -= onCrystalSpinnerConstructor;

            frostHelperHook?.Dispose();
            frostHelperHook = null;

            vivHelperHook?.Dispose();
            vivHelperHook = null;
        }

        public void Initialize() {
            if (frostHelperHook == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "FrostHelper", Version = new Version(1, 41, 2) })) {
                ConstructorInfo frostConstructor = Everest.Modules.Where(m => m.Metadata?.Name == "FrostHelper").First().GetType().Assembly
                    .GetType("FrostHelper.CustomSpinner").GetConstructor(new Type[] { typeof(EntityData), typeof(Vector2), typeof(bool), typeof(string), typeof(string), typeof(bool), typeof(string) });

                frostHelperHook = new Hook(frostConstructor,
                    typeof(SpinnerColor).GetMethod("onFrostHelperSpinnerConstructor", BindingFlags.NonPublic | BindingFlags.Instance), this);
            }

            if (vivHelperHook == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "VivHelper", Version = new Version(1, 12, 2) })) {
                ConstructorInfo vivConstructor = Everest.Modules.Where(m => m.Metadata?.Name == "VivHelper").First().GetType().Assembly
                    .GetType("VivHelper.Entities.CustomSpinner").GetConstructor(new Type[] { typeof(EntityData), typeof(Vector2) });

                vivHelperHook = new Hook(vivConstructor,
                    typeof(SpinnerColor).GetMethod("onVivHelperSpinnerConstructor", BindingFlags.NonPublic | BindingFlags.Instance), this);
            }
        }

        private void onCrystalSpinnerConstructor(On.Celeste.CrystalStaticSpinner.orig_ctor_Vector2_bool_CrystalColor orig, CrystalStaticSpinner self,
            Vector2 position, bool attachToSolid, CrystalColor color) {

            Color spinnerColor = GetVariantValue<Color>(Variant.SpinnerColor);
            if (spinnerColor != Color.Default) {
                color = (CrystalColor) spinnerColor;
            }

            orig(self, position, attachToSolid, color);
        }

        private void onFrostHelperSpinnerConstructor(Action<Entity, EntityData, Vector2, bool, string, string, bool, string> orig, Entity self,
            EntityData data, Vector2 position, bool attachToSolid, string directory, string destroyColor, bool isCore, string tint) {

            Color spinnerColor = GetVariantValue<Color>(Variant.SpinnerColor);
            if (spinnerColor != Color.Default) {
                data = new EntityData {
                    Position = data.Position,
                    ID = data.ID,
                    Values = new Dictionary<string, object>(data.Values)
                };

                data.Values["rainbow"] = spinnerColor == Color.Rainbow;
                data.Values["spritePathSuffix"] = "_" + getColorName(spinnerColor);
                data.Values["drawOutline"] = true;
                data.Values["borderColor"] = "000000";
                directory = "danger/crystal";
                destroyColor = getDebrisColor(spinnerColor);
                isCore = false;
                tint = "ffffff";
            }

            orig(self, data, position, attachToSolid, directory, destroyColor, isCore, tint);
        }


        private void onVivHelperSpinnerConstructor(Action<Entity, EntityData, Vector2> orig, Entity self, EntityData data, Vector2 offset) {
            Color spinnerColor = GetVariantValue<Color>(Variant.SpinnerColor);
            if (spinnerColor != Color.Default) {
                data = new EntityData {
                    Position = data.Position,
                    ID = data.ID,
                    Values = new Dictionary<string, object>(data.Values)
                };

                data.Values["Type"] = spinnerColor == Color.Rainbow ? "RainbowClassic" : "White";
                data.Values["ref"] = "danger/crystal/fg_" + getColorName(spinnerColor) + "00";
                data.Values["Color"] = "ffffff";
                data.Values["ShatterColor"] = getDebrisColor(spinnerColor);
                data.Values["BorderColor"] = "000000";
            }

            orig(self, data, offset);
        }

        private static string getColorName(Color color) {
            switch (color) {
                case Color.Red: return "red";
                case Color.Purple: return "purple";
                case Color.Blue: return "blue";
                default: return "white";
            }
        }

        private static string getDebrisColor(Color color) {
            switch (color) {
                case Color.Red: return "ff4f4f";
                case Color.Purple: return "ff4fef";
                case Color.Blue: return "639bff";
                default: return "ffffff";
            }
        }
    }
}
