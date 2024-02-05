using Celeste;
using ExtendedVariants.Module;
using System;

namespace ExtendedVariants.Variants {
    public class PermanentBinoStorage : AbstractExtendedVariant {
        public override void Load() {
            On.Celeste.Player.OnTransition += Player_OnTransition;
        }

        public override void Unload() {
            On.Celeste.Player.OnTransition -= Player_OnTransition;
        }

        private void Player_OnTransition(On.Celeste.Player.orig_OnTransition orig, Player self) {
            if ((bool)ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.PermanentBinoStorage)) {
                self.StateMachine.State = Player.StNormal;
            }
            orig(self);
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override Type GetVariantType() {
            return typeof(bool);
        }
    }
}
