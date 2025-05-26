using System;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace ExtendedVariants.Variants {
    public class AlternativeBuffering : AbstractExtendedVariant {
        public override void Load() => IL.Monocle.VirtualButton.Update += VirtualButton_Update_il;

        public override void Unload() => IL.Monocle.VirtualButton.Update -= VirtualButton_Update_il;

        public AlternativeBuffering() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) => value != 0;

        private void VirtualButton_Update_il(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.GotoNext(
                instr => instr.MatchLdcR4(0f),
                instr => instr.MatchStfld<VirtualButton>("bufferCounter"));
            cursor.Index++;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit<VirtualButton>(OpCodes.Ldfld, "bufferCounter");
            cursor.EmitDelegate<Func<float, float, float>>((zero, bufferCounter)
                => GetVariantValue<bool>(ExtendedVariantsModule.Variant.AlternativeBuffering) ? bufferCounter : zero);
        }
    }
}