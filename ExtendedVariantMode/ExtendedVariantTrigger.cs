using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants {
    public class ExtendedVariantTrigger : Trigger {
        private ExtendedVariantsModule.Variant variantChange;
        private int newValue;
        private bool revertOnLeave;
        private int oldValueToRevertOnLeave;

        public ExtendedVariantTrigger(EntityData data, Vector2 offset): base(data, offset) {
            // parse the trigger parameters
            variantChange = data.Enum("variantChange", ExtendedVariantsModule.Variant.Gravity);
            newValue = data.Int("newValue", 10);
            revertOnLeave = data.Bool("revertOnLeave", false);

            if (!data.Bool("enable", true)) {
                // "disabling" a variant is actually just resetting its value to default
                newValue = getDefaultValueForVariant(variantChange);
            }

            // failsafe
            oldValueToRevertOnLeave = newValue;
        }

        private int getDefaultValueForVariant(ExtendedVariantsModule.Variant variant) {
            switch (variant) {
                case ExtendedVariantsModule.Variant.ChaserCount: return 1;
                case ExtendedVariantsModule.Variant.AffectExistingChasers: return 0;
                case ExtendedVariantsModule.Variant.HiccupStrength: return 10;
                case ExtendedVariantsModule.Variant.RefillJumpsOnDashRefill: return 0;
                case ExtendedVariantsModule.Variant.SnowballDelay: return 8;
                case ExtendedVariantsModule.Variant.BadelineLag: return 0;
                case ExtendedVariantsModule.Variant.DisableOshiroSlowdown: return 0;

                default: return ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultValue();
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            int oldValue = ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(variantChange, newValue, revertOnLeave);

            if(revertOnLeave) {
                oldValueToRevertOnLeave = oldValue;
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if(revertOnLeave) {
                ExtendedVariantsModule.Instance.TriggerManager.OnExitedRevertOnLeaveTrigger(variantChange, oldValueToRevertOnLeave);
            }
        }
    }
}
