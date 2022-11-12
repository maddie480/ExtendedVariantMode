using Celeste;
using Celeste.Mod;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class DisableJumpingOutOfWater : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.DisableJumpingOutOfWater;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DisableJumpingOutOfWater = (bool) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.DisableJumpingOutOfWater = (value != 0);
        }

        public override void Load() {
            IL.Celeste.Player.SwimUpdate += modSwimUpdate;
            IL.Celeste.Player.NormalUpdate += modNormalUpdate;
        }

        private void modNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Call && (instr.Operand as MethodReference)?.FullName == "T Monocle.Entity::CollideFirst<Celeste.Water>(Microsoft.Xna.Framework.Vector2)")) {
                Logger.Log("ExtendedVariantMode/DisableJumpingOutOfWater", $"Disabling jumping out of water at {cursor.Index} in IL for Player.NormalUpdate");

                cursor.EmitDelegate<Func<Water, Water>>(ignoreWater);
            }
        }

        public override void Unload() {
            IL.Celeste.Player.SwimUpdate -= modSwimUpdate;
        }

        private void modSwimUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Input), "Jump"), instr => instr.MatchCallvirt<VirtualButton>("get_Pressed"))) {
                Logger.Log("ExtendedVariantMode/DisableJumpingOutOfWater", $"Disabling jumping out of water at {cursor.Index} in IL for Player.SwimUpdate");

                cursor.EmitDelegate<Func<bool, bool>>(modInputJumpResult);
            }
        }

        private bool modInputJumpResult(bool orig) {
            if (Settings.DisableJumpingOutOfWater) {
                return false;
            }

            return orig;
        }

        private Water ignoreWater(Water orig) {
            if (Settings.DisableJumpingOutOfWater) {
                return null;
            }

            return orig;
        }
    }
}
