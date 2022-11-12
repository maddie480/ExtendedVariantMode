using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class SuperdashSteeringSpeed : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object GetVariantValue() {
            return Settings.SuperdashSteeringSpeed;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.SuperdashSteeringSpeed = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.SuperdashSteeringSpeed = (value / 10f);
        }

        public override void Load() {
            IL.Celeste.Player.DashUpdate += modDashUpdate;
        }

        public override void Unload() {
            IL.Celeste.Player.DashUpdate -= modDashUpdate;
        }

        private void modDashUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(4.18879032f))) {
                Logger.Log("ExtendedVariantMode/SuperdashSteeringSpeed", $"Editing the steering speed for super dashes at {cursor.Index} in IL code for Player.DashUpdate");

                cursor.EmitDelegate<Func<float>>(determineSuperdashSteeringFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private float determineSuperdashSteeringFactor() {
            return Settings.SuperdashSteeringSpeed;
        }
    }
}
