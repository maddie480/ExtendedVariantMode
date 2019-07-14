using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;

namespace Celeste.Mod.ExtendedVariants {
    public class ExtendedVariantsModule : EverestModule {

        public static ExtendedVariantsModule Instance;

        public override Type SettingsType => typeof(ExtendedVariantsSettings);
        public static ExtendedVariantsSettings Settings => (ExtendedVariantsSettings)Instance._Settings;

        public ExtendedVariantsModule() {
            Instance = this;
        }

        public override void Load() {
            // mod methods here
            IL.Celeste.Player.NormalUpdate += ModNormalUpdate;
            IL.Celeste.Player.ClimbUpdate += ModClimbUpdate;
        }

        public override void Unload() {
            // unmod methods here
            IL.Celeste.Player.NormalUpdate -= ModNormalUpdate;
            IL.Celeste.Player.ClimbUpdate -= ModClimbUpdate;

            moddedMethods.Clear();
        }

        /// <summary>
        /// Keeps track of already patched methods.
        /// </summary>
        private static HashSet<string> moddedMethods = new HashSet<string>();

        /// <summary>
        /// Utility method to prevent methods from getting patched multiple times.
        /// </summary>
        /// <param name="methodName">Name of the patched method</param>
        /// <param name="patcher">Action to run in order to patch method</param>
        private static void ModMethod(string methodName, Action patcher) {
            // for whatever reason mod methods are called multiple times: only patch the methods once
            if (moddedMethods.Contains(methodName)) {
                Logger.Log("ExtendedVariantsModule", $"Method {methodName} already patched");
            } else {
                Logger.Log("ExtendedVariantsModule", $"Patching method {methodName}");
                patcher.Invoke();
                moddedMethods.Add(methodName);
            }
        }

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModNormalUpdate(ILContext il) {
            ModMethod("NormalUpdate", () => {
                ILCursor cursor = new ILCursor(il);

                // we will edit 3 constants here:
                // * 160 = max falling speed
                // * 240 = max falling speed when holding Down
                // * 900 = downward acceleration

                // find out where those constants are loaded into the stack
                while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4
                     && ((float)instr.Operand == 160f || (float)instr.Operand == 240f || (float)instr.Operand == 900f))) {

                    Logger.Log("ExtendedVariantsModule", $"I found a constant to edit at {cursor.Index} in CIL code for NormalUpdate");

                    // add two instructions to multiply those constants with the "gravity factor"
                    cursor.EmitDelegate<Func<float>>(DetermineGravityFactor);
                    cursor.Emit(OpCodes.Mul);
                }
            });
        }

        /// <summary>
        /// Edits the ClimbUpdate method in Player (handling the player state when climbing).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModClimbUpdate(ILContext il) {
            ModMethod("ClimbUpdate", () => {
                ILCursor cursor = new ILCursor(il);

                // we will sneak our method call when "num" gets loaded on this line
                // this.Speed.Y = Calc.Approach(this.Speed.Y, num, 900f * Engine.DeltaTime);
                // "num" is loaded just before 900 is loaded via ldc.r4 900
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldc_R4 && (float) instr.Operand == 900f)) {
                    Logger.Log("ExtendedVariantsModule", $"Injecting method at index {cursor.Index} in CIL code for ClimbUpdate");

                    // now call the method, it will update "num" just before Calc.Approach gets called
                    cursor.EmitDelegate<Func<float, float>>(ModClimbSpeed);
                }
            });
        }

        /// <summary>
        /// Computes the climb speed based on gravity.
        /// </summary>
        /// <param name="initialValue">The initial climb speed computed by the vanilla method</param>
        /// <returns>The modded climb speed</returns>
        public static float ModClimbSpeed(float initialValue) {
            if(initialValue > 0) {
                // climbing down: apply gravity
                return initialValue * Settings.GravityFactor;
            } else {
                // climbing up: apply reverse gravity
                return initialValue * (1 / Settings.GravityFactor);
            }
        }

        /// <summary>
        /// Returns the currently configured gravity factor.
        /// </summary>
        /// <returns>The gravity factor (1 = default gravity)</returns>
        public static float DetermineGravityFactor() {
            return Settings.GravityFactor;
        }
    }
}
