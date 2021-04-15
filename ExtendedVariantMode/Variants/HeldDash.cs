using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace ExtendedVariants.Variants {
    class HeldDash : AbstractExtendedVariant {
        private static ILHook hookDashCoroutine;

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.HeldDash ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.HeldDash = (value != 0);
        }

        public override void Load() {
            IL.Celeste.Player.DashUpdate += modDashUpdate;

            hookDashCoroutine = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modDashCoroutine);
        }

        public override void Unload() {
            IL.Celeste.Player.DashUpdate -= modDashUpdate;

            hookDashCoroutine?.Dispose();
            hookDashCoroutine = null;
        }

        private void modDashCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.AfterLabel,
                instr => instr.MatchLdloc(1), instr => instr.MatchCallvirt<Player>("CreateTrail"),
                instr => instr.MatchLdloc(1), instr => instr.MatchLdcI4(1), instr => instr.MatchStfld<Player>("AutoJump"))) {

                Logger.Log("ExtendedVariantMode/HeldDash", $"Modding DashCoroutine to implement HeldDash at {cursor.Index} in IL");

                // if (Input.Dash.Check && hasHeldDash(self)), we need to hold the dash!
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate<Func<Player, bool>>(self => Input.Dash.Check && hasHeldDash(self));
                cursor.Emit(OpCodes.Brfalse, cursor.Next);

                // all this mess is just "yield return null"
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldc_I4_2);
                cursor.Emit(OpCodes.Stfld, typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget()
                    .DeclaringType.GetField("<>1__state", BindingFlags.NonPublic | BindingFlags.Instance));
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldnull);
                cursor.Emit(OpCodes.Stfld, typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget()
                    .DeclaringType.GetField("<>2__current", BindingFlags.NonPublic | BindingFlags.Instance));
                cursor.Emit(OpCodes.Ldc_I4_1);
                cursor.Emit(OpCodes.Ret);
            }
        }

        /// <summary>
        /// Edits the DashUpdate method in Player (called while the player is dashing).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modDashUpdate(ILContext il) {
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

        private int modDashTrailCounter(int dashTrailCounter, Player self) {
            if (hasHeldDash(self)) {
                return 2; // lock the counter to 2 to have an infinite trail
            }
            return dashTrailCounter;
        }

        private bool hasHeldDash(Player self) {
            // expose an "ExtendedVariantsHeldDash" DynData field to other mods.
            return Settings.HeldDash || (new DynData<Player>(self).Data.TryGetValue("ExtendedVariantsHeldDash", out object o) && o is bool b && b);
        }
    }
}
