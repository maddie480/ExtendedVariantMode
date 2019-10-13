using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;

namespace ExtendedVariants.Variants {
    public class DashLength : AbstractExtendedVariant {

        private float lastDashDuration = 0f;

        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.DashLength;
        }

        public override void SetValue(int value) {
            Settings.DashLength = value;
        }

        public override void Load() {
            IL.Celeste.Player.DashBegin += modDashBegin;
            IL.Celeste.Player.DashUpdate += modDashUpdate;
            On.Celeste.Player.DashCoroutine += modDashCoroutine;
        }

        public override void Unload() {
            IL.Celeste.Player.DashBegin -= modDashBegin;
            IL.Celeste.Player.DashUpdate -= modDashUpdate;
            On.Celeste.Player.DashCoroutine -= modDashCoroutine;
        }

        /// <summary>
        /// Edits the DashBegin method in Player (called when the player dashes).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modDashBegin(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump where 0.3 is loaded (0.3 is the dash timer)
            if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 0.3f)) {
                Logger.Log("ExtendedVariantsModule", $"Applying dash length to constant at {cursor.Index} in CIL code for DashBegin");

                cursor.EmitDelegate<Func<float>>(determineDashLengthFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Returns the current dash length factor.
        /// </summary>
        /// <returns>The dash length factor (1 = default dash length)</returns>
        private float determineDashLengthFactor() {
            return Settings.DashLength / 10f;
        }

        private IEnumerator modDashCoroutine(On.Celeste.Player.orig_DashCoroutine orig, Player self) {
            // let's try and intercept whenever the DashCoroutine sends out 0.3f or 0.15f
            // because we should mod that
            IEnumerator coroutine = orig.Invoke(self);
            while(coroutine.MoveNext()) {
                object o = coroutine.Current;
                if(o != null && o.GetType() == typeof(float)) {
                    float f = (float)o;
                    if (f == 0.15f || f == 0.3f) {
                        f *= Settings.DashLength / 10f;
                        lastDashDuration = f;
                    }
                    yield return f;
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

            Logger.Log("ExtendedVariantsModule", $"Patching dashTrailCounter to fix animation with long dashes at {cursor.Index} in CIL code for DashUpdate");

            // add a delegate call to modify dashTrailCounter (private variable set in DashCoroutine we can't mod with IL)
            // so that we add more trails if the dash is made longer than usual
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, typeof(Player).GetField("dashTrailCounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
            cursor.EmitDelegate<Func<int, int>>(modDashTrailCounter);
            cursor.Emit(OpCodes.Stfld, typeof(Player).GetField("dashTrailCounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
        }

        private int modDashTrailCounter(int dashTrailCounter) {
            if (Settings.DashLength != 10 && lastDashDuration != 0f) {
                float bakLastDashDuration = lastDashDuration;
                lastDashDuration = 0f;
                return (int)Math.Round(bakLastDashDuration * Settings.DashLength) - 1;
            }
            return dashTrailCounter;
        }
    }
}
