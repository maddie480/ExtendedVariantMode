using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    class SuperdashSteeringSpeed : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.SuperdashSteeringSpeed;
        }

        public override void SetValue(int value) {
            Settings.SuperdashSteeringSpeed = value;
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
            return Settings.SuperdashSteeringSpeed / 10f;
        }
    }
}
