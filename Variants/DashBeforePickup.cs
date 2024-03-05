using System;
using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace ExtendedVariants.Variants;

public class DashBeforePickup : AbstractExtendedVariant {
    public override void Load() => IL.Celeste.Player.NormalUpdate += Player_NormalUpdate_il;

    public override void Unload() => IL.Celeste.Player.NormalUpdate -= Player_NormalUpdate_il;

    public override Type GetVariantType() => typeof(bool);

    public override object GetDefaultVariantValue() => false;

    public override object ConvertLegacyVariantValue(int value) => value != 0;

    private void Player_NormalUpdate_il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After,
            instr => instr.MatchCallvirt<Player>("get_Holding"),
            instr => instr.OpCode == OpCodes.Brtrue);

        var label = cursor.DefineLabel();

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Func<Player, bool>>(player => {
            if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.DashBeforePickup) || !player.CanDash)
                return false;

            player.Speed += DynamicData.For(player).Invoke<Vector2>("get_LiftBoost");
            player.StartDash();

            return true;
        });
        cursor.Emit(OpCodes.Brfalse_S, label);
        cursor.Emit(OpCodes.Ldc_I4_2);
        cursor.Emit(OpCodes.Ret);
        cursor.MarkLabel(label);

        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("get_CanDash"));

        cursor.EmitDelegate<Func<bool, bool>>(value => value && !GetVariantValue<bool>(ExtendedVariantsModule.Variant.DashBeforePickup));
    }
}