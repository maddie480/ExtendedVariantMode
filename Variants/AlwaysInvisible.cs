
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class AlwaysInvisible : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }
        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.PlayerSeeker.Render += modPlayerSeekerRender;
            On.Celeste.Player.Render += modPlayerRender;
        }

        public override void Unload() {
            On.Celeste.PlayerSeeker.Render -= modPlayerSeekerRender;
            On.Celeste.Player.Render -= modPlayerRender;
        }

        // vanilla implements Invisible Motion by skipping the Render method entirely when the player is moving... so we're just going to do the same. :p

        private void modPlayerSeekerRender(On.Celeste.PlayerSeeker.orig_Render orig, Celeste.PlayerSeeker self) {
            if (!GetVariantValue<bool>(Variant.AlwaysInvisible)) {
                orig(self);
            }
        }

        private void modPlayerRender(On.Celeste.Player.orig_Render orig, Celeste.Player self) {
            if (!GetVariantValue<bool>(Variant.AlwaysInvisible)) {
                orig(self);
            }
        }
    }
}
