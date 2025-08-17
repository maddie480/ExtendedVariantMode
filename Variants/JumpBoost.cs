using System;
using System.Reflection;
using Celeste;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace ExtendedVariants.Variants;

public class JumpBoost : AbstractExtendedVariant {
    private const float JumpHBoost = 40f;
    private const float WallJumpHSpeed = Player.MaxRun + JumpHBoost;
    private const float SuperWallJumpH = Player.MaxRun + JumpHBoost * 2;

    public JumpBoost() : base(typeof(float), 1f) { }

    public override object ConvertLegacyVariantValue(int value) {
        return value / 10f;
    }

    private ILHook Player_orig_WallJump;
    private ILHook Player_orig_Update;

    public override void Load() {
        IL.Celeste.Player.HiccupJump += ChangeJumpHBoost;
        IL.Celeste.Player.Jump += ChangeJumpHBoost;

        Player_orig_WallJump = new ILHook(
            typeof(Player).GetMethod("orig_WallJump", BindingFlags.NonPublic | BindingFlags.Instance),
            ChangeWallJumpHSpeed);
        Player_orig_Update = new ILHook(
            typeof(Player).GetMethod("orig_Update"),
            ChangeWallJumpHSpeedInUpdate);

        IL.Celeste.Player.SuperWallJump += ChangeSuperWallJumpH;
    }

    public override void Unload() {
        IL.Celeste.Player.HiccupJump -= ChangeJumpHBoost;
        IL.Celeste.Player.Jump -= ChangeJumpHBoost;

        Player_orig_WallJump?.Dispose();
        Player_orig_Update?.Dispose();

        IL.Celeste.Player.SuperWallJump -= ChangeSuperWallJumpH;
    }

    private void ChangeJumpHBoost(ILContext il) {
        ILCursor cursor = new(il);

        // hELPER emits some il in Player.Jump which also puts a ldc.r4 40. we need to change all of those +40s.
        while (cursor.TryGotoNextBestFit(MoveType.Before,
            static instr => instr.MatchLdcR4(JumpHBoost),
            static instr => instr.MatchLdarg(0),
            static instr => instr.MatchLdfld<Player>("moveX"),
            static instr => instr.MatchConvR4(),
            static instr => instr.MatchMul())) {
            // we're guaranteed to be before the ldc.r4 40
            cursor.Index++;
            cursor.EmitDelegate<Func<float, float>>(ApplyJumpHBoostMultiplier);
        }
    }

    private void ChangeWallJumpHSpeed(ILContext il) {
        ILCursor cursor = new(il);

        while (cursor.TryGotoNextBestFit(MoveType.Before,
            static instr => instr.MatchLdcR4(WallJumpHSpeed),
            static instr => instr.MatchLdarg(1),
            static instr => instr.MatchConvR4(),
            static instr => instr.MatchMul())) {
            // we're guaranteed to be before the ldc.r4 130
            cursor.Index++;
            cursor.EmitDelegate<Func<float, float>>(ApplyWallJumpHSpeedMultiplier);
        }
    }

    private void ChangeWallJumpHSpeedInUpdate(ILContext il) {
        ILCursor cursor = new(il);

        // this code handles wallboosts so we need to also affect that
        while (cursor.TryGotoNextBestFit(MoveType.Before,
            static instr => instr.MatchLdcR4(WallJumpHSpeed),
            static instr => instr.MatchLdarg(0),
            static instr => instr.MatchLdfld<Player>("moveX"),
            static instr => instr.MatchConvR4(),
            static instr => instr.MatchMul())) {
            // we're guaranteed to be before the ldc.r4 130
            cursor.Index++;
            cursor.EmitDelegate<Func<float, float>>(ApplyWallJumpHSpeedMultiplier);
        }
    }

    private void ChangeSuperWallJumpH(ILContext il) {
        ILCursor cursor = new(il);

        while (cursor.TryGotoNextBestFit(MoveType.Before,
            static instr => instr.MatchLdcR4(SuperWallJumpH),
            static instr => instr.MatchLdarg(1),
            static instr => instr.MatchConvR4(),
            static instr => instr.MatchMul())) {
            // we're guaranteed to be before the ldc.r4 170
            cursor.Index++;
            cursor.EmitDelegate<Func<float, float>>(ApplySuperWallJumpHMultiplier);
        }
    }

    private float ApplyJumpHBoostMultiplier(float orig) {
        return orig * GetVariantValue<float>(ExtendedVariantsModule.Variant.JumpBoost);
    }

    private float ApplyWallJumpHSpeedMultiplier(float orig) {
        // orig already contains JumpHBoost
        return orig + JumpHBoost * (GetVariantValue<float>(ExtendedVariantsModule.Variant.JumpBoost) - 1);
    }

    private float ApplySuperWallJumpHMultiplier(float orig) {
        // orig already contains JumpHBoost * 2
        return orig + 2 * JumpHBoost * (GetVariantValue<float>(ExtendedVariantsModule.Variant.JumpBoost) - 1);
    }
}
