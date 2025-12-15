using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class HorizontalSpringBounceDuration : AbstractExtendedVariant {
        public HorizontalSpringBounceDuration() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Player.SideBounce += modForceMoveXTimer;
        }

        public override void Unload() {
            IL.Celeste.Player.SideBounce -= modForceMoveXTimer;
        }

        private static void modForceMoveXTimer(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("forceMoveXTimer"))) {
                Logger.Log("ExtendedVariantMode/HorizontalSpringBounceDuration", $"Modding forceMoveXTimer at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<float, float>>(applyHorizontalSpringBounceDuration);
                cursor.Index++;
            }
        }
        private static float applyHorizontalSpringBounceDuration(float orig) {
            return orig * GetVariantValue<float>(Variant.HorizontalSpringBounceDuration);
        }
    }
}
