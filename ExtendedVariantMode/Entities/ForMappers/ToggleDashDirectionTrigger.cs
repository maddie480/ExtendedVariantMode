using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/ToggleDashDirectionTrigger")]
    public class ToggleDashDirectionTrigger : Trigger {
        private int dashDirection;
        private bool enable;
        private bool revertOnLeave;
        private bool revertOnDeath;
        private bool[,] oldValueToRevertOnLeave;

        // we are using bit fields for backwards compatibility, but this can otherwise be seen as an enum.
        private const int TOP = 0b1000000000;
        private const int TOP_RIGHT = 0b0100000000;
        private const int RIGHT = 0b0010000000;
        private const int BOTTOM_RIGHT = 0b0001000000;
        private const int BOTTOM = 0b0000100000;
        private const int BOTTOM_LEFT = 0b0000010000;
        private const int LEFT = 0b0000001000;
        private const int TOP_LEFT = 0b0000000100;

        public ToggleDashDirectionTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            // parse the trigger parameters
            dashDirection = data.Int("dashDirection", TOP);
            enable = data.Bool("enable", true);
            revertOnLeave = data.Bool("revertOnLeave", false);
            revertOnDeath = data.Bool("revertOnDeath", true);

            // failsafe
            oldValueToRevertOnLeave = ExtendedVariantsModule.Settings.AllowedDashDirections;
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            bool[,] newValue = ExtendedVariantsModule.Settings.AllowedDashDirections;

            int x = 0, y = 0;
            switch (dashDirection) {
                case TOP:           x = 1; y = 0; break;
                case TOP_RIGHT:     x = 2; y = 0; break;
                case RIGHT:         x = 2; y = 1; break;
                case BOTTOM_RIGHT:  x = 2; y = 2; break;
                case BOTTOM:        x = 1; y = 2; break;
                case BOTTOM_LEFT:   x = 0; y = 2; break;
                case LEFT:          x = 0; y = 1; break;
                case TOP_LEFT:      x = 0; y = 0; break;
            }

            newValue[x, y] = enable;

            bool[,] oldValue = (bool[,]) ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(ExtendedVariantsModule.Variant.DashDirection, newValue, revertOnLeave, isFade: false, revertOnDeath, legacy: false);

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
