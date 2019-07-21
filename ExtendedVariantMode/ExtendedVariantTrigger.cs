using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ExtendedVariants {
    public class ExtendedVariantTrigger : Trigger {
        private Variant variantChange;
        private int newValue;

        public ExtendedVariantTrigger(EntityData data, Vector2 offset): base(data, offset) {
            // parse the trigger parameters
            variantChange = data.Enum("variantChange", Variant.Gravity);
            newValue = data.Int("newValue", 10);

            if (!data.Bool("enable", true)) {
                // "disabling" a variant is actually just resetting its value to default
                // (most of the time, the default value is 10)
                switch(variantChange) {
                    case Variant.Stamina: newValue = 11; break;
                    case Variant.DashCount: newValue = -1; break;
                    default: newValue = 10; break;
                }
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            // change the variant value
            int oldValue = SetVariantValue(variantChange, newValue);

            // store the fact that the variant was changed within the room
            // so that it can be reverted if we die, or saved if we save & quit later
            if (oldValue != newValue) {
                Logger.Log("ExtendedVariantTrigger", $"Triggered ExtendedVariantTrigger: changed {variantChange} from {oldValue} to {newValue}");

                if (!ExtendedVariantsModule.OldVariantsInRoom.ContainsKey(variantChange)) {
                    ExtendedVariantsModule.OldVariantsInRoom.Add(variantChange, oldValue);
                }
                if (!ExtendedVariantsModule.OldVariantsInSession.ContainsKey(variantChange)) {
                    ExtendedVariantsModule.OldVariantsInSession.Add(variantChange, oldValue);
                }
                ExtendedVariantsModule.OverridenVariantsInRoom.Add(variantChange, newValue);
            }
        }

        /// <summary>
        /// Sets a variant value.
        /// </summary>
        /// <param name="variantChange">The variant to change</param>
        /// <param name="newValue">The new value</param>
        /// <returns>The old value for this variant</returns>
        public static int SetVariantValue(Variant variantChange, int newValue) {
            int oldValue = 0;

            switch(variantChange) {
                case Variant.Gravity:
                    oldValue = ExtendedVariantsModule.Settings.Gravity;
                    ExtendedVariantsModule.Settings.Gravity = newValue;
                    break;
                case Variant.JumpHeight:
                    oldValue = ExtendedVariantsModule.Settings.JumpHeight;
                    ExtendedVariantsModule.Settings.JumpHeight = newValue;
                    break;
                case Variant.SpeedX:
                    oldValue = ExtendedVariantsModule.Settings.SpeedX;
                    ExtendedVariantsModule.Settings.SpeedX = newValue;
                    break;
                case Variant.Stamina:
                    oldValue = ExtendedVariantsModule.Settings.Stamina;
                    ExtendedVariantsModule.Settings.Stamina = newValue;
                    break;
                case Variant.DashSpeed:
                    oldValue = ExtendedVariantsModule.Settings.DashSpeed;
                    ExtendedVariantsModule.Settings.DashSpeed = newValue;
                    break;
                case Variant.DashCount:
                    oldValue = ExtendedVariantsModule.Settings.DashCount;
                    ExtendedVariantsModule.Settings.DashCount = newValue;
                    break;
                case Variant.Friction:
                    oldValue = ExtendedVariantsModule.Settings.Friction;
                    ExtendedVariantsModule.Settings.Friction = newValue;
                    break;
            }

            return oldValue;
        }
    }

    public enum Variant {
        Gravity, JumpHeight, SpeedX, Stamina, DashSpeed, DashCount, Friction
    }
}
