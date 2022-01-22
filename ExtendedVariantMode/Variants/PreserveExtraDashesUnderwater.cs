using Celeste;
using System;

namespace ExtendedVariants.Variants {
    public class PreserveExtraDashesUnderwater : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return true;
        }

        public override object GetVariantValue() {
            return Settings.PreserveExtraDashesUnderwater;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.PreserveExtraDashesUnderwater = (bool) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.PreserveExtraDashesUnderwater = (value != 0);
        }

        public override void Load() {
            On.Celeste.Player.SwimUpdate += onSwimUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.SwimUpdate -= onSwimUpdate;
        }

        private int onSwimUpdate(On.Celeste.Player.orig_SwimUpdate orig, Player self) {
            int result = orig(self);

            if (!Settings.PreserveExtraDashesUnderwater && result == Player.StDash && self.Dashes > self.MaxDashes) {
                // if we're dashing and have more than our dash count, consume a dash!
                self.Dashes--;
            }

            return result;
        }
    }
}
