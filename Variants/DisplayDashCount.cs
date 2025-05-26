using ExtendedVariants.Entities;
using System;
using System.Linq;

namespace ExtendedVariants.Variants {
    public class DisplayDashCount : AbstractExtendedVariant {

        public DisplayDashCount() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Celeste.Level self, Celeste.Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (!self.Entities.Any(entity => entity is DashCountIndicator && !(entity is Speedometer))) {
                // add the entity showing the dash count (it will be invisible unless the option is enabled)
                self.Add(new DashCountIndicator());
                self.Entities.UpdateLists();
            }
        }
    }
}
