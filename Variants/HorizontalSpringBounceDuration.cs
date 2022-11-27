using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class HorizontalSpringBounceDuration : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetVariantValue() {
            return Settings.HorizontalSpringBounceDuration;
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.HorizontalSpringBounceDuration = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.HorizontalSpringBounceDuration = value / 10f;
        }

        public override void Load() {
            IL.Celeste.Player.SideBounce += modForceMoveXTimer;
        }

        public override void Unload() {
            IL.Celeste.Player.SideBounce -= modForceMoveXTimer;
        }

        private void modForceMoveXTimer(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("forceMoveXTimer"))) {
                Logger.Log("ExtendedVariantMode/HorizontalSpringBounceDuration", $"Modding forceMoveXTimer at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<float, float>>(orig => orig * Settings.HorizontalSpringBounceDuration);
                cursor.Index++;
            }
        }
    }
}
