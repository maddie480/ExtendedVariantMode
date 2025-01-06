using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants;

public class SlowfallGravityMultiplier : AbstractExtendedVariant
{
    public override Type GetVariantType()
    {
        return typeof(float);
    }

    public override object GetDefaultVariantValue()
    {
        return 0.5f;
    }
    public override object ConvertLegacyVariantValue(int value)
    {
        return value / 10f;
    }

    public override void Load()
    {
        IL.Celeste.Player.NormalUpdate += modNormalUpdate;
    }

    private void modNormalUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        //we try to find the place in NormalUpdate where it loads a 0.5f constant, then stores it in local var 12 
        //this is the constant that defines the initial gravity multiplier affected by slowfall
        //we're moving right before that pair of instructions
        if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdcR4(0.5f), instr => instr.MatchStloc(12)))
        {
            //move forward to after the 0.5f load but before the local var storage
            cursor.Index++;
            cursor.EmitDelegate((float orig) =>
            {
                float gravity = GetVariantValue<float>(Variant.SlowfallGravityMultiplier);
                if (gravity != 0.5f && !GetVariantValue<bool>(Variant.DisableJumpGravityLowering)) return gravity;
                return orig;
            });
        }
    }

    public override void Unload()
    {
        IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
    }
}
