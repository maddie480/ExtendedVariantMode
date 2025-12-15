using Celeste;
using MonoMod.RuntimeDetour;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class InvertHorizontalControls : AbstractExtendedVariant {

        public InvertHorizontalControls() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            // we want to be extra sure we're applied after Crow Control here.
            using (new DetourConfigContext(new DetourConfig("ExtendedVariantMode_AfterAll").WithPriority(int.MaxValue)).Use()) {
                On.Celeste.Level.Update += onLevelUpdate;
            }
        }

        public override void Unload() {
            On.Celeste.Level.Update -= onLevelUpdate;
        }

        private static void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            if (Input.Aim == null || Input.MoveX == null || SaveData.Instance?.Assists == null) {
                orig(self);
                return;
            }

            bool isMirrorModeActive = GetVariantValue<bool>(Variant.MirrorMode) || SaveData.Instance.Assists.MirrorMode;
            bool isInvertHorizontalControlsActive = GetVariantValue<bool>(Variant.InvertHorizontalControls);

            // this is vanilla behavior
            Input.Aim.InvertedX = isMirrorModeActive;

            // there may be Crow Control here. if so, it will mess with Input.Aim.InvertedX
            orig(self);

            // at this point, Input.Aim.InvertedX is either the vanilla value, or what Crow Control wants.
            // either way, we should keep it or invert it based on our settings.

            bool expectedValue = Input.Aim.InvertedX ^ isInvertHorizontalControlsActive;

            Input.Aim.InvertedX = expectedValue;
            Input.MoveX.Inverted = expectedValue;
            Input.Feather.InvertedX = expectedValue;
        }
    }
}
