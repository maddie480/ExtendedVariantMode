using Celeste;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class PermanentBinoStorage : AbstractExtendedVariant {
        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public PermanentBinoStorage() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override void Load() {
            On.Celeste.Player.OnTransition += Player_OnTransition;
        }

        public override void Unload() {
            On.Celeste.Player.OnTransition -= Player_OnTransition;
        }

        private static void Player_OnTransition(On.Celeste.Player.orig_OnTransition orig, Player self) {
            if (GetVariantValue<bool>(Variant.PermanentBinoStorage)) {
                self.StateMachine.State = Player.StNormal;
            }

            orig(self);
        }

    }
}
