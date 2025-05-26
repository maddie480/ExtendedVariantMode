using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class DisableWallJumping : AbstractExtendedVariant {

        private ILHook wallJumpHook;

        public DisableWallJumping() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            wallJumpHook = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic), modWallJump);
            On.Celeste.Player.WallJumpCheck += modWallJumpCheck;
        }

        public override void Unload() {
            if (wallJumpHook != null) wallJumpHook.Dispose();
            On.Celeste.Player.WallJumpCheck -= modWallJumpCheck;
        }

        /// <summary>
        /// Detour the WallJump method in order to disable it if we want.
        /// </summary>
        /// <param name="orig">the original method</param>
        /// <param name="self">the player</param>
        /// <param name="dir">the wall jump direction</param>
        private void modWallJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            Instruction firstInstruction = cursor.Next;

            Logger.Log("ExtendedVariantMode/DisableWallJumping", $"Injecting code to break method if wall jumping is disabled at {cursor.Index} in IL code for WallJump");

            cursor.EmitDelegate<Func<bool>>(isWallJumpingDisabled);
            cursor.Emit(OpCodes.Brfalse, firstInstruction);
            cursor.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Mods the WallJumpCheck method, checking if a walljump is possible.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self">the player</param>
        /// <param name="dir">the direction</param>
        /// <returns>true if walljumping is possible, false otherwise</returns>
        private bool modWallJumpCheck(On.Celeste.Player.orig_WallJumpCheck orig, Player self, int dir) {
            if (GetVariantValue<bool>(Variant.DisableWallJumping)) {
                return false;
            }
            return orig(self, dir);
        }

        private bool isWallJumpingDisabled() {
            return GetVariantValue<bool>(Variant.DisableWallJumping);
        }
    }
}
