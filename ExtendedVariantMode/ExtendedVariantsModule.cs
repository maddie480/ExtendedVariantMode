using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using Monocle;
using FMOD.Studio;
using Celeste.Mod.UI;

namespace Celeste.Mod.ExtendedVariants {
    public class ExtendedVariantsModule : EverestModule {

        public static ExtendedVariantsModule Instance;

        public override Type SettingsType => typeof(ExtendedVariantsSettings);
        public static ExtendedVariantsSettings Settings => (ExtendedVariantsSettings)Instance._Settings;

        public ExtendedVariantsModule() {
            Instance = this;
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            base.CreateModMenuSection(menu, inGame, snapshot);

            // Add a button to easily revert to default values
            menu.Add(new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESETTODEFAULT")).Pressed(() => {
                Settings.Gravity = 10;
                Settings.SpeedX = 10;
                Settings.Stamina = 11;
                Settings.DashCount = -1;
                Settings.DashSpeed = 10;

                // updating displayed values sounds like a pain, so let's just close the menu instead.
                if (inGame) {
                    menu.OnCancel();
                } else {
                    OuiModOptions.Instance.Overworld.Goto<OuiMainMenu>();
                }
            }));
        }

        public override void Load() {
            // mod methods here
            IL.Celeste.Player.NormalUpdate += ModNormalUpdate;
            IL.Celeste.Player.ClimbUpdate += ModClimbUpdate;
            On.Celeste.Player.RefillStamina += ModRefillStamina;
            IL.Celeste.Player.SwimBegin += ModSwimBegin;
            IL.Celeste.Player.DreamDashBegin += ModDreamDashBegin;
            On.Celeste.Player.Update += ModUpdate;
            IL.Celeste.Player.ctor += ModPlayerConstructor;
            IL.Celeste.Player.UpdateSprite += ModUpdateSprite;
            On.Celeste.Player.RefillDash += ModRefillDash;
            IL.Celeste.Player.UseRefill += ModUseRefill;
            On.Celeste.Player.Added += ModAdded;
            IL.Celeste.Player.CallDashEvents += ModCallDashEvents;
            IL.Celeste.Player.UpdateHair += ModUpdateHair;
        }

        public override void Unload() {
            // unmod methods here
            IL.Celeste.Player.NormalUpdate -= ModNormalUpdate;
            IL.Celeste.Player.ClimbUpdate -= ModClimbUpdate;
            On.Celeste.Player.RefillStamina -= ModRefillStamina;
            IL.Celeste.Player.SwimBegin -= ModSwimBegin;
            IL.Celeste.Player.DreamDashBegin -= ModDreamDashBegin;
            On.Celeste.Player.Update -= ModUpdate;
            IL.Celeste.Player.ctor -= ModPlayerConstructor;
            IL.Celeste.Player.UpdateSprite -= ModUpdateSprite;
            On.Celeste.Player.RefillDash -= ModRefillDash;
            IL.Celeste.Player.UseRefill -= ModUseRefill;
            On.Celeste.Player.Added -= ModAdded;
            IL.Celeste.Player.CallDashEvents -= ModCallDashEvents;
            IL.Celeste.Player.UpdateHair -= ModUpdateHair;

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
                Logger.Log("ExtendedVariantsModule", $"> Method {methodName} already patched");
            } else {
                Logger.Log("ExtendedVariantsModule", $"> Patching method {methodName}");
                patcher.Invoke();
                moddedMethods.Add(methodName);
            }
        }

        // ================ Gravity handling ================

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

                    Logger.Log("ExtendedVariantsModule", $"Applying gravity to constant at {cursor.Index} in CIL code for NormalUpdate");

                    // add two instructions to multiply those constants with the "gravity factor"
                    cursor.EmitDelegate<Func<float>>(DetermineGravityFactor);
                    cursor.Emit(OpCodes.Mul);
                }

                ModNormalUpdateSpeedX(il);
            });
        }

        /// <summary>
        /// Edits the UpdateSprite method in Player (updating the player animation.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModUpdateSprite(ILContext il) {
            ModMethod("UpdateSprite", () => {
                ILCursor cursor = new ILCursor(il);

                // the goal is to multiply 160 (max falling speed) with the gravity factor to fix the falling animation
                // let's search for all 160 occurrences in the IL code
                while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 160f)) {
                    Logger.Log("ExtendedVariantsModule", $"Applying gravity to constant at {cursor.Index} in CIL code for UpdateSprite to fix animation");

                    // add two instructions to multiply those constants with the "gravity factor"
                    cursor.EmitDelegate<Func<float>>(DetermineGravityFactor);
                    cursor.Emit(OpCodes.Mul);
                    // also remove 0.1 to prevent an animation glitch caused by rounding (I guess?) on very low gravity
                    cursor.Emit(OpCodes.Ldc_R4, 0.1f);
                    cursor.Emit(OpCodes.Sub);
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

                // we will sneak our method call after "num" gets loaded on this line
                // this.Speed.Y = Calc.Approach(this.Speed.Y, num, 900f * Engine.DeltaTime);
                // "num" is loaded just before 900 is loaded via ldc.r4 900
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldc_R4 && (float) instr.Operand == 900f)) {
                    Logger.Log("ExtendedVariantsModule", $"Injecting method at index {cursor.Index} in CIL code for ClimbUpdate to apply gravity when climbing");

                    // now call the method, it will update "num" just before Calc.Approach gets called
                    cursor.EmitDelegate<Func<float, float>>(ModClimbSpeed);
                }

                patchOutStamina(il);
            });
        }

        /// <summary>
        /// Returns the currently configured gravity factor.
        /// </summary>
        /// <returns>The gravity factor (1 = default gravity)</returns>
        public static float DetermineGravityFactor() {
            return Settings.GravityFactor;
        }

        /// <summary>
        /// Computes the climb speed based on gravity.
        /// </summary>
        /// <param name="initialValue">The initial climb speed computed by the vanilla method</param>
        /// <returns>The modded climb speed</returns>
        public static float ModClimbSpeed(float initialValue) {
            if (initialValue > 0) {
                // climbing down: apply gravity
                return initialValue * Settings.GravityFactor;
            } else {
                // climbing up: apply reverse gravity
                return initialValue * (1 / Settings.GravityFactor);
            }
        }


        // ================ X speed handling ================

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.)
        /// to handle the X speed part.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModNormalUpdateSpeedX(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we use 90 as an anchor (an "if" before the instruction we want to mod loads 90 in the stack)
            // then we jump to the next usage of V_6 to get the reference to it (no idea how to build it otherwise)
            if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 90f)
                && cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Stloc_S && ((VariableDefinition)instr.Operand).Index == 6)) {

                VariableDefinition variable = (VariableDefinition) cursor.Next.Operand;

                // we jump before the next ldflda, which is between the "if (this.level.InSpace)" and the next one
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldflda)) {
                    Logger.Log("ExtendedVariantsModule", $"Applying X speed modding to variable {variable.ToString()} at {cursor.Index} in CIL code for NormalUpdate");

                    // pop ldarg.0
                    cursor.Emit(OpCodes.Pop);

                    // modify variable 6 to apply X factor
                    cursor.Emit(OpCodes.Ldloc_S, variable);
                    cursor.EmitDelegate<Func<float>>(DetermineSpeedXFactor);
                    cursor.Emit(OpCodes.Mul);
                    cursor.Emit(OpCodes.Stloc_S, variable);

                    // execute ldarg.0 again
                    cursor.Emit(OpCodes.Ldarg_0);
                }
            }
        }

        /// <summary>
        /// Returns the currently configured X speed factor.
        /// </summary>
        /// <returns>The speed factor (1 = default speed)</returns>
        public static float DetermineSpeedXFactor() {
            return Settings.SpeedXFactor;
        }

        // ================ Stamina handling ================

        /// <summary>
        /// Edits the SwimBegin method in Player (handling the player state when starting to swim).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModSwimBegin(ILContext il) {
            ModMethod("SwimBegin", () => {
                patchOutStamina(il);
            });
        }

        /// <summary>
        /// Edits the DreamDashBegin method in Player (handling the player state when entering a dream dash block).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModDreamDashBegin(ILContext il) {
            ModMethod("DreamDashBegin", () => {
                patchOutStamina(il);
            });
        }

        /// <summary>
        /// Edits the constructor of Player.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModPlayerConstructor(ILContext il) {
            ModMethod("PlayerConstructor", () => {
                patchOutStamina(il);
            });
        }

        /// <summary>
        /// Wraps the Update method in the base game (used to refresh the player state).
        /// </summary>
        /// <param name="orig">The original Update method</param>
        /// <param name="self">The Player instance</param>
        public static void ModUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            // since we cannot patch IL in orig_Update, we will wrap it and try to guess if the stamina was reset
            // this is **certainly** the case if the stamina changed and is now 110
            float staminaBeforeCall = self.Stamina;
            orig.Invoke(self);
            if (self.Stamina == 110f && staminaBeforeCall != 110f) {
                // reset it to the value we chose instead of 110
                self.Stamina = DetermineBaseStamina();
            }
        }

        /// <summary>
        /// Replaces the default 110 stamina value with the one defined in the settings.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private static void patchOutStamina(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            // now, patch everything stamina-related (every instance of 110)
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 110f)) {
                Logger.Log("ExtendedVariantsModule", $"Patching stamina at index {cursor.Index} in CIL code");

                // pop the 110 and call our method instead
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate<Func<float>>(DetermineBaseStamina);
            }
        }

        /// <summary>
        /// Replaces the RefillStamina in the base game.
        /// </summary>
        /// <param name="orig">The original RefillStamina method</param>
        /// <param name="self">The Player instance</param>
        public static void ModRefillStamina(On.Celeste.Player.orig_RefillStamina orig, Player self) {
            self.Stamina = DetermineBaseStamina();
        }

        /// <summary>
        /// Returns the max stamina.
        /// </summary>
        /// <returns>The max stamina (default 110)</returns>
        public static float DetermineBaseStamina() {
            return Settings.Stamina * 10f;
        }

        // ================ Dash speed handling ================

        /// <summary>
        /// Edits the CallDashEvents method in Player (called multiple times when the player dashes).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModCallDashEvents(ILContext il) {
            ModMethod("CallDashEvents", () => {
                ILCursor cursor = new ILCursor(il);

                // enter the 2 ifs in the method and inject ourselves in there
                if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Brtrue) && 
                    cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Brtrue)) {
                    Logger.Log("ExtendedVariantsModule", $"Adding code to mod dash speed at index {cursor.Index} in CIL code for CallDashEvents");

                    // just add a call to ModifyDashSpeed (arg 0 = this)
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.EmitDelegate<Action<Player>>(ModifyDashSpeed);
                }
            });
        }

        /// <summary>
        /// Modifies the dash speed of the player.
        /// </summary>
        /// <param name="self">A reference to the player</param>
        public static void ModifyDashSpeed(Player self) {
            self.Speed *= Settings.DashSpeedFactor;
        }

        // ================ Dash count handling ================

        /// <summary>
        /// Replaces the RefillDash in the base game.
        /// </summary>
        /// <param name="orig">The original RefillDash method</param>
        /// <param name="self">The Player instance</param>
        public static bool ModRefillDash(On.Celeste.Player.orig_RefillDash orig, Player self) {
            if (Settings.DashCount == -1) {
                return orig.Invoke(self);
            } else if(self.Dashes < Settings.DashCount) {
                self.Dashes = Settings.DashCount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Edits the UseRefill method in Player (called when the player gets a refill, obviously.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModUseRefill(ILContext il) {
            ModMethod("UseRefill", () => {
                ILCursor cursor = new ILCursor(il);

                // we want to insert ourselves just before the first stloc.0
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Stloc_0)) {
                    Logger.Log("ExtendedVariantsModule", $"Modding dash count given by refills at {cursor.Index} in CIL code for UseRefill");

                    // call our method just before storing the result from get_MaxDashes in local variable 0
                    cursor.EmitDelegate<Func<int, int>>(DetermineDashCount);
                }
            });
        }

        /// <summary>
        /// Edits the UpdateHair method in Player (mainly computing the hair color).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModUpdateHair(ILContext il) {
            ModMethod("UpdateHair", () => {
                ILCursor cursor = new ILCursor(il);

                // the goal here is to turn "this.Dashes == 2" checks into "this.Dashes >= 2" to make it look less weird
                // and be more consistent with the behaviour of the "Infinite Dashes" variant.
                // (without this patch, with > 2 dashes, Madeline's hair is red, then turns pink, then red again before becoming blue)
                while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_I4_2 && instr.Next.OpCode == OpCodes.Bne_Un_S)) {
                    Logger.Log("ExtendedVariantsModule", $"Fixing hair color when having more than 2 dashes by modding a check at {cursor.Index} in CIL code for UpdateHair");

                    // small trap: the instruction in CIL code actually says "jump if **not** equal to 2". So we set it to "jump if lower than 2" instead
                    cursor.Next.OpCode = OpCodes.Blt_Un_S;
                }
            });
        }

        /// <summary>
        /// Wraps the Added method in the base game (used to initialize the player state).
        /// </summary>
        /// <param name="orig">The original Added method</param>
        /// <param name="self">The Player instance</param>
        /// <param name="scene">Argument of the original method (passed as is)</param>
        public static void ModAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig.Invoke(self, scene);
            self.Dashes = DetermineDashCount(self.Dashes);
        }

        /// <summary>
        /// Returns the dash count.
        /// </summary>
        /// <param name="defaultValue">The default value (= Player.MaxDashes)</param>
        /// <returns>The dash count</returns>
        public static int DetermineDashCount(int defaultValue) {
            if (Settings.DashCount == -1) {
                return defaultValue;
            }
            return Settings.DashCount;
        }
    }
}
