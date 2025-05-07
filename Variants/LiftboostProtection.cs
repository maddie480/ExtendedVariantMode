using System;
using System.Reflection;
using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Platform = Celeste.Platform;

namespace ExtendedVariants.Variants {
    public class LiftboostProtection : AbstractExtendedVariant {
        private const string PLATFORM_LIFT_SPEED_HISTORY = "ExtendedVariantMode.Platform.liftSpeedHistory";

        private static bool TryGetPlatform(Player player, Vector2 dir, out Platform platform) {
            if (dir.X == 0f && dir.Y > 0f)
                platform = player.CollideFirst<Platform>(player.Position + dir);
            else
                platform = player.CollideFirst<Solid>(player.Position + dir);

            return platform != null;
        }

        private static LiftSpeedHistory GetLiftSpeedHistory(Platform platform) {
            var dynamicData = DynamicData.For(platform);

            if (dynamicData.TryGet<LiftSpeedHistory>(PLATFORM_LIFT_SPEED_HISTORY, out var liftSpeedHistory))
                return liftSpeedHistory;

            liftSpeedHistory = new LiftSpeedHistory();
            dynamicData.Set(PLATFORM_LIFT_SPEED_HISTORY, liftSpeedHistory);

            return liftSpeedHistory;
        }

        private static Vector2 GetCorrectedLiftSpeed(Platform platform) {
            var liftSpeedHistory = GetLiftSpeedHistory(platform);

            var minusTwo = liftSpeedHistory.MinusTwo;
            var minusOne = liftSpeedHistory.MinusOne;
            var current = liftSpeedHistory.Current;

            return new Vector2(CorrectLiftSpeed(minusTwo.X, minusOne.X, current.X), CorrectLiftSpeed(minusTwo.Y, minusOne.Y, current.Y));

            float CorrectLiftSpeed(float minusTwo, float minusOne, float liftSpeed)
                => Math.Sign(liftSpeed) * Math.Max(Math.Abs(liftSpeed), Math.Min(Math.Sign(liftSpeed) * minusOne, Math.Sign(liftSpeed) * (2f * minusOne - minusTwo)));
        }

        private ILHook il_Celeste_Player_Orig_WallJump;

