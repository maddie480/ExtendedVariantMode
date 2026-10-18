using System;
using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Variants;

public class InteractBeforeGrab : AbstractExtendedVariant {
    private static bool Player_ClimbCheck(On.Celeste.Player.orig_ClimbCheck climbCheck, Player player, int dir, int yAdd) =>
        climbCheck(player, dir, yAdd) && !(GetVariantValue<bool>(ExtendedVariantsModule.Variant.InteractBeforeGrab)
                                           && player.StateMachine.State == Player.StNormal
                                           && player.DashAttacking
                                           && Math.Sign(player.Speed.X) == dir
                                           && player.CollideFirst<Solid>(player.Position + new Vector2(2f * dir, yAdd))?.OnDashCollide is not null);

    public InteractBeforeGrab() : base(typeof(bool), false) { }

    public override object ConvertLegacyVariantValue(int value) => value != 0;

    public override void Load() => On.Celeste.Player.ClimbCheck += Player_ClimbCheck;

    public override void Unload() => On.Celeste.Player.ClimbCheck -= Player_ClimbCheck;
}