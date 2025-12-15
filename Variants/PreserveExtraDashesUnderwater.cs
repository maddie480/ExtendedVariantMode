using Celeste;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class PreserveExtraDashesUnderwater : AbstractExtendedVariant {

        public PreserveExtraDashesUnderwater() : base(variantType: typeof(bool), defaultVariantValue: true) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Player.SwimUpdate += onSwimUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.SwimUpdate -= onSwimUpdate;
        }

        private static int onSwimUpdate(On.Celeste.Player.orig_SwimUpdate orig, Player self) {
            int result = orig(self);

            if (!GetVariantValue<bool>(Variant.PreserveExtraDashesUnderwater)
                    && result == Player.StDash
                    && (self.Dashes > self.MaxDashes || (self.Inventory.NoRefills && self.Dashes > 0))) {
                // consume a dash, if we're dashing and...
                // - have more than our dash count
                // - core mode is enabled and we have dashes
                self.Dashes--;
            }

            return result;
        }
    }
}
