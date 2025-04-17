using Celeste;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using Monocle;
using static ExtendedVariants.Module.ExtendedVariantsModule;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Celeste.Mod;
using Mono.Cecil.Cil;

namespace ExtendedVariants.Variants {
    public class PermanentDashAttack : AbstractExtendedVariant {
        private Hook dashAttackingHook;
        private static MethodInfo playerCorrectDashPrecision = typeof(Player).GetMethod("CorrectDashPrecision", BindingFlags.NonPublic | BindingFlags.Instance);

        public PermanentDashAttack() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return (value != 0);
        }

        public override void Load() {
            dashAttackingHook = new Hook(
                typeof(Player).GetMethod("get_DashAttacking"),
                typeof(PermanentDashAttack).GetMethod("hookOnDashAttacking", BindingFlags.NonPublic | BindingFlags.Instance),
                this
            );

            On.Celeste.Player.Update += onPlayerUpdate;
            IL.Celeste.Player.OnCollideH += onPlayerCollide;
            IL.Celeste.Player.OnCollideV += onPlayerCollide;
        }

        public override void Unload() {
            dashAttackingHook.Dispose();
            dashAttackingHook = null;

            On.Celeste.Player.Update -= onPlayerUpdate;
            IL.Celeste.Player.OnCollideH -= onPlayerCollide;
            IL.Celeste.Player.OnCollideV -= onPlayerCollide;
        }

        private bool hookOnDashAttacking(Func<Player, bool> orig, Player self) {
            if (GetVariantValue<bool>(Variant.PermanentDashAttack)) {
                return true;
            }

            return orig(self);
        }

        private void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            if (self.StateMachine.State != Player.StDash && GetVariantValue<bool>(Variant.PermanentDashAttack)) {
                // make the (fake) dash direction match the player's direction, to trigger dash blocks when running into them
                // without having to dash in the right direction first for example.
                self.DashDir = self.Speed.SafeNormalize();
                self.DashDir = (Vector2) playerCorrectDashPrecision.Invoke(self, new object[] { self.DashDir });
            }

            orig(self);
        }

        private void onPlayerCollide(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("event:/game/06_reflection/feather_state_bump"))) {
                Logger.Log("ExtendedVariantMode/PermanentDashAttack", $"Tweaking feather behavior with permanent dash attack at {cursor.Index} in Player.{il.Method.Name}");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Action<Player, CollisionData>>((self, data) => {
                    if (GetVariantValue<bool>(Variant.PermanentDashAttack)) {
                        // trigger dash events when the player bounces around with the feather.
                        data.Hit?.OnDashCollide?.Invoke(self, data.Direction);
                    }
                });
            }
        }
    }
}
