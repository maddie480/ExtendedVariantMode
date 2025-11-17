using System;
using System.Runtime.InteropServices.ComTypes;
using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace ExtendedVariants.Variants {
    public class AutoDash : AbstractExtendedVariant {
        public AutoDash() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Player.Update += Player_Update;
        }

        public override void Unload() {
            On.Celeste.Player.Update -= Player_Update;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.AutoDash))
                return;

            if (!IsValidDashState(self.StateMachine.State))
                return;

            ForceDash(self);
        }

        private static bool IsValidDashState(int currentState)
            => currentState is Player.StNormal or Player.StRedDash or Player.StStarFly or Player.StSwim or Player.StClimb or Player.StBoost;

        private static void ForceDash(Player self) {
            DynamicData selfData = DynamicData.For(self);

            if (selfData.Get<float>("dashCooldownTimer") <= 0f && self.Dashes > 0 && (TalkComponent.PlayerOver == null || !Input.Talk.Pressed)) {
                if (self.LastBooster != null && self.LastBooster.Ch9HubTransition && self.LastBooster.BoostingPlayer)
                    return;

                if (self.CurrentBooster != null) {
                    if (selfData.Get<bool>("boostRed")) self.StateMachine.State = Player.StRedDash;
                    else self.StateMachine.State = Player.StDash;
                } else {
                    self.StateMachine.State = self.StartDash();
                }
            }
        }
    }
}