        public override void Load() {
            On.Celeste.Player.Jump += Player_Jump;
            On.Celeste.Player.SuperJump += Player_SuperJump;
            il_Celeste_Player_Orig_WallJump
                = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.NonPublic | BindingFlags.Instance), Player_orig_WallJump_il);
            On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
            On.Celeste.Platform.Update += Platform_Update;
            IL.Celeste.Platform.MoveH_float += PatchLiftboostProtectionX;
            IL.Celeste.Platform.MoveH_float_float += PatchLiftboostProtectionX;
            IL.Celeste.Platform.MoveV_float += PatchLiftboostProtectionY;
            IL.Celeste.Platform.MoveV_float_float += PatchLiftboostProtectionY;
            IL.Celeste.Platform.MoveHNaive += PatchLiftboostProtectionX;
            IL.Celeste.Platform.MoveVNaive += PatchLiftboostProtectionY;
            IL.Celeste.Platform.MoveHCollideSolids += PatchLiftboostProtectionX;
            IL.Celeste.Platform.MoveVCollideSolids += PatchLiftboostProtectionY;
            IL.Celeste.Platform.MoveHCollideSolidsAndBounds += PatchLiftboostProtectionX;
            IL.Celeste.Platform.MoveVCollideSolidsAndBounds_Level_float_bool_Action3_bool += PatchLiftboostProtectionY;
        }

        public override void Unload() {
            On.Celeste.Player.Jump -= Player_Jump;
            On.Celeste.Player.SuperJump -= Player_SuperJump;
            il_Celeste_Player_Orig_WallJump.Dispose();
            On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
            On.Celeste.Platform.Update -= Platform_Update;
            IL.Celeste.Platform.MoveH_float -= PatchLiftboostProtectionX;
            IL.Celeste.Platform.MoveH_float_float -= PatchLiftboostProtectionX;
            IL.Celeste.Platform.MoveV_float -= PatchLiftboostProtectionY;
            IL.Celeste.Platform.MoveV_float_float -= PatchLiftboostProtectionY;
            IL.Celeste.Platform.MoveHNaive -= PatchLiftboostProtectionX;
            IL.Celeste.Platform.MoveVNaive -= PatchLiftboostProtectionY;
            IL.Celeste.Platform.MoveHCollideSolids -= PatchLiftboostProtectionX;
            IL.Celeste.Platform.MoveVCollideSolids -= PatchLiftboostProtectionY;
            IL.Celeste.Platform.MoveHCollideSolidsAndBounds -= PatchLiftboostProtectionX;
            IL.Celeste.Platform.MoveVCollideSolidsAndBounds_Level_float_bool_Action3_bool -= PatchLiftboostProtectionY;
        }

        public LiftboostProtection() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) => value != 0;

        private void Player_Jump(On.Celeste.Player.orig_Jump jump, Player player, bool particles, bool playsfx) {
            if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection)
                && player.LiftSpeed == Vector2.Zero && TryGetPlatform(player, Vector2.UnitY, out var platform)) {
                var liftSpeed = GetCorrectedLiftSpeed(platform);

                if (platform is not JumpThru || liftSpeed.Y != 0f)
                    player.LiftSpeed = liftSpeed;
            }

            jump(player, particles, playsfx);
        }

        private void Player_SuperJump(On.Celeste.Player.orig_SuperJump superJump, Player player) {
            if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection)
                && player.LiftSpeed == Vector2.Zero && TryGetPlatform(player, Vector2.UnitY, out var platform)) {
                var liftSpeed = GetCorrectedLiftSpeed(platform);

                if (platform is not JumpThru || liftSpeed.Y != 0f)
                    player.LiftSpeed = liftSpeed;
            }

            superJump(player);
        }

        private void Player_orig_WallJump_il(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.GotoNext(instr => instr.MatchCall<Actor>("set_LiftSpeed"));

            cursor.Emit(OpCodes.Ldloc_2);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Func<Vector2, Solid, int, Vector2>>((value, solid, dir) => {
                if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection))
                    return value;

                var liftSpeed = GetCorrectedLiftSpeed(solid);

                if (Math.Sign(liftSpeed.X) == dir)
                    return liftSpeed.X * Vector2.UnitX;

                return value;
            });
        }

        private void Player_SuperWallJump(On.Celeste.Player.orig_SuperWallJump superWallJump, Player player, int dir) {
            if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection)
                && player.LiftSpeed == Vector2.Zero && TryGetPlatform(player, -5 * dir * Vector2.UnitX, out var platform)) {
                var liftSpeed = GetCorrectedLiftSpeed(platform);

                if (Math.Sign(liftSpeed.X) == dir)
                    player.LiftSpeed = liftSpeed.X * Vector2.UnitX;
            }

            superWallJump(player, dir);
        }

        private void Platform_Update(On.Celeste.Platform.orig_Update update, Platform platform) {
            if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection)) {
                var liftSpeedHistory = GetLiftSpeedHistory(platform);

                liftSpeedHistory.MinusTwo = liftSpeedHistory.MinusOne;
                liftSpeedHistory.MinusOne = liftSpeedHistory.Current;
                liftSpeedHistory.Current = Vector2.Zero;
            }

            update(platform);
        }

        private void PatchLiftboostProtectionX(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.Index = -1;
            cursor.GotoPrev(instr => instr.MatchLdflda<Platform>("LiftSpeed"));
            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld<Vector2>("X"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Platform>>(platform => {
                if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection))
                    return;

                float liftSpeed = platform.LiftSpeed.X;

                if (liftSpeed == 0f)
                    return;

                GetLiftSpeedHistory(platform).Current.X = liftSpeed;
                platform.LiftSpeed = GetCorrectedLiftSpeed(platform);
            });

            cursor.GotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Stloc_0);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.EmitDelegate<Action<Platform, int>>((platform, move) => {
                if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection)
                    || move != 0 || platform.LiftSpeed.X == 0f || !platform.Collidable || platform is not Solid solid)
                    return;

                foreach (var entity in platform.Scene.Tracker.GetEntities<Actor>()) {
                    var actor = (Actor) entity;

                    if (actor.IsRiding(solid))
                        actor.LiftSpeed = solid.LiftSpeed;
                }
            });
        }

        private void PatchLiftboostProtectionY(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.Index = -1;
            cursor.GotoPrev(instr => instr.MatchLdflda<Platform>("LiftSpeed"));
            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld<Vector2>("Y"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Platform>>(platform => {
                if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection))
                    return;

                float liftSpeed = platform.LiftSpeed.Y;

                if (liftSpeed == 0f)
                    return;

                GetLiftSpeedHistory(platform).Current.Y = liftSpeed;
                platform.LiftSpeed = GetCorrectedLiftSpeed(platform);
            });

            cursor.GotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Stloc_0);

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.EmitDelegate<Action<Platform, int>>((platform, move) => {
                if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection)
                    || move != 0 || platform.LiftSpeed.Y == 0f || !platform.Collidable)
                    return;

                if (platform is Solid solid) {
                    foreach (var entity in platform.Scene.Tracker.GetEntities<Actor>()) {
                        var actor = (Actor) entity;

                        if (actor.IsRiding(solid))
                            actor.LiftSpeed = solid.LiftSpeed;
                    }
                }
                else if (platform is JumpThru jumpThru) {
                    foreach (var entity in platform.Scene.Tracker.GetEntities<Actor>()) {
                        var actor = (Actor) entity;

                        if (actor.IsRiding(jumpThru))
                            actor.LiftSpeed = jumpThru.LiftSpeed;
                    }
                }
            });
        }

        private class LiftSpeedHistory {
            public Vector2 MinusTwo;
            public Vector2 MinusOne;
            public Vector2 Current;
        }
    }
}
