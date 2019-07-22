using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using Monocle;
using FMOD.Studio;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using On.Celeste;

namespace Celeste.Mod.ExtendedVariants {
    public class ExtendedVariantsModule : EverestModule {

        public static ExtendedVariantsModule Instance;

        public override Type SettingsType => typeof(ExtendedVariantsSettings);
        public static ExtendedVariantsSettings Settings => (ExtendedVariantsSettings)Instance._Settings;

        public static Dictionary<Variant, int> OverridenVariantsInRoom = new Dictionary<Variant, int>();
        public static Dictionary<Variant, int> OldVariantsInRoom = new Dictionary<Variant, int>();
        public static Dictionary<Variant, int> OldVariantsInSession = new Dictionary<Variant, int>();

        public static TextMenu.Option<bool> MasterSwitchOption;
        public static TextMenu.Option<int> GravityOption;
        public static TextMenu.Option<int> JumpHeightOption;
        public static TextMenu.Option<int> SpeedXOption;
        public static TextMenu.Option<int> StaminaOption;
        public static TextMenu.Option<int> DashSpeedOption;
        public static TextMenu.Option<int> DashCountOption;
        public static TextMenu.Option<int> FrictionOption;
        public static TextMenu.Item ResetToDefaultOption;

        public ExtendedVariantsModule() {
            Instance = this;
        }

        // ================ Options menu handling ================

        /// <summary>
        /// List of options shown for multipliers.
        /// </summary>
        private static int[] multiplierScale = new int[] {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100, 250, 500, 1000
        };

        /// <summary>
        /// Formats a multiplier (with no decimal point if not required).
        /// </summary>
        private static Func<int, string> multiplierFormatter = option => {
            option = multiplierScale[option];
            if (option % 10 == 0) {
                return $"{option / 10f:n0}x";
            }
            return $"{option / 10f:n1}x";
        };

