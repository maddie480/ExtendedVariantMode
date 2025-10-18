using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class ClimbJumpStaminaCost : AbstractExtendedVariant {
        public ClimbJumpStaminaCost() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        private Hook checkStaminaHook;
        private ILHook playerUpdateHook;

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Player.ClimbJump += onPlayerClimbJump;
            playerUpdateHook = new ILHook(typeof(Player).GetMethod(nameof(Player.orig_Update), BindingFlags.Instance | BindingFlags.Public), onPlayerUpdate);
            checkStaminaHook = new Hook(
                typeof(Player).GetProperty("CheckStamina", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetGetMethod(true),
                typeof(ClimbJumpStaminaCost).GetMethod("modCheckStamina", BindingFlags.NonPublic | BindingFlags.Static)
            );
        }

        public override void Unload() {
            IL.Celeste.Player.ClimbJump -= onPlayerClimbJump;

            playerUpdateHook?.Dispose();
            playerUpdateHook = null;

            checkStaminaHook?.Dispose();
            checkStaminaHook = null;
        }

        private void onPlayerClimbJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(27.5f))) {
                Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/ClimbJumpStaminaCost", $"Applying cost multiplier to Stamina @ {cursor.Index} in IL for Player.ClimbJump");
                cursor.EmitDelegate<Func<float>>(() => GetVariantValue<float>(Variant.ClimbJumpStaminaCost));
                cursor.Emit(OpCodes.Mul);
            }
        }

        private void onPlayerUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(27.5f))) {
                Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/ClimbJumpStaminaCost", $"Applying refund multiplier to Stamina @ {cursor.Index} in IL for Player.Update");
                cursor.EmitDelegate<Func<float>>(() => GetVariantValue<float>(Variant.ClimbJumpStaminaCost));
                cursor.Emit(OpCodes.Mul);
            }
        }

        private static float modCheckStamina(Func<Player, float> orig, Player self) {
            if ((float) Instance.TriggerManager.GetCurrentVariantValue(Variant.ClimbJumpStaminaCost) == 1f) {
                return orig(self);
            }
            if (DynamicData.For(self).Get<float>("wallBoostTimer") > 0f) {
                float climbJumpStaminaCost = (float) Instance.TriggerManager.GetCurrentVariantValue(Variant.ClimbJumpStaminaCost);
                return self.Stamina + 27.5f * climbJumpStaminaCost;
            }
            return self.Stamina;
        }
    }
}
