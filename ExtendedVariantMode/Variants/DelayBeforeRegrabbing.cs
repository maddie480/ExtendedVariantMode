using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class DelayBeforeRegrabbing : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetVariantValue() {
            return Settings.DelayBeforeRegrabbing;
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.DelayBeforeRegrabbing = value / 10f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DelayBeforeRegrabbing = (float) value;
        }

        public override void Load() {
            IL.Celeste.Holdable.Release += modHoldableRelease;
        }

        public override void Unload() {
            IL.Celeste.Holdable.Release -= modHoldableRelease;
        }
        private void modHoldableRelease(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Holdable>("cannotHoldDelay"))) {
                Logger.Log("ExtendedVariantMode/DelayBeforeRegrabbing", $"Modding delay before regrabbing at {cursor.Index} in IL for Holdable.Release");
                cursor.EmitDelegate<Func<float, float>>(orig => orig * Settings.DelayBeforeRegrabbing);
            }
        }
    }
}
