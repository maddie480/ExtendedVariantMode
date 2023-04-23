using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class SuperdashSteeringSpeed : AbstractExtendedVariant {
        private static bool inDashUpdate = false;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Player.DashUpdate += modDashUpdate;
            On.Celeste.Player.DashUpdate += onDashUpdate;
            On.Monocle.Calc.RotateTowards_Vector2_float_float += onRotateTowards;
        }

        public override void Unload() {
            IL.Celeste.Player.DashUpdate -= modDashUpdate;
            On.Celeste.Player.DashUpdate -= onDashUpdate;
            On.Monocle.Calc.RotateTowards_Vector2_float_float -= onRotateTowards;
        }

        private void modDashUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(4.18879032f))) {
                Logger.Log("ExtendedVariantMode/SuperdashSteeringSpeed", $"Editing the steering speed for super dashes at {cursor.Index} in IL code for Player.DashUpdate");

                cursor.EmitDelegate<Func<float>>(determineSuperdashSteeringFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private int onDashUpdate(On.Celeste.Player.orig_DashUpdate orig, Celeste.Player self) {
            inDashUpdate = true;
            int result = orig(self);
            inDashUpdate = false;

            return result;
        }

        private Vector2 onRotateTowards(On.Monocle.Calc.orig_RotateTowards_Vector2_float_float orig, Vector2 vec, float targetAngleRadians, float maxMoveRadians) {
            if (maxMoveRadians == 0f && inDashUpdate && determineSuperdashSteeringFactor() == 0f) {
                // rotating a vector by 0 does nothing, except cause floating point imprecision errors.
                return vec;
            }

            return orig(vec, targetAngleRadians, maxMoveRadians);
        }

        private float determineSuperdashSteeringFactor() {
            return GetVariantValue<float>(Variant.SuperdashSteeringSpeed);
        }
    }
}
