using System;
using System.Reflection;
using Celeste;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace ExtendedVariants.Variants {
    public class CornerboostProtection : AbstractExtendedVariant {
        private class PlayerData : Component {
            public bool SafeCornerboostReady;

            public PlayerData() : base(false, false) { }
        }

        private static ILHook il_Celeste_Player_Orig_Update;

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

        private static PlayerData GetPlayerData(Player player) {
            if (player.Components.Get<PlayerData>() is { } playerData)
                return playerData;

            playerData = new PlayerData();
            player.Components.Add(playerData);

            return playerData;
        }

        private static bool TryGetPlayerData(Player player, out PlayerData playerData) {
            playerData = player.Components.Get<PlayerData>();

            return playerData is not null;
        }

        public CornerboostProtection() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) => value != 0;

        private static void Player_orig_Update_il(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, instr => instr.MatchCall<Actor>("Update"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(DisableSafeCornerboostReady);
        }

        private static void DisableSafeCornerboostReady(Player player) {
            if (TryGetPlayerData(player, out var data))
                data.SafeCornerboostReady = false;
        }

        private static void Player_OnCollideH_il(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld<Player>("wallSpeedRetained"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(EnableSafeCornerboostReady);
        }

        private static void EnableSafeCornerboostReady(Player player, CollisionData data) {
            if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.CornerboostProtection) || Math.Abs(data.Moved.X) <= 2)
                return;

            int state = player.StateMachine.State;

            if (state is Player.StNormal or Player.StDash or Player.StRedDash)
                GetPlayerData(player).SafeCornerboostReady = true;
        }

        private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump climbJump, Player player) {
            climbJump(player);

            if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.CornerboostProtection)
                || !TryGetPlayerData(player, out var data) || !data.SafeCornerboostReady)
                return;

            float addSpeed = 40f * Input.MoveX + player.LiftBoost.X;

            if (Math.Sign(addSpeed) == Math.Sign(player.wallSpeedRetained))
                player.wallSpeedRetained += addSpeed;

            data.SafeCornerboostReady = false;
        }
    }
}