using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class AirFriction : AbstractExtendedVariant {
        public AirFriction() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            // what even is this?
            switch (value) {
                case -1: return 0f;
                case 0: return 0.05f;
                default: return value / 10f;
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
            return GetVariantValue<float>(Variant.AirFriction);
        }
    }
}
