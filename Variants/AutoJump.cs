using System;
using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using static Celeste.TrackSpinner;

namespace ExtendedVariants.Variants;

public class AutoJump : AbstractExtendedVariant {
    public override Type GetVariantType() {
        return typeof(bool);
    }

    public override object GetDefaultVariantValue() {
        return false;
    }

    public override object ConvertLegacyVariantValue(int value) {
        return value != 0;
    }

    public override void Load()
    {
        On.Celeste.Player.Update += Player_Update;
    }

    public override void Unload()
    {
        On.Celeste.Player.Update -= Player_Update;
    }

    private void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.AutoJump))
            return;

        if (!IsValidJumpState(self.StateMachine.State))
            return;

        ForceJump(self);
    }

    private static bool IsValidJumpState(int currentState)
        => currentState is Player.StNormal or Player.StDash or Player.StRedDash or Player.StStarFly or Player.StSwim;

    private static void ForceJump(Player self)
    {
        DynamicData selfData = DynamicData.For(self);

        bool canUnDuck = self.CanUnDuck;
        if (!canUnDuck)
            return;

        if (selfData.Invoke<bool>("WallJumpCheck", 1))
        {
            if (self.DashAttacking && selfData.Invoke<bool>("get_SuperWallJumpAngleCheck"))
            {
                selfData.Invoke("SuperWallJump", -1);
                SetAutoJump(self, 0.25f);
            }
            else
            {
                selfData.Invoke("WallJump", -1);
                SetAutoJump(self);
            }
        }
        else if (selfData.Invoke<bool>("WallJumpCheck", -1))
        {
            if (self.DashAttacking && selfData.Invoke<bool>("get_SuperWallJumpAngleCheck"))
            {
                selfData.Invoke("SuperWallJump", 1);
                SetAutoJump(self);
            }
            else
            {
                selfData.Invoke("WallJump", 1);
                SetAutoJump(self);
            }
        }
        else
        {
            if (self.AutoJumpTimer > 0f) return;

            if (self.CollideFirst<Water>(self.Position + Vector2.UnitY * 2f) is { } water && selfData.Invoke<bool>("SwimJumpCheck"))
            {
                self.Jump();
                water.TopSurface.DoRipple(self.Position, 1f);
                SetAutoJump(self);
            }

            if (self.OnGround())
            {
                if ((self.StateMachine.State is Player.StDash or Player.StRedDash) && self.CanUnDuck && self.DashDir.X != 0)
                {
                    if(self.DashDir.Y == 1f && self.StateMachine.State is Player.StDash) self.Ducking = true;
                    selfData.Invoke("SuperJump");
                    SetAutoJump(self);
                }
                else
                {
                    self.Jump();
                    SetAutoJump(self);
                }
                self.RefillDash();
                self.RefillStamina();

                return;
            }
        }
    }

    private static void SetAutoJump(Player self, float duration = 0.5f, bool resetState = true)
    {
        self.AutoJump = true;
        self.AutoJumpTimer = duration;
        if (resetState) self.StateMachine.State = Player.StNormal;
    }
}
