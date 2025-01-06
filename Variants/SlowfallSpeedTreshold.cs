using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants;

public class SlowfallSpeedTreshold : AbstractExtendedVariant
{
    public override Type GetVariantType()
    {
        return typeof(float);
    }

    public override object GetDefaultVariantValue()
    {
        return 40f;
    }

    public override object ConvertLegacyVariantValue(int value)
    {
        return (float)value;
    }

    public override void Load()
    {
        IL.Celeste.Player.NormalUpdate += modNormalUpdate;
    }

    private void modNormalUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(MoveType.After,
        instr => instr.MatchCall(typeof(Math), "Abs"),
        instr => instr.MatchLdcR4(40f)))
        {
            cursor.EmitDelegate((float orig) =>
            {
                float value = GetVariantValue<float>(Variant.SlowfallSpeedTreshold);
                if (value != 40 && !GetVariantValue<bool>(Variant.DisableJumpGravityLowering)) return value;
                return orig;
            });
        }
    }

    public override void Unload()
    {
        IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
    }
}
