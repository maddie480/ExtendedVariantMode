using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;

namespace ExtendedVariants.Variants {
    class InfiniteDashes : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.InfiniteDashes ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.InfiniteDashes = (value != 0);
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
            // let's try and intercept whenever the DashCoroutine sends out 0.3f or 0.15f
            // because we should mod that
            IEnumerator coroutine = orig.Invoke(self);
            while(coroutine.MoveNext()) {
                object o = coroutine.Current;
                if(o != null && o.GetType() == typeof(float)) {
                    yield return o;
                    while(Settings.InfiniteDashes && Input.Dash.Check) {
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

            Logger.Log("ExtendedVariantsModule", $"Patching dashTrailCounter to fix animation with infinite dashes at {cursor.Index} in CIL code for DashUpdate");

            // add a delegate call to modify dashTrailCounter (private variable set in DashCoroutine we can't mod with IL)
            // so that we add more trails if the dash is made longer than usual
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, typeof(Player).GetField("dashTrailCounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            cursor.EmitDelegate<Func<int, int>>(modDashTrailCounter);
            cursor.Emit(OpCodes.Stfld, typeof(Player).GetField("dashTrailCounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
        }

        private int modDashTrailCounter(int dashTrailCounter) {
            if (Settings.InfiniteDashes) {
                return 2; // lock the counter to 2 to have an infinite trail
            }
            return dashTrailCounter;
        }
    }
}
