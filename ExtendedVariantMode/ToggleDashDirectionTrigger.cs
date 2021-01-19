using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using System;

namespace ExtendedVariants {
    [CustomEntity("ExtendedVariantMode/ToggleDashDirectionTrigger")]
    public class ToggleDashDirectionTrigger : Trigger {
        private int dashDirection;
        private bool enable;
        private bool revertOnLeave;
        private bool revertOnDeath;
        private int oldValueToRevertOnLeave;

        public ToggleDashDirectionTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            // parse the trigger parameters
            dashDirection = data.Int("dashDirection", Variants.DashDirection.TOP);
            enable = data.Bool("enable", true);
            revertOnLeave = data.Bool("revertOnLeave", false);
            revertOnDeath = data.Bool("revertOnDeath", true);

            // failsafe
            oldValueToRevertOnLeave = ExtendedVariantsModule.Settings.DashDirection;
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            int newValue = ExtendedVariantsModule.Settings.DashDirection;
            if (newValue == 0) {
                // all directions allowed
                newValue = 0b1111111111;
            } else if (newValue == 1) {
                // straight only
                newValue = 0b1010101011;
            } else if (newValue == 2) {
                // diagonal only
                newValue = 0b0101010111;
            }

            if (enable) {
                newValue |= dashDirection;
            } else {
                newValue &= (dashDirection ^ 0b1111111111);
            }

            Logger.Log("ExtendedVariantMode/ToggleDashDirectionTrigger", $"Old value was {ExtendedVariantsModule.Settings.DashDirection} / {Convert.ToString(ExtendedVariantsModule.Settings.DashDirection, 2)}, " +
                $"new value is {newValue} / {Convert.ToString(newValue, 2)}");

            int oldValue = ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(ExtendedVariantsModule.Variant.DashDirection, newValue, revertOnLeave, isFade: false, revertOnDeath);

            if (revertOnLeave) {
                oldValueToRevertOnLeave = oldValue;
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if (revertOnLeave) {
                ExtendedVariantsModule.Instance.TriggerManager.OnExitedRevertOnLeaveTrigger(ExtendedVariantsModule.Variant.DashDirection, oldValueToRevertOnLeave);
            }
        }
    }
}