        /// <summary>
        /// Finds out the index of a multiplier in the multiplierScale table.
        /// If it is not present, will return the previous option.
        /// (For example, 18x will return the index for 10x.)
        /// </summary>
        /// <param name="option">The multiplier</param>
        /// <returns>The index of the multiplier in the multiplierScale table</returns>
        private static int indexFromMultiplier(int option) {
            for(int index = 0; index < multiplierScale.Length - 1; index++) {
                if(multiplierScale[index + 1] > option) {
                    return index;
                }
            }

            return multiplierScale.Length - 1;
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            base.CreateModMenuSection(menu, inGame, snapshot);

            // create every option
            GravityOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_GRAVITY"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.Gravity)).Change(i => Settings.Gravity = multiplierScale[i]);
            JumpHeightOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_JUMPHEIGHT"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.JumpHeight)).Change(i => Settings.JumpHeight = multiplierScale[i]);
            SpeedXOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_SPEEDX"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.SpeedX)).Change(i => Settings.SpeedX = multiplierScale[i]);
            StaminaOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_STAMINA"),
                i => $"{i * 10}", 0, 50, Settings.Stamina).Change(i => Settings.Stamina = i);
            DashSpeedOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHSPEED"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.DashSpeed)).Change(i => Settings.DashSpeed = multiplierScale[i]);
            DashCountOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHCOUNT"), i => {
                if (i == -1) {
                    return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DEFAULT");
                }
                return i.ToString();
            }, -1, 5, Settings.DashCount).Change(i => Settings.DashCount = i);
            FrictionOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FRICTION"),
                i => {
                    switch (i) {
                        case -1: return "0x";
                        case 0: return "0.05x";
                        default: return multiplierFormatter(i);
                    }
                }, -1, multiplierScale.Length - 1, Settings.Friction == -1 ? -1 : indexFromMultiplier(Settings.Friction))
                .Change(i => Settings.Friction = (i == -1 ? -1 : multiplierScale[i]));

            // create the "master switch" option with specific enable/disable handling.
            MasterSwitchOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_MASTERSWITCH"), Settings.MasterSwitch)
                .Change(v => {
                    Settings.MasterSwitch = v;
                    if (!v) {
                        // We are disabling extended variants: reset values to their defaults.
                        resetToDefaultSettings();
                        refreshOptionMenuValues();
                    }

                    refreshOptionMenuEnabledStatus();
                });

            // Add a button to easily revert to default values
            ResetToDefaultOption = new TextMenu.Button(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_RESETTODEFAULT")).Pressed(() => {
                resetToDefaultSettings();
                refreshOptionMenuValues();
            });

            refreshOptionMenuEnabledStatus();

            menu.Add(MasterSwitchOption);
            menu.Add(GravityOption);
            menu.Add(JumpHeightOption);
            menu.Add(SpeedXOption);
            menu.Add(StaminaOption);
            menu.Add(DashSpeedOption);
            menu.Add(DashCountOption);
            menu.Add(FrictionOption);
            menu.Add(ResetToDefaultOption);
        }

        private static void resetToDefaultSettings() {
            Settings.Gravity = 10;
            Settings.JumpHeight = 10;
            Settings.SpeedX = 10;
            Settings.Stamina = 11;
            Settings.DashSpeed = 10;
            Settings.DashCount = -1;
            Settings.Friction = 10;
        }

        private static void refreshOptionMenuValues() {
            setValue(GravityOption, 0, indexFromMultiplier(Settings.Gravity));
            setValue(JumpHeightOption, 0, indexFromMultiplier(Settings.JumpHeight));
            setValue(SpeedXOption, 0, indexFromMultiplier(Settings.SpeedX));
            setValue(StaminaOption, 0, Settings.Stamina);
            setValue(DashSpeedOption, 0, indexFromMultiplier(Settings.DashSpeed));
            setValue(DashCountOption, -1, Settings.DashCount);
            setValue(FrictionOption, -1, Settings.Friction == -1 ? -1 : indexFromMultiplier(Settings.Friction));
        }

        private static void refreshOptionMenuEnabledStatus() {
            GravityOption.Disabled = !Settings.MasterSwitch;
            JumpHeightOption.Disabled = !Settings.MasterSwitch;
            SpeedXOption.Disabled = !Settings.MasterSwitch;
            StaminaOption.Disabled = !Settings.MasterSwitch;
            DashCountOption.Disabled = !Settings.MasterSwitch;
            DashSpeedOption.Disabled = !Settings.MasterSwitch;
            FrictionOption.Disabled = !Settings.MasterSwitch;
            ResetToDefaultOption.Disabled = !Settings.MasterSwitch;
        }

        private static void setValue(TextMenu.Option<int> option, int min, int newValue) {
            newValue -= min;

            if(newValue != option.Index) {
                // replicate the vanilla behaviour
                option.PreviousIndex = option.Index;
                option.Index = newValue;
                option.ValueWiggler.Start();
            }
        }

        // ================ Module loading ================

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
            IL.Celeste.Player.Jump += ModJump;
            On.Celeste.AreaComplete.VersionNumberAndVariants += ModVersionNumberAndVariants;
            Everest.Events.Level.OnLoadEntity += new Everest.Events.Level.LoadEntityHandler(OnLoadEntity);
            Everest.Events.Player.OnSpawn += OnPlayerSpawn;
            Everest.Events.Level.OnTransitionTo += OnLevelTransitionTo;
            Everest.Events.Level.OnEnter += OnLevelEnter;
            Everest.Events.Level.OnExit += OnLevelExit;
            On.Celeste.SaveData.TryDelete += OnSaveDataDelete;
            IL.Celeste.ChangeRespawnTrigger.OnEnter += ModRespawnTrigger;

            // if master switch is disabled, ensure all values are the default ones. (variants are disabled even if the yml file has been edited.)
            if (!Settings.MasterSwitch) {
                resetToDefaultSettings();
            }
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
            IL.Celeste.Player.Jump -= ModJump;
            On.Celeste.AreaComplete.VersionNumberAndVariants -= ModVersionNumberAndVariants;
            Everest.Events.Level.OnLoadEntity -= new Everest.Events.Level.LoadEntityHandler(OnLoadEntity);
            Everest.Events.Player.OnSpawn -= OnPlayerSpawn;
            Everest.Events.Level.OnTransitionTo -= OnLevelTransitionTo;
            Everest.Events.Level.OnEnter -= OnLevelEnter;
            Everest.Events.Level.OnExit -= OnLevelExit;
            On.Celeste.SaveData.TryDelete -= OnSaveDataDelete;
            IL.Celeste.ChangeRespawnTrigger.OnEnter -= ModRespawnTrigger;

            moddedMethods.Clear();
        }

        // ================ Extended Variant Trigger handling ================

        /// <summary>
        /// Restore extended variants values when entering a saved level.
        /// </summary>
        /// <param name="session">unused</param>
        /// <param name="fromSaveData">true if loaded from save data, false otherwise</param>
        public void OnLevelEnter(Session session, bool fromSaveData) {
            int slot = SaveData.Instance.FileSlot;
            if (fromSaveData && Settings.OverrideValuesInSave.ContainsKey(slot)) {
                // reset all variants that got set in the room
                Dictionary<Variant, int> values = Settings.OverrideValuesInSave[slot];
                foreach (Variant v in values.Keys) {
                    Logger.Log("ExtendedVariantsModule/OnLevelEnter", $"Loading save {slot}: restoring {v} to {values[v]}");
                    int oldValue = ExtendedVariantTrigger.SetVariantValue(v, values[v]);
                    OldVariantsInSession[v] = oldValue;
                }
            }
        }

        /// <summary>
        /// Handles ExtendedVariantTrigger constructing when loading a level.
        /// </summary>
        /// <param name="level">The level being loaded</param>
        /// <param name="levelData">unused</param>
        /// <param name="offset">offset passed to the trigger</param>
        /// <param name="entityData">the entity parameters</param>
        /// <returns>true if the trigger was loaded, false otherwise</returns>
        public bool OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if(entityData.Name == "ExtendedVariantTrigger") {
                level.Add(new ExtendedVariantTrigger(entityData, offset));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle respawn (reset variants that were set in the room).
        /// </summary>
        /// <param name="obj">unused</param>
        public void OnPlayerSpawn(Player obj) {
            if (OldVariantsInRoom.Count != 0) {
                // reset all variants that got set in the room
                foreach (Variant v in OldVariantsInRoom.Keys) {
                    Logger.Log("ExtendedVariantsModule/OnPlayerSpawn", $"Died in room: resetting {v} to {OldVariantsInRoom[v]}");
                    ExtendedVariantTrigger.SetVariantValue(v, OldVariantsInRoom[v]);
                }

                // clear values
                Logger.Log("ExtendedVariantsModule/OnPlayerSpawn", "Room state reset");
                OldVariantsInRoom.Clear();
                OverridenVariantsInRoom.Clear();
            }
        }

        /// <summary>
        /// Handle screen transitions (make variants set within the room permanent).
        /// </summary>
        /// <param name="level">unused</param>
        /// <param name="next">unused</param>
        /// <param name="direction">unused</param>
        public void OnLevelTransitionTo(Level level, LevelData next, Vector2 direction) {
            CommitVariantChanges();
        }

        /// <summary>
        /// Edits the OnEnter method in ChangeRespawnTrigger, so that the variants set are made permanent when the respawn point is changed.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModRespawnTrigger(ILContext il) {
            ModMethod("RespawnTrigger", () => {
                ILCursor cursor = new ILCursor(il);

                // simply jump into the "if" controlling whether the respawn should be changed or not
                if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Brtrue_S)) {
                    // and call our method in there
                    Logger.Log("ExtendedVariantsModule", $"Inserting call to CommitVariantChanges at index {cursor.Index} in CIL code for OnEnter in ChangeRespawnTrigger");
                    cursor.EmitDelegate<Action>(CommitVariantChanges);
                }
            });
        }

        /// <summary>
        /// Make the changes in variant settings permanent (even if the player dies).
        /// </summary>
        public static void CommitVariantChanges() {
            if (OverridenVariantsInRoom.Count != 0) {
                int fileSlot = SaveData.Instance.FileSlot;

                // create slot if not present
                if (!Settings.OverrideValuesInSave.ContainsKey(fileSlot)) {
                    Logger.Log("ExtendedVariantsModule/CommitVariantChanges", $"Creating save slot {fileSlot}");
                    Settings.OverrideValuesInSave[fileSlot] = new Dictionary<Variant, int>();
                }

                // "commit" variants set in the room to save slot
                foreach (Variant v in OverridenVariantsInRoom.Keys) {
                    Logger.Log("ExtendedVariantsModule/CommitVariantChanges", $"Committing variant change {v} to {OverridenVariantsInRoom[v]} in save file slot {fileSlot}");
                    Settings.OverrideValuesInSave[fileSlot][v] = OverridenVariantsInRoom[v];
                }

                // clear values
                Logger.Log("ExtendedVariantsModule/CommitVariantChanges", "Room state reset");
                OldVariantsInRoom.Clear();
                OverridenVariantsInRoom.Clear();
            }
        }

        public void OnLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            int fileSlot = SaveData.Instance.FileSlot;
            if (mode != LevelExit.Mode.SaveAndQuit && Settings.OverrideValuesInSave.ContainsKey(fileSlot)) {
                // we definitely exited the level: reset the variants state
                Logger.Log("ExtendedVariantsModule/OnLevelExit", $"Removing all variant changes in save file slot {fileSlot}");
                Settings.OverrideValuesInSave.Remove(fileSlot);
            }

            if (OldVariantsInSession.Count != 0) {
                // reset all variants that got set during the session
                foreach (Variant v in OldVariantsInSession.Keys) {
                    Logger.Log("ExtendedVariantsModule/OnLevelExit", $"Ending session: resetting {v} to {OldVariantsInSession[v]}");
                    ExtendedVariantTrigger.SetVariantValue(v, OldVariantsInSession[v]);
                }
            }

            // exiting level: clear state
            Logger.Log("ExtendedVariantsModule/OnLevelExit", "Room and session state reset");
            OverridenVariantsInRoom.Clear();
            OldVariantsInRoom.Clear();
            OldVariantsInSession.Clear();

            // make sure to save the settings
            Logger.Log("ExtendedVariantsModule/OnLevelExit", "Saving variant module settings");
            SaveSettings();
        }

        /// <summary>
        /// Wraps the TryDelete method in SaveData, in order to handle the corner case where a save data with a session containing extended variants is deleted.
        /// (Pretty sure it will never happen, but still, this would cause weird behavior.)
        /// </summary>
        /// <param name="orig">The original TryDelete method</param>
        /// <param name="slot">The slot being deleted</param>
        /// <returns></returns>
        public bool OnSaveDataDelete(On.Celeste.SaveData.orig_TryDelete orig, int slot) {
            bool success = orig.Invoke(slot);

            if(success && Settings.OverrideValuesInSave.ContainsKey(slot)) {
                Logger.Log("ExtendedVariantsModule/OnSaveDataDelete", $"Removing all variant changes in save file slot {slot} since save file was just deleted");
                Settings.OverrideValuesInSave.Remove(slot);
                SaveSettings();
            }

            return success;
        }

        // ================ Stamp on Chapter Complete screen ================

        /// <summary>
        /// Wraps the VersionNumberAndVariants in the base game in order to add the Variant Mode logo if Extended Variants are enabled.
        /// </summary>
        public static void ModVersionNumberAndVariants(On.Celeste.AreaComplete.orig_VersionNumberAndVariants orig, string version, float ease, float alpha) {
            if(Settings.MasterSwitch) {
                // The "if" conditioning the display of the Variant Mode logo is in an "orig_" method, we can't access it with IL.Celeste.
                // The best we can do is turn on Variant Mode, run the method then restore its original value.
                bool oldVariantModeValue = SaveData.Instance.VariantMode;
                SaveData.Instance.VariantMode = true;

                orig.Invoke(version, ease, alpha);

                SaveData.Instance.VariantMode = oldVariantModeValue;
            } else {
                // Extended Variants are disabled so just keep the original behaviour
                orig.Invoke(version, ease, alpha);
            }
        }

        // ================ Utility methods for IL modding ================

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

                // chain every other NormalUpdate usage
                ModNormalUpdateSpeedX(il);
                ModNormalUpdateFriction(il);
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

                // chain every other UpdateSprite usage
                ModUpdateSpriteFriction(il);
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

        // ================ Ground friction handling ================

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.) to apply ground friction.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModNormalUpdateFriction(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to the 500 in "this.Speed.X = Calc.Approach(this.Speed.X, 0f, 500f * Engine.DeltaTime);"
            if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 500f)) {
                Logger.Log("ExtendedVariantsModule", $"Applying friction to constant at {cursor.Index} (ducking stop speed on ground) in CIL code for NormalUpdate");

                cursor.EmitDelegate<Func<float>>(DetermineFrictionFactor);
                cursor.Emit(OpCodes.Mul);
            }

            // jump to "float num = this.onGround ? 1f : 0.65f;" by jumping to 0.65 then 1 (the numbers are swapped in the IL code)
            if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 0.65f)
                && cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 1f)) {

                Logger.Log("ExtendedVariantsModule", $"Applying friction to constant at {cursor.Index} (friction factor on ground) in CIL code for NormalUpdate");

                // 1 is the acceleration when on the ground. Apply the friction factor to it.
                cursor.EmitDelegate<Func<float>>(DetermineFrictionFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Edits the UpdateSprite method in Player (updating the player animation) to fix the animations when using modded friction.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModUpdateSpriteFriction(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're jumping to this line: "if (Math.Abs(this.Speed.X) <= 25f && this.moveX == 0)"
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 25f)) {
                Logger.Log("ExtendedVariantsModule", $"Modding constant at {cursor.Index} in CIL code for UpdateSprite to fix animation with friction");

                // call our method which will essentially replace the 25 with whatever value we want
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate<Func<float>>(GetIdleAnimationThreshold);
            }
        }

        /// <summary>
        /// Compute the idle animation threshold (when the player lets go every button, Madeline will use the walking animation until
        /// her X speed gets below this value. Under this value, she will use her idle animation.)
        /// </summary>
        /// <returns>The idle animation threshold (minimum 25, gets higher as the friction factor is lower)</returns>
        public static float GetIdleAnimationThreshold() {
            if(Settings.FrictionFactor >= 1f) {
                // keep the default value
                return 25f;
            }

            // shift the "stand still" threshold towards max walking speed, which is 90f
            // for example, it will give 83.5 when friction factor is 0.1, Madeline will appear to slip standing still.
            return 25f + (90f * Settings.SpeedXFactor - 25f) * (1 - Settings.FrictionFactor);
        }

        /// <summary>
        /// Returns the currently configured friction factor.
        /// </summary>
        /// <returns>The friction factor (1 = default friction)</returns>
        public static float DetermineFrictionFactor() {
            return Settings.FrictionFactor;
        }

        // ================ Jump height handling ================

        /// <summary>
        /// Edits the Jump method in Player (called when jumping, simply.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModJump(ILContext il) {
            ModMethod("Jump", () => {
                ILCursor cursor = new ILCursor(il);

                // the speed applied to jumping is simply -105f (negative = up). Let's multiply this with our jump height factor.
                while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == -105f)) {
                    Logger.Log("ExtendedVariantsModule", $"Modding constant at {cursor.Index} in CIL code for Jump to make jump height editable");

                    // add two instructions to multiply -105f with the "jump height factor"
                    cursor.EmitDelegate<Func<float>>(DetermineJumpHeightFactor);
                    cursor.Emit(OpCodes.Mul);
                }

                // chain every other UpdateSprite usage
                ModUpdateSpriteFriction(il);
            });
        }

        /// <summary>
        /// Returns the currently configured jump height factor.
        /// </summary>
        /// <returns>The jump height factor (1 = default jump height)</returns>
        public static float DetermineJumpHeightFactor() {
            return Settings.JumpHeightFactor;
        }
    }
}
