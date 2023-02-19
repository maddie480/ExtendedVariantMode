using ExtendedVariants.Entities;
using System;
using System.Linq;

namespace ExtendedVariants.Variants {
    public class DisplaySpeedometer : AbstractExtendedVariant {
        public enum SpeedometerConfiguration { DISABLED, HORIZONTAL, VERTICAL, BOTH }

        public override Type GetVariantType() {
            return typeof(SpeedometerConfiguration);
        }

        public override object GetDefaultVariantValue() {
            return SpeedometerConfiguration.DISABLED;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return (SpeedometerConfiguration) value;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Celeste.Level self, Celeste.Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (!self.Entities.Any(entity => entity is Speedometer)) {
                // add the entity showing the speedometer (it will be invisible unless the option is enabled)
                self.Add(new Speedometer());
                self.Entities.UpdateLists();
            }
        }
    }
}
