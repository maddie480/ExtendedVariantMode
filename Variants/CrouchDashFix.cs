using System;
using Celeste;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace ExtendedVariants.Variants;

public class CrouchDashFix : AbstractExtendedVariant {
    public override void Load() => IL.Celeste.Player.DashBegin += Player_DashBegin_il;

    public override void Unload() => IL.Celeste.Player.DashBegin -= Player_DashBegin_il;

    public override Type GetVariantType() => typeof(bool);

    public override object GetDefaultVariantValue() => false;

    public override object ConvertLegacyVariantValue(int value) => value != 0;

    private void Player_DashBegin_il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(instr => instr.MatchCallvirt<Player>("set_Ducking"));
        cursor.GotoPrev(MoveType.After,
            instr => instr.OpCode == OpCodes.Ldarg_0,
            instr => instr.MatchLdfld<Player>("onGround"));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Func<bool, Player, bool>>((onGround, player) => {
            if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.CrouchDashFix))
                return onGround;

            return Input.MoveY.Value == 1 || DynamicData.For(player).Get<bool>("demoDashed");
        });
    }
}