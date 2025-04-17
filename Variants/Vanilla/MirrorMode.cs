using Celeste;
using ExtendedVariants.Module;
using System;

namespace ExtendedVariants.Variants.Vanilla {
    public class MirrorMode : AbstractVanillaVariant {
        public MirrorMode() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override void Load() {
            On.Celeste.Level.BeforeRender += Level_BeforeRender;
            On.Celeste.Level.AfterRender += Level_AfterRender;
        }

        public override void Unload() {
            On.Celeste.Level.BeforeRender -= Level_BeforeRender;
            On.Celeste.Level.AfterRender -= Level_AfterRender;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void VariantValueChanged() {
            bool mirrorMode = getActiveAssistValues().MirrorMode;
            Input.MoveX.Inverted = (Input.Aim.InvertedX = (Input.Feather.InvertedX = mirrorMode));
        }

        protected override Assists applyVariantValue(Assists target, object value) {
            target.MirrorMode = (bool) value;
            return target;
        }


        private static bool previousMirrorModeValue;

        private static void Level_BeforeRender(On.Celeste.Level.orig_BeforeRender orig, Level self) {
            // Some entities (CoreMessage for example) check for MirrorMode in their Render function,
            // we need to ensure that they still render correctly even if we have a level specific MirrorMode.
            previousMirrorModeValue = SaveData.Instance.Assists.MirrorMode;
            bool isMirrorModeActive = (bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.MirrorMode)
                                      || SaveData.Instance.Assists.MirrorMode;

            SaveData.Instance.Assists.MirrorMode = isMirrorModeActive;

            orig(self);
        }

        private static void Level_AfterRender(On.Celeste.Level.orig_AfterRender orig, Level self) {
            SaveData.Instance.Assists.MirrorMode = previousMirrorModeValue;

            orig(self);
        }
    }
}
