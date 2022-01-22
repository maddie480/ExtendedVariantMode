using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class AirFriction : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object GetVariantValue() {
            return Settings.AirFriction;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.AirFriction = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            // what even is this?
            switch (value) {
                case -1:
                    Settings.AirFriction = 0f;
                    break;
                case 0:
                    Settings.AirFriction = 0.05f;
                    break;
                default:
                    Settings.AirFriction = value / 10f;
                    break;
            }
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= modNormalUpdate;
        }

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.) to apply air friction.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to "float num = this.onGround ? 1f : 0.65f;"
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.65f))) {
                Logger.Log("ExtendedVariantMode/AirFriction", $"Applying friction to constant at {cursor.Index} (friction factor in air) in CIL code for NormalUpdate");

                // 0.65f is the acceleration when in air. Apply the friction factor to it.
                cursor.EmitDelegate<Func<float>>(determineFrictionFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private float determineFrictionFactor() {
            return Settings.AirFriction;
        }
    }
}
