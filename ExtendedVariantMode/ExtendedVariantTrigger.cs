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
        private bool revertOnLeave;
        private int oldValueToRevertOnLeave;

        public ExtendedVariantTrigger(EntityData data, Vector2 offset): base(data, offset) {
            // parse the trigger parameters
            variantChange = data.Enum("variantChange", Variant.Gravity);
            newValue = data.Int("newValue", 10);
            revertOnLeave = data.Bool("revertOnLeave", false);

            if (!data.Bool("enable", true)) {
                // "disabling" a variant is actually just resetting its value to default
                // (most of the time, the default value is 10)
                switch(variantChange) {
                    case Variant.Stamina: newValue = 11; break;

                    case Variant.DashCount:
                    case Variant.RoomLighting:
                        newValue = -1; break;

                    case Variant.DisableWallJumping:
                    case Variant.UpsideDown:
                    case Variant.ForceDuckOnGround:
                    case Variant.InvertDashes:
                    case Variant.DisableNeutralJumping:
                    case Variant.ChangeVariantsRandomly:
                    case Variant.ChangeVariantsInterval:
                    case Variant.BadelineChasersEverywhere:
                    case Variant.AffectExistingChasers:
                    case Variant.RegularHiccups:
                    case Variant.RefillJumpsOnDashRefill:
                    case Variant.DoNotRandomizeInvincibility:
                        newValue = 0; break;

                    case Variant.JumpCount:
                    case Variant.ChaserCount:
                        newValue = 1; break;

                    default: newValue = 10; break;
                }
            }

            // failsafe
            oldValueToRevertOnLeave = newValue;
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            // change the variant value
            int oldValue = SetVariantValue(variantChange, newValue);

            // store the fact that the variant was changed within the room
            // so that it can be reverted if we die, or saved if we save & quit later
            if (oldValue != newValue) {
                Logger.Log("ExtendedVariantTrigger", $"Triggered ExtendedVariantTrigger: changed {variantChange} from {oldValue} to {newValue} (revertOnLeave = {revertOnLeave})");

                if (!ExtendedVariantsModule.OldVariantsInRoom.ContainsKey(variantChange)) {
                    ExtendedVariantsModule.OldVariantsInRoom[variantChange] = oldValue;
                }
                if (!ExtendedVariantsModule.OldVariantsInSession.ContainsKey(variantChange)) {
                    ExtendedVariantsModule.OldVariantsInSession[variantChange] = oldValue;
                }
                if (revertOnLeave) {
                    // save the value we will revert when leaving the trigger
                    oldValueToRevertOnLeave = oldValue;
                } else { // we don't want the value to be committed when leaving the room, since this is temporary
                    ExtendedVariantsModule.OverridenVariantsInRoom[variantChange] = newValue;
                }
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if(revertOnLeave) {
                SetVariantValue(variantChange, oldValueToRevertOnLeave);
                Logger.Log("ExtendedVariantTrigger", $"Left ExtendedVariantTrigger: reverted {variantChange} to {oldValueToRevertOnLeave}");
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
                case Variant.FallSpeed:
                    oldValue = ExtendedVariantsModule.Settings.FallSpeed;
                    ExtendedVariantsModule.Settings.FallSpeed = newValue;
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
                case Variant.DisableWallJumping:
                    oldValue = ExtendedVariantsModule.Settings.DisableWallJumping ? 1 : 0;
                    ExtendedVariantsModule.Settings.DisableWallJumping = (newValue != 0);
                    break;
                case Variant.JumpCount:
                    oldValue = ExtendedVariantsModule.Settings.JumpCount;
                    ExtendedVariantsModule.Settings.JumpCount = newValue;
                    break;
                case Variant.UpsideDown:
                    oldValue = ExtendedVariantsModule.Settings.UpsideDown ? 1 : 0;
                    ExtendedVariantsModule.Settings.UpsideDown = (newValue != 0);
                    break;
                case Variant.HyperdashSpeed:
                    oldValue = ExtendedVariantsModule.Settings.HyperdashSpeed;
                    ExtendedVariantsModule.Settings.HyperdashSpeed = newValue;
                    break;
                case Variant.WallBouncingSpeed:
                    oldValue = ExtendedVariantsModule.Settings.WallBouncingSpeed;
                    ExtendedVariantsModule.Settings.WallBouncingSpeed = newValue;
                    break;
                case Variant.DashLength:
                    oldValue = ExtendedVariantsModule.Settings.DashLength;
                    ExtendedVariantsModule.Settings.DashLength = newValue;
                    break;
                case Variant.ForceDuckOnGround:
                    oldValue = ExtendedVariantsModule.Settings.ForceDuckOnGround ? 1 : 0;
                    ExtendedVariantsModule.Settings.ForceDuckOnGround = (newValue != 0);
                    break;
                case Variant.InvertDashes:
                    oldValue = ExtendedVariantsModule.Settings.InvertDashes ? 1 : 0;
                    ExtendedVariantsModule.Settings.InvertDashes = (newValue != 0);
                    break;
                case Variant.DisableNeutralJumping:
                    oldValue = ExtendedVariantsModule.Settings.DisableNeutralJumping ? 1 : 0;
                    ExtendedVariantsModule.Settings.DisableNeutralJumping = (newValue != 0);
                    break;
                case Variant.BadelineChasersEverywhere:
                    oldValue = ExtendedVariantsModule.Settings.BadelineChasersEverywhere ? 1 : 0;
                    ExtendedVariantsModule.Settings.BadelineChasersEverywhere = (newValue != 0);
                    break;
                case Variant.ChaserCount:
                    oldValue = ExtendedVariantsModule.Settings.ChaserCount;
                    ExtendedVariantsModule.Settings.ChaserCount = newValue;
                    break;
                case Variant.AffectExistingChasers:
                    oldValue = ExtendedVariantsModule.Settings.AffectExistingChasers ? 1 : 0;
                    ExtendedVariantsModule.Settings.AffectExistingChasers = (newValue != 0);
                    break;
                case Variant.ChangeVariantsRandomly:
                    oldValue = ExtendedVariantsModule.Settings.ChangeVariantsRandomly;
                    ExtendedVariantsModule.Settings.ChangeVariantsRandomly = newValue;
                    break;
                case Variant.ChangeVariantsInterval:
                    oldValue = ExtendedVariantsModule.Settings.ChangeVariantsInterval;
                    ExtendedVariantsModule.Settings.ChangeVariantsInterval = newValue;
                    break;
                case Variant.RegularHiccups:
                    oldValue = ExtendedVariantsModule.Settings.RegularHiccups;
                    ExtendedVariantsModule.Settings.RegularHiccups = newValue;
                    break;
                case Variant.RefillJumpsOnDashRefill:
                    oldValue = ExtendedVariantsModule.Settings.RefillJumpsOnDashRefill ? 1 : 0;
                    ExtendedVariantsModule.Settings.RefillJumpsOnDashRefill = (newValue != 0);
                    break;
                case Variant.DoNotRandomizeInvincibility:
                    oldValue = ExtendedVariantsModule.Settings.DoNotRandomizeInvincibility ? 1 : 0;
                    ExtendedVariantsModule.Settings.DoNotRandomizeInvincibility = (newValue != 0);
                    break;
                case Variant.RoomLighting:
                    oldValue = ExtendedVariantsModule.Settings.RoomLighting;
                    ExtendedVariantsModule.Settings.RoomLighting = newValue;
                    break;
            }

            return oldValue;
        }
    }

    public enum Variant {
        Gravity, FallSpeed, JumpHeight, SpeedX, Stamina, DashSpeed, DashCount, Friction, DisableWallJumping, JumpCount, UpsideDown, HyperdashSpeed, WallBouncingSpeed,
        DashLength, ForceDuckOnGround, InvertDashes, DisableNeutralJumping, ChangeVariantsRandomly, ChangeVariantsInterval, BadelineChasersEverywhere, ChaserCount, AffectExistingChasers,
        RegularHiccups, RefillJumpsOnDashRefill, DoNotRandomizeInvincibility, RoomLighting
    }
}
