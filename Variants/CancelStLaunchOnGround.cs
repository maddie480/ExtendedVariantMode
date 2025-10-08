using System;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;

using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants;

public class CancelStLaunchOnGround : AbstractExtendedVariant
{
    private readonly FieldInfo f_Player_onGround = typeof(Player)
        .GetField("onGround", BindingFlags.NonPublic | BindingFlags.Instance);

    public CancelStLaunchOnGround() : base(variantType: typeof(bool), defaultVariantValue: false) { }

    public override object ConvertLegacyVariantValue(int value) => value != 0;

    public override void Load() => IL.Celeste.Player.LaunchUpdate += ILPlayerLaunchUpdate;

    public override void Unload() => IL.Celeste.Player.LaunchUpdate -= ILPlayerLaunchUpdate;

    private void ILPlayerLaunchUpdate(ILContext il)
    {
        ILCursor cur = new(il);

        if (f_Player_onGround is null)
            throw new Exception("Private field player.onGround hasn't been found!");

        Logger.Log(LogLevel.Info, $"ExtendedVariantMode/{nameof(CancelStLaunchOnGround)}",
                "Patching IL for Player.LaunchUpdate to add grounded condition"
            );

        /*
             GroundedCheck(this.onGround) ||
             vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
         if (                                Speed.Length() < 220f)
             return 0; // StNormal
         
         return 7; // StLaunch
         */
        ILLabel returnStNormalLabel = cur.DefineLabel();

        cur.GotoNext(MoveType.AfterLabel,
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdflda<Player>(nameof(Player.Speed)),
                instr => instr.MatchCall<Vector2>(nameof(Vector2.Length))
            ); // search forward for this.Speed.Length()

        // Clone the cursor
        ILCursor clone = cur.Clone();
        clone.GotoNext(MoveType.AfterLabel, instr => instr.MatchLdcI4(0)); // search forward for 0 (StNormal)
        clone.MarkLabel(returnStNormalLabel); // mark label here

        // Back to the original cursor
        cur.Emit(OpCodes.Ldarg_0); // this
        cur.Emit(OpCodes.Ldfld, f_Player_onGround); // this.onGround
        cur.EmitDelegate(GroundedCheck); // GroundedCheck(this.onGround)
        cur.Emit(OpCodes.Brtrue_S, returnStNormalLabel); // go to that label if the check returns true

        // Since we can't access private members of base game classes with this mod,
        // we have to pass player.onGround as an argument.
        bool GroundedCheck(bool player_onGround)
            => GetVariantValue<bool>(Variant.CancelStLaunchOnGround) && player_onGround;
    }
}
