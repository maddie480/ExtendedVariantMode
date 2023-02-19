using Celeste;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class PreserveExtraDashesUnderwater : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return true;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Player.SwimUpdate += onSwimUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.SwimUpdate -= onSwimUpdate;
        }

        private int onSwimUpdate(On.Celeste.Player.orig_SwimUpdate orig, Player self) {
            int result = orig(self);

            if (!GetVariantValue<bool>(Variant.PreserveExtraDashesUnderwater) && result == Player.StDash && self.Dashes > self.MaxDashes) {
                // if we're dashing and have more than our dash count, consume a dash!
                self.Dashes--;
            }

            return result;
        }
    }
}
