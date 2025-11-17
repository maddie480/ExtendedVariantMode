using System;
using System.Reflection;
using Celeste;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace ExtendedVariants.Variants {
    public class CornerboostProtection : AbstractExtendedVariant {
        private  ILHook il_Celeste_Player_Orig_Update;

        public override void Load() {
            il_Celeste_Player_Orig_Update = new ILHook(typeof(Player).GetMethod(nameof(Player.orig_Update), BindingFlags.Instance | BindingFlags.Public), Player_orig_Update_il);
            IL.Celeste.Player.OnCollideH += Player_OnCollideH_il;
            On.Celeste.Player.ClimbJump += Player_ClimbJump;
        }

        public override void Unload() {
            il_Celeste_Player_Orig_Update.Dispose();
            IL.Celeste.Player.OnCollideH -= Player_OnCollideH_il;
            On.Celeste.Player.ClimbJump -= Player_ClimbJump;
        }

        public CornerboostProtection() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) => value != 0;

        private void Player_orig_Update_il(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, instr => instr.MatchCall<Actor>("Update"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Player>>(disableSafeCornerboostReady);
        }
        private static void disableSafeCornerboostReady(Player player) {
            DynamicData.For(player).Set("safeCornerboostReady", false);
        }

        private static void Player_OnCollideH_il(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld<Player>("wallSpeedRetained"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Action<Player, CollisionData>>(enableSafeCornerboostReady);
        }
        private static void enableSafeCornerboostReady(Player player, CollisionData data) {

            if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.CornerboostProtection) || Math.Abs(data.Moved.X) <= 2)
                return;

            int state = player.StateMachine.State;

            if (state == 0 || state == 2 || state == 5)
                DynamicData.For(player).Set("safeCornerboostReady", true);
        }

        private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump climbJump, Player player) {
            climbJump(player);

            if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.CornerboostProtection))
                return;

            var dynamicData = DynamicData.For(player);

            if (!(dynamicData.Get<bool?>("safeCornerboostReady") ?? false))
                return;

            float wallSpeedRetained = dynamicData.Get<float>("wallSpeedRetained");

            if (Input.MoveX == Math.Sign(wallSpeedRetained))
                dynamicData.Set("wallSpeedRetained", wallSpeedRetained + 40f * Input.MoveX);

            dynamicData.Set("safeCornerboostReady", false);
        }
    }
}