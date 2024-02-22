using System;
using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace ExtendedVariants.Variants;

public class LiftboostProtection : AbstractExtendedVariant {
    public override void Load() {
        On.Celeste.Player.Jump += Player_Jump;
        On.Celeste.Player.SuperJump += Player_SuperJump;
        On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
        IL.Celeste.Platform.Update += Platform_Update_il;
        IL.Celeste.Platform.MoveH_float += PatchLiftboostProtectionX;
        IL.Celeste.Platform.MoveH_float_float += PatchLiftboostProtectionX;
        IL.Celeste.Platform.MoveV_float += PatchLiftboostProtectionY;
        IL.Celeste.Platform.MoveV_float_float += PatchLiftboostProtectionY;
        IL.Celeste.Platform.MoveHNaive += PatchLiftboostProtectionX;
        IL.Celeste.Platform.MoveVNaive += PatchLiftboostProtectionY;
        IL.Celeste.Platform.MoveHCollideSolids += PatchLiftboostProtectionX;
        IL.Celeste.Platform.MoveVCollideSolids += PatchLiftboostProtectionY;
        IL.Celeste.Platform.MoveHCollideSolidsAndBounds += PatchLiftboostProtectionX;
        IL.Celeste.Platform.MoveVCollideSolidsAndBounds_Level_float_bool_Action3 += PatchLiftboostProtectionY;
    }

    public override void Unload() {
        On.Celeste.Player.Jump -= Player_Jump;
        On.Celeste.Player.SuperJump -= Player_SuperJump;
        On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
        IL.Celeste.Platform.Update -= Platform_Update_il;
        IL.Celeste.Platform.MoveH_float -= PatchLiftboostProtectionX;
        IL.Celeste.Platform.MoveH_float_float -= PatchLiftboostProtectionX;
        IL.Celeste.Platform.MoveV_float -= PatchLiftboostProtectionY;
        IL.Celeste.Platform.MoveV_float_float -= PatchLiftboostProtectionY;
        IL.Celeste.Platform.MoveHNaive -= PatchLiftboostProtectionX;
        IL.Celeste.Platform.MoveVNaive -= PatchLiftboostProtectionY;
        IL.Celeste.Platform.MoveHCollideSolids -= PatchLiftboostProtectionX;
        IL.Celeste.Platform.MoveVCollideSolids -= PatchLiftboostProtectionY;
        IL.Celeste.Platform.MoveHCollideSolidsAndBounds -= PatchLiftboostProtectionX;
        IL.Celeste.Platform.MoveVCollideSolidsAndBounds_Level_float_bool_Action3 -= PatchLiftboostProtectionY;
    }

    public override Type GetVariantType() => typeof(bool);

    public override object GetDefaultVariantValue() => false;

    public override object ConvertLegacyVariantValue(int value) => value != 0;

    private static void CheckForLiftboost(Player player, Vector2 dir) {
        if (player.LiftSpeed != Vector2.Zero)
            return;

        Platform platform;

        if (dir.X == 0f && dir.Y > 0f)
            platform = player.CollideFirst<Platform>(player.Position + dir);
        else
            platform = player.CollideFirst<Solid>(player.Position + dir);

        if (platform != null)
            player.LiftSpeed = platform.LiftSpeed;
    }

    private void Player_Jump(On.Celeste.Player.orig_Jump jump, Player player, bool particles, bool playsfx) {
        if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection))
            CheckForLiftboost(player, Vector2.UnitY);

        jump(player, particles, playsfx);
    }

    private void Player_SuperJump(On.Celeste.Player.orig_SuperJump superJump, Player player) {
        if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection))
            CheckForLiftboost(player, Vector2.UnitY);

        superJump(player);
    }

    private void Player_SuperWallJump(On.Celeste.Player.orig_SuperWallJump superWallJump, Player player, int dir) {
        if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection))
            CheckForLiftboost(player, -5 * dir * Vector2.UnitX);

        superWallJump(player, dir);
    }

    private void Platform_Update_il(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Action<Platform>>(platform => {
            if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection))
                platform.LiftSpeed = Vector2.Zero;
        });

        cursor.GotoNext(instr => instr.MatchStfld<Platform>("LiftSpeed"));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate<Func<Vector2, Platform, Vector2>>((value, platform)
            => GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection) ? platform.LiftSpeed : value);
    }

    private void PatchLiftboostProtectionX(ILContext il) {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdflda<Platform>("LiftSpeed"))) {
            cursor.GotoNext(instr => instr.MatchStfld<Vector2>("X"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, Platform, float>>((value, platform)
                => GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection) && value == 0f ? platform.LiftSpeed.X : value);
        }
    }

    private void PatchLiftboostProtectionY(ILContext il) {
        var cursor = new ILCursor(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdflda<Platform>("LiftSpeed"))) {
            cursor.GotoNext(instr => instr.MatchStfld<Vector2>("Y"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, Platform, float>>((value, platform)
                => GetVariantValue<bool>(ExtendedVariantsModule.Variant.LiftboostProtection) && value == 0f ? platform.LiftSpeed.Y : value);
        }
    }
}