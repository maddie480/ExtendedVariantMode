using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class DisableClimbJumping : AbstractExtendedVariant {

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Player.ClimbJump += modClimbJump;
            IL.Celeste.Player.ClimbUpdate += modClimbUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.ClimbJump -= modClimbJump;
            IL.Celeste.Player.ClimbUpdate -= modClimbUpdate;
        }

        private void modClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            if (!GetVariantValue<bool>(Variant.DisableClimbJumping)) {
                orig(self);
            }
        }
        private void modClimbUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<VirtualButton>("get_Pressed"))) {
                Logger.Log("ExtendedVariantMode/DisableClimbJumping", $"Adding condition to kill climb jumping at {cursor.Index} in IL code for ClimbUpdate");

                cursor.Emit(OpCodes.Ldarg_0);
                // load moveX which is private
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(Player).GetField("moveX", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));

                // inject a method to mod the result of the jump button check
                cursor.EmitDelegate<Func<bool, Player, int, bool>>(modJumpButtonCheck);
            }
        }

        private bool modJumpButtonCheck(bool actualValue, Player self, int moveX) {
            if (!GetVariantValue<bool>(Variant.DisableClimbJumping)) {
                // nothing to do
                return actualValue;
            }

            if (moveX == 0 - (int) self.Facing) {
                // This will lead to a wall jump. We want to kill climb jumping. So let it go
                return actualValue;
            }

            // let the game believe Jump is not pressed, so it won't return the player to the Normal state (leading to a weird animation / sound effect).
            return false;
        }
    }
}
