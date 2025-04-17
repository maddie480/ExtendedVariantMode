using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class WalllessWallbounce : AbstractExtendedVariant {
        private static readonly MethodInfo m_SuperWallJump = typeof(Player).GetMethod("SuperWallJump", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo m_WallJumpCheck = typeof(Player).GetMethod("WallJumpCheck", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo m_WallJump = typeof(Player).GetMethod("WallJump", BindingFlags.NonPublic | BindingFlags.Instance);

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public WalllessWallbounce() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += NormalUpdateHook;
            IL.Celeste.Player.DashUpdate += DashOrRedDashUpdateHook;
            IL.Celeste.Player.RedDashUpdate += DashOrRedDashUpdateHook;
        }

        public override void Unload() {
            IL.Celeste.Player.NormalUpdate -= NormalUpdateHook;
            IL.Celeste.Player.DashUpdate -= DashOrRedDashUpdateHook;
            IL.Celeste.Player.RedDashUpdate -= DashOrRedDashUpdateHook;
        }

        private static void DashOrRedDashUpdateHook(ILContext il) {
            ILCursor cursor = new(il);

            // we want to favor vanilla wallbounce behavior, so we append our own to the end

            if (!cursor.TryGotoNext(
                    static instr => instr.MatchLdarg(0),
                    static instr => instr.MatchLdcI4(1),
                    static instr => instr.MatchCallvirt(m_SuperWallJump),
                    static instr => instr.MatchLdcI4(0),
                    static instr => instr.MatchRet())) {
                Logger.Log(
                    LogLevel.Error, $"{nameof(ExtendedVariantsModule)}/{nameof(WalllessWallbounce)}",
                    $"Couldn't find this.SuperWallJump(1) IL sequence to hook in {il.Method.FullName}!"
                );
                return;
            }

            // the IL patch is equivalent to this:

            //   this.SuperWallJump(1);
            //   return 0;
            // }
            //+else if (WalllessWallbounceDashCheck(this))
            //+{
            //+  return 0;
            //+}

            // yoink, you'll be needed later
            Instruction brfalse_Continue = cursor.Prev;
            ILLabel @continue = (ILLabel) brfalse_Continue.Operand;

            cursor.GotoNext(MoveType.After, instr => instr.MatchRet());
            ILLabel tryWalllessWallbounce = cursor.MarkLabel();

            // don't cursor.MoveAfterLabels();, else you'll emit IL in the wrong place

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<Player, bool>>(WalllessWallbounceDashCheck);
            cursor.Emit(OpCodes.Brfalse, @continue);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Ret);

            // remember to fix the label! else our patch will get skipped
            brfalse_Continue.Operand = tryWalllessWallbounce;
        }

        private static void NormalUpdateHook(ILContext il) {
            ILCursor cursor = new(il);

            // we want to favor vanilla wallbounce behavior, so we append our own to the end
            if (!cursor.TryGotoNext(MoveType.After,
                    static instr => instr.MatchLdarg(0),
                    static instr => instr.MatchLdcI4(1),
                    static instr => instr.MatchCallvirt(m_WallJump))) {
                Logger.Log(
                    LogLevel.Error, $"{nameof(ExtendedVariantsModule)}/{nameof(WalllessWallbounce)}",
                    $"Couldn't find this.WallJump(1) IL sequence to hook in {il.Method.FullName}!"
                );
                return;
            }

            // the IL patch is equivalent to this:

            //     this.WallJump(1);
            //   }
            // }
            //+else if (WalllessWallbounceNormalCheck(this, canUnDuck))
            //+{
            //+}
            // else if ((water = base.CollideFirst<Water>(this.Position + Vector2.UnitY * 2f)) != null)
            // {

            // yoink, you'll be needed later
            ILLabel @continue = (ILLabel) cursor.Next.Operand;

            cursor.Index++;

            // important to cursor.MoveAfterLabels, else the labels won't point to our patch
            cursor.MoveAfterLabels();

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_S, (byte) 14);
            cursor.EmitDelegate<Func<Player, bool, bool>>(WalllessWallbounceNormalCheck);
            cursor.Emit(OpCodes.Brtrue_S, @continue);
        }

        private static bool WalllessWallbounceDashCheck(Player player) {
            bool canWallbounce = (bool) Instance.TriggerManager.GetCurrentVariantValue(Variant.WalllessWallbounce);
            if (canWallbounce)
                DoWallbounce(player);
            return canWallbounce;
        }

        private static bool WalllessWallbounceNormalCheck(Player player, bool canUnDuck) {
            bool variantEnabled = (bool) Instance.TriggerManager.GetCurrentVariantValue(Variant.WalllessWallbounce);
            if (!variantEnabled) return false;

            DynamicData playerData = DynamicData.For(player);

            bool canWallbounce
                = canUnDuck
                && player.DashAttacking
                && playerData.Invoke<bool>("get_SuperWallJumpAngleCheck")
                && variantEnabled;

            if (canWallbounce)
                DoWallbounce(player);
            return canWallbounce;
        }

        private static void DoWallbounce(Player player) {
            DynamicData.For(player).Invoke("SuperWallJump", (int) player.Facing);
        }
    }
}
