using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections;

namespace ExtendedVariants.Variants {
    class HeldDash : AbstractExtendedVariant {
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
            On.Celeste.Player.DashCoroutine += modDashCoroutine;
            IL.Celeste.Player.DashUpdate += modDashUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.DashCoroutine -= modDashCoroutine;
            IL.Celeste.Player.DashUpdate -= modDashUpdate;
        }

        private IEnumerator modDashCoroutine(On.Celeste.Player.orig_DashCoroutine orig, Player self) {
            // intercept the moment when the dash coroutine sends out the dash time
            // so that we can extend it as long as Dash is pressed.
            IEnumerator coroutine = orig.Invoke(self);
            while (coroutine.MoveNext()) {
                object o = coroutine.Current;
                if (o != null && o.GetType() == typeof(float)) {
                    yield return o;
                    while (Input.Dash.Check && hasHeldDash(self)) {
                        yield return null;
                    }
                } else {
                    yield return o;
                }
            }

            yield break;
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
