using Celeste.Mod;
using MonoMod.Cil;
using System;
using Monocle;
using Microsoft.Xna.Framework;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class NoFreezeFrames : AbstractExtendedVariant {

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
            On.Monocle.Engine.Update += onEngineUpdate;
            On.Celeste.Celeste.Freeze += onCelesteFreeze;
        }

        public override void Unload() {
            On.Monocle.Engine.Update -= onEngineUpdate;
            On.Celeste.Celeste.Freeze -= onCelesteFreeze;
        }

        private void onCelesteFreeze(On.Celeste.Celeste.orig_Freeze orig, float time) {
            if (GetVariantValue<bool>(Variant.NoFreezeFrames)) {
                return;
            }
            orig(time);
        }

        private void onEngineUpdate(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime) {
            if (GetVariantValue<bool>(Variant.NoFreezeFrames)) {
                Engine.FreezeTimer = 0f;
            }

            orig(self, gameTime);
        }
    }
}
