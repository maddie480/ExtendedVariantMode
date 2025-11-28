using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants;

public class SlowfallSpeedThreshold : AbstractExtendedVariant {

    public SlowfallSpeedThreshold() : base(variantType: typeof(float), defaultVariantValue: 40f) { }

    public override void Load() {
        IL.Celeste.Player.NormalUpdate += modNormalUpdate;
    }

    public override void Unload() {
        IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
    }

    public override object ConvertLegacyVariantValue(int value) {
        return (float) value;
    }

    private static void modNormalUpdate(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(MoveType.After,
            instr => instr.MatchCall(typeof(Math), "Abs"),
            instr => instr.MatchLdcR4(40f))) {
            cursor.EmitDelegate(applySlowfallSpeedThreshold);
        }
    }
    private static float applySlowfallSpeedThreshold(float orig) {
        float value = GetVariantValue<float>(Variant.SlowfallSpeedThreshold);
        if (value != 40) return value;
        return orig;
    }
}
