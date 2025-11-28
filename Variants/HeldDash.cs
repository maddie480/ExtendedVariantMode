using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Reflection;
using Celeste.Mod.EV;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class HeldDash : AbstractExtendedVariant {
        // allows to check with reflection that Input.CrouchDash exists before using it.
        private static FieldInfo crouchDash = typeof(Input).GetField("CrouchDash");

        public HeldDash() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Player.DashCoroutine += modDashCoroutine;
            IL.Celeste.Player.DashUpdate += modDashUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.DashCoroutine -= modDashCoroutine;
            IL.Celeste.Player.DashUpdate -= modDashUpdate;
        }

        private static IEnumerator modDashCoroutine(On.Celeste.Player.orig_DashCoroutine orig, Player self) {
            // intercept the moment when the dash coroutine sends out the dash time
            // so that we can extend it as long as Dash is pressed.
            IEnumerator coroutine = orig(self).SafeEnumerate();
            while (coroutine.MoveNext()) {
                object o = coroutine.Current;
                if (o != null && o.GetType() == typeof(float)) {
                    yield return o;
                    while (hasHeldDash(self) && (Input.Dash.Check || (crouchDash != null && crouchDashCheck()))) {
                        DynData<Player> selfData = new DynData<Player>(self);
                        selfData["dashAttackTimer"] = 0.15f; // hold the dash attack timer to continue breaking dash blocks and such.
                        selfData["gliderBoostTimer"] = 0.30f; // hold the glider boost timer to still get boosted by jellies.

                        yield return null;
                    }
                } else {
                    yield return o;
                }
            }

            yield break;
        }

        private static bool crouchDashCheck() {
            return Input.CrouchDash.Check;
        }

        /// <summary>
        /// Edits the DashUpdate method in Player (called while the player is dashing).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private static void modDashUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            Logger.Log("ExtendedVariantMode/HeldDash", $"Patching dashTrailCounter to fix animation with infinite dashes at {cursor.Index} in CIL code for DashUpdate");

            // add a delegate call to modify dashTrailCounter (private variable set in DashCoroutine we can't mod with IL)
            // so that we add more trails if the dash is made longer than usual
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, typeof(Player).GetField("dashTrailCounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<int, Player, int>>(modDashTrailCounter);
            cursor.Emit(OpCodes.Stfld, typeof(Player).GetField("dashTrailCounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
        }

        private static int modDashTrailCounter(int dashTrailCounter, Player self) {
            if (hasHeldDash(self)) {
                return 2; // lock the counter to 2 to have an infinite trail
            }
            return dashTrailCounter;
        }

        private static bool hasHeldDash(Player self) {
            // expose an "ExtendedVariantsHeldDash" DynData field to other mods.
            return GetVariantValue<bool>(Variant.HeldDash) || (new DynData<Player>(self).Data.TryGetValue("ExtendedVariantsHeldDash", out object o) && o is bool b && b);
        }
    }
}
