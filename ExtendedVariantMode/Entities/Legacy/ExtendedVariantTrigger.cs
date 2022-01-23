using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using System;

namespace ExtendedVariants.Entities.Legacy {
    // legacy extended variant trigger that can set any variant by taking an integer as an input.
    // this used tricks such as multiplying by 10 for multipliers, using a list for color grades, or taking a bit field as an input for dash directions.
    // ... it was bad, and is obsoleted by the new typed extended variant triggers.
    [CustomEntity("ExtendedVariantTrigger", "ExtendedVariantMode/ExtendedVariantTrigger")]
    public class ExtendedVariantTrigger : Trigger {

        // === hook on teleport to sync up a variant change with a teleport
        // since all teleport triggers call UnloadLevel(), we can hook that to detect the instant the teleport happens at.

        private static event Action onTeleport;

        public static void Load() {
            On.Celeste.Level.UnloadLevel += onUnloadLevel;
        }

        public static void Unload() {
            On.Celeste.Level.UnloadLevel -= onUnloadLevel;
            onTeleport = null;
        }

        private static void onUnloadLevel(On.Celeste.Level.orig_UnloadLevel orig, Level self) {
            if (onTeleport != null) {
                onTeleport();
                onTeleport = null;
            }

            orig(self);
        }

        // === extended variant trigger code

        private ExtendedVariantsModule.Variant variantChange;
        private object newValue;
        private bool revertOnLeave;
        private bool revertOnDeath;
        private object oldValueToRevertOnLeave;
        private bool withTeleport;
        private bool isLegacy = true;

        public ExtendedVariantTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            // parse the trigger parameters
            variantChange = data.Enum("variantChange", ExtendedVariantsModule.Variant.Gravity);
            newValue = data.Int("newValue", 10);
            revertOnLeave = data.Bool("revertOnLeave", false);
            revertOnDeath = data.Bool("revertOnDeath", true);
            withTeleport = data.Bool("withTeleport", false);

            if (!data.Bool("enable", true)) {
                // "disabling" a variant is actually just resetting its value to default
                isLegacy = false;
                newValue = ExtendedVariantTriggerManager.GetDefaultValueForVariant(variantChange);
            }

            // failsafe
            oldValueToRevertOnLeave = newValue;
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            Action applyVariant = () => {
                object oldValue = ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(variantChange, newValue, revertOnLeave, isFade: false, revertOnDeath, isLegacy);

                if (revertOnLeave) {
                    oldValueToRevertOnLeave = oldValue;
                }
            };

            if (withTeleport) {
                onTeleport += applyVariant;
            } else {
                applyVariant();
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if (revertOnLeave) {
                ExtendedVariantsModule.Instance.TriggerManager.OnExitedRevertOnLeaveTrigger(variantChange, oldValueToRevertOnLeave);
            }
        }
    }
}
