using System;
using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using Monocle;

namespace ExtendedVariants.Variants {
    public class AutoJump : AbstractExtendedVariant {
        private static float JumpRefillTimer = 0f;
        private static bool JumpFrameDelay = true;

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
            On.Celeste.Player.Update += Player_Update;
        }

        public override void Unload() {
            On.Celeste.Player.Update -= Player_Update;
        }

        private void Player_Update(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.AutoJump))
                return;

            if (JumpRefillTimer > 0f) JumpRefillTimer -= Engine.DeltaTime;

            if (!IsValidJumpState(self.StateMachine.State))
                return;

            ForceJump(self);
        }

        private static bool IsValidJumpState(int currentState)
            => currentState is Player.StNormal or Player.StDash or Player.StRedDash or Player.StStarFly or Player.StSwim;

        private void ForceJump(Player self) {
            if (!JumpFrameDelay) {
                JumpFrameDelay = true;
                return;
            }

            DynamicData selfData = DynamicData.For(self);

            bool canUnDuck = self.CanUnDuck;
            if (!canUnDuck)
                return;

            if (selfData.Invoke<bool>("WallJumpCheck", 1)) {
                if (self.DashAttacking && selfData.Invoke<bool>("get_SuperWallJumpAngleCheck")) {
                    selfData.Invoke("SuperWallJump", -1);
                    SetAutoJump(self);
                } else {
                    selfData.Invoke("WallJump", -1);
                    SetAutoJump(self);
                }
                JumpFrameDelay = false;
            } else if (selfData.Invoke<bool>("WallJumpCheck", -1)) {
                if (self.DashAttacking && selfData.Invoke<bool>("get_SuperWallJumpAngleCheck")) {
                    selfData.Invoke("SuperWallJump", 1);
                    SetAutoJump(self);
                } else {
                    selfData.Invoke("WallJump", 1);
                    SetAutoJump(self);
                }
                JumpFrameDelay = false;
            } else {
                if (self.AutoJumpTimer > 0f && (JumpCount.GetJumpBuffer() == 0)) return;

                if (self.CollideFirst<Water>(self.Position + Vector2.UnitY * 2f) is { } water && selfData.Invoke<bool>("SwimJumpCheck")) {
                    self.Jump();
                    water.TopSurface.DoRipple(self.Position, 1f);
                    SetAutoJump(self);
                    JumpFrameDelay = false;
                }

                if (self.OnGround() && self.Speed.Y >= 0 || (JumpCount.GetJumpBuffer() > 0 && JumpRefillTimer <= 0f)) {
                    if ((self.StateMachine.State is Player.StDash or Player.StRedDash) && self.CanUnDuck && self.DashDir != new Vector2(0, 1)) {
                        if (self.DashDir.Y < 0) return;

                        if (self.DashDir.Y != 0) self.Ducking = true;
                        selfData.Invoke("SuperJump");
                        SetAutoJump(self);
                    } else {
                        self.Jump();
                        SetAutoJump(self);
                    }
                    if (JumpCount.GetJumpBuffer() > 0) {
                        JumpCount.SetJumpCount(JumpCount.GetJumpBuffer() - 1, false);
                        JumpRefillTimer = GetVariantValue<float>(ExtendedVariantsModule.Variant.JumpCooldown);
                    } else {
                        if (!self.Inventory.NoRefills && selfData.Get<float>("dashRefillCooldownTimer") < 0f) self.RefillDash();
                        self.RefillStamina();
                    }
                    return;
                }
            }
        }

        private static void SetAutoJump(Player self, float duration = 0.4f, bool resetState = true) {
            self.AutoJump = true;
            self.AutoJumpTimer = duration;
            if (resetState) self.StateMachine.State = Player.StNormal;
        }
    }
}
