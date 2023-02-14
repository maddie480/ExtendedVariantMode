using Celeste;
using Celeste.Mod;
using Celeste.Mod.XaphanHelper.Managers;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using ExtendedVariants.Variants.Vanilla;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExtendedVariants {
    public class ExtendedVariantTriggerManager {

        private Dictionary<ExtendedVariantsModule.Variant, object> overridenVariantsInRoom = new Dictionary<ExtendedVariantsModule.Variant, object>();
        private Dictionary<ExtendedVariantsModule.Variant, object> overridenVariantsInRoomRevertOnLeave = new Dictionary<ExtendedVariantsModule.Variant, object>();
        private Dictionary<ExtendedVariantsModule.Variant, object> oldVariantsInRoom = new Dictionary<ExtendedVariantsModule.Variant, object>();
        private Dictionary<ExtendedVariantsModule.Variant, object> variantValuesBeforeOverride = new Dictionary<ExtendedVariantsModule.Variant, object>();

        private static Hook hookOnXaphanHelperTeleport;

        public void Load() {
            Everest.Events.Level.OnEnter += onLevelEnter;
            Everest.Events.Player.OnSpawn += onPlayerSpawn;
            Everest.Events.Level.OnTransitionTo += onLevelTransitionTo;
            Everest.Events.Level.OnExit += onLevelExit;
            IL.Celeste.ChangeRespawnTrigger.OnEnter += modRespawnTriggerOnEnter;
            On.Celeste.SaveData.AssistModeChecks += modAssistModeChecks;

            if (ExtendedVariantsModule.Instance.XaphanHelperInstalled) {
                hookXaphanHelper();
            }
        }

        public void Unload() {
            Everest.Events.Level.OnEnter -= onLevelEnter;
            Everest.Events.Player.OnSpawn -= onPlayerSpawn;
            Everest.Events.Level.OnTransitionTo -= onLevelTransitionTo;
            Everest.Events.Level.OnExit -= onLevelExit;
            IL.Celeste.ChangeRespawnTrigger.OnEnter -= modRespawnTriggerOnEnter;
            On.Celeste.SaveData.AssistModeChecks -= modAssistModeChecks;

            hookOnXaphanHelperTeleport?.Dispose();
            hookOnXaphanHelperTeleport = null;
        }

        private void hookXaphanHelper() {
            hookOnXaphanHelperTeleport = new Hook(
                typeof(WarpManager).GetMethod("TeleportToChapter", BindingFlags.NonPublic | BindingFlags.Static),
                typeof(ExtendedVariantTriggerManager).GetMethod("resetVariantsOnXaphanTeleport", BindingFlags.NonPublic | BindingFlags.Instance),
                this);
        }

        /// <summary>
        /// Restore extended variants values when entering a saved level.
        /// </summary>
        /// <param name="session">unused</param>
        /// <param name="fromSaveData">true if loaded from save data, false otherwise</param>
        private void onLevelEnter(Session session, bool fromSaveData) {
            // failsafe: if VariantsEnabledViaTrigger is null, initialize it. THIS SHOULD NEVER HAPPEN, but already happened in a case of a corrupted save.
            if ((ExtendedVariantsModule.Session?.VariantsEnabledViaTrigger ?? null) == null) {
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantTriggerManager", "Session was null. This should not happen. Initializing it to an empty session.");
                ExtendedVariantsModule.Instance._Session = new ExtendedVariantsSession();
            }
            if (fromSaveData) {
                foreach (ExtendedVariantsModule.Variant v in ExtendedVariantsModule.Session.VariantsEnabledViaTrigger.Keys.ToList()) {
                    Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Loading save: restoring {v} to {ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[v]}");

                    object oldValue = setVariantValue(v, ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[v], out object fixedValue);
                    ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[v] = fixedValue;

                    variantValuesBeforeOverride[v] = oldValue;
                }
            }
        }

        /// <summary>
        /// Handle respawn (reset variants that were set in the room).
        /// </summary>
        /// <param name="obj">unused</param>
        private void onPlayerSpawn(Player obj) {
            if (oldVariantsInRoom.Count != 0) {
                // reset all variants that got set in the room
                foreach (ExtendedVariantsModule.Variant v in oldVariantsInRoom.Keys) {
                    Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Died in room: resetting {v} to {oldVariantsInRoom[v]}");
                    setVariantValue(v, oldVariantsInRoom[v], out _);
                }

                // clear values
                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", "Room state reset");
                oldVariantsInRoom.Clear();
                overridenVariantsInRoom.Clear();
                overridenVariantsInRoomRevertOnLeave.Clear();
            }
        }

        /// <summary>
        /// Handle screen transitions (make variants set within the room permanent).
        /// </summary>
        /// <param name="level">unused</param>
        /// <param name="next">unused</param>
        /// <param name="direction">unused</param>
        private void onLevelTransitionTo(Level level, LevelData next, Vector2 direction) {
            commitVariantChanges();
        }

        /// <summary>
        /// Edits the OnEnter method in ChangeRespawnTrigger, so that the variants set are made permanent when the respawn point is changed.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        private void modRespawnTriggerOnEnter(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // simply jump into the "if" controlling whether the respawn should be changed or not
            // (yet again, this is brtrue.s in XNA and brfalse.s in FNA. Thanks compiler.)
            if (cursor.TryGotoNext(MoveType.After, instr => (instr.OpCode == OpCodes.Brtrue_S || instr.OpCode == OpCodes.Brfalse_S))) {
                // and call our method in there
                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Inserting call to CommitVariantChanges at index {cursor.Index} in CIL code for OnEnter in ChangeRespawnTrigger");
                cursor.EmitDelegate<Action>(commitVariantChanges);
            }
        }

        private void modAssistModeChecks(On.Celeste.SaveData.orig_AssistModeChecks orig, SaveData self) {
            bool isVanillaVariantUsed = false;

            foreach (ExtendedVariantsModule.Variant v in ExtendedVariantsModule.Session.VariantsEnabledViaTrigger.Keys.ToList()) {
                if (ExtendedVariantsModule.Instance.VariantHandlers[v] is AbstractVanillaVariant) {
                    isVanillaVariantUsed = true;
                    break;
                }
            }

            if (!isVanillaVariantUsed) {
                orig(self);
            }
        }

        /// <summary>
        /// Make the changes in variant settings permanent (even if the player dies).
        /// </summary>
        private void commitVariantChanges() {
            if (overridenVariantsInRoom.Count != 0) {
                // "commit" variants set in the room to save slot
                foreach (ExtendedVariantsModule.Variant v in overridenVariantsInRoom.Keys) {
                    Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Committing variant change {v} to {overridenVariantsInRoom[v]}");
                    ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[v] = overridenVariantsInRoom[v];
                }

                // clear values
                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", "Room state reset");
                oldVariantsInRoom.Clear();
                overridenVariantsInRoom.Clear();
                overridenVariantsInRoomRevertOnLeave.Clear();
            }
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            ResetVariantsOnLevelExit();
        }

        private void resetVariantsOnXaphanTeleport(Action<int> orig, int areaId) {
            if (Engine.Scene is Level) {
                // Xaphan Helper is going to teleport us to another level, and THAT is a level exit. Variants should be reset.
                ResetVariantsOnLevelExit();
            }

            orig(areaId);
        }

        internal void ResetVariantsOnLevelExit() {
            if (variantValuesBeforeOverride.Count != 0) {
                // reset all variants that got set during the session
                foreach (ExtendedVariantsModule.Variant v in variantValuesBeforeOverride.Keys) {
                    Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Ending session: resetting {v} to {variantValuesBeforeOverride[v]}");
                    setVariantValue(v, variantValuesBeforeOverride[v], out _);
                }
            }

            // exiting level: clear state
            Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", "Room and session state reset");
            overridenVariantsInRoom.Clear();
            overridenVariantsInRoomRevertOnLeave.Clear();
            oldVariantsInRoom.Clear();
            variantValuesBeforeOverride.Clear();

            ExtendedVariantsModule.Instance.SaveSettings();
        }

        public object OnEnteredInTrigger(ExtendedVariantsModule.Variant variantChange, object newValue, bool revertOnLeave, bool isFade, bool revertOnDeath, bool legacy) {
            // change the variant value
            object oldValue;
            object actualNewValue;
            if (legacy) {
                oldValue = setLegacyVariantValue(variantChange, (int) newValue, out actualNewValue);
            } else {
                oldValue = setVariantValue(variantChange, newValue, out actualNewValue);
            }

            if (!variantValuesBeforeOverride.ContainsKey(variantChange)) {
                variantValuesBeforeOverride[variantChange] = oldValue;
            }

            if (revertOnDeath || revertOnLeave) {
                // store the fact that the variant was changed within the room
                // so that it can be reverted if we die, or saved if we save & quit later
                // fade triggers get a special tag, because it can very quickly flood logs (1 line per frame) and needs to be turned on only when necessary.
                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager" + (isFade ? "-fade" : ""), $"Triggered ExtendedVariantTrigger: changed {variantChange} from {oldValue} to {newValue} (revertOnLeave = {revertOnLeave}, legacy = {legacy}) => variant set to {actualNewValue}");

                if (!oldVariantsInRoom.ContainsKey(variantChange)) {
                    oldVariantsInRoom[variantChange] = oldValue;
                }
                if (revertOnLeave) {
                    overridenVariantsInRoomRevertOnLeave[variantChange] = actualNewValue;
                } else {
                    overridenVariantsInRoom[variantChange] = actualNewValue;
                }
            } else {
                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Triggered ExtendedVariantTrigger: changed and committed {variantChange} from {oldValue} to {newValue} => variant set to {actualNewValue}");

                // remove the variant from the room state if it was in there...
                oldVariantsInRoom.Remove(variantChange);
                overridenVariantsInRoom.Remove(variantChange);
                overridenVariantsInRoomRevertOnLeave.Remove(variantChange);

                // ... and save it straight into session.
                ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[variantChange] = actualNewValue;
            }

            return oldValue;
        }

        public void OnExitedRevertOnLeaveTrigger(ExtendedVariantsModule.Variant variantChange, object oldValueToRevertOnLeave) {
            setVariantValue(variantChange, oldValueToRevertOnLeave, out _);

            overridenVariantsInRoomRevertOnLeave[variantChange] = oldValueToRevertOnLeave;
            Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Left ExtendedVariantTrigger: reverted {variantChange} to {oldValueToRevertOnLeave}");
        }

        public object GetExpectedVariantValue(ExtendedVariantsModule.Variant variant) {
            if (overridenVariantsInRoomRevertOnLeave.ContainsKey(variant)) {
                // variant was replaced in current room in "revert on leave" mode: we expect this value to be set.
                return overridenVariantsInRoomRevertOnLeave[variant];
            }
            if (overridenVariantsInRoom.ContainsKey(variant)) {
                // variant was replaced in current room: we expect this value to be set.
                return overridenVariantsInRoom[variant];
            }
            if (ExtendedVariantsModule.Session.VariantsEnabledViaTrigger.ContainsKey(variant)) {
                // variant was replaced in a previous room: we expect this value to be set.
                return ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[variant];
            }
            // no variant trigger has been used: we expect the default value.
            return GetDefaultValueForVariant(variant);
        }

        public static object GetDefaultValueForVariant(ExtendedVariantsModule.Variant variant) {
            return ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue();
        }

        public object GetCurrentVariantValue(ExtendedVariantsModule.Variant variant) {
            return ExtendedVariantsModule.Instance.VariantHandlers[variant].GetVariantValue();
        }

        public void ResetAllVariantsToDefault(bool isVanilla) {
            // reset the variants themselves
            ExtendedVariantsModule.Instance.ResetVariantsToDefaultSettings(isVanilla);

            // reset the session
            filterOutVariants(ExtendedVariantsModule.Session.VariantsEnabledViaTrigger, isVanilla);

            // reset the variants set in the room
            filterOutVariants(oldVariantsInRoom, isVanilla);
            filterOutVariants(overridenVariantsInRoom, isVanilla);
            filterOutVariants(overridenVariantsInRoomRevertOnLeave, isVanilla);
            filterOutVariants(variantValuesBeforeOverride, isVanilla);
        }

        private void filterOutVariants(Dictionary<ExtendedVariantsModule.Variant, object> list, bool isVanilla) {
            foreach (ExtendedVariantsModule.Variant variant in list.Keys.ToList()) {
                if (ExtendedVariantsModule.Instance.VariantHandlers.TryGetValue(variant, out AbstractExtendedVariant variantHandler) && variantHandler.IsVanilla()) {
                    if (isVanilla) {
                        list.Remove(variant);
                    }
                } else {
                    if (!isVanilla) {
                        list.Remove(variant);
                    }
                }
            }
        }

        internal class LegacyVariantValue {
            public int Value { get; }

            public LegacyVariantValue(int value) {
                Value = value;
            }
        }

        /// <summary>
        /// Sets a variant value.
        /// </summary>
        /// <param name="variantChange">The variant to change</param>
        /// <param name="newValue">The new value</param>
        /// <returns>The old value for this variant</returns>
        private object setVariantValue(ExtendedVariantsModule.Variant variantChange, object newValue, out object actualNewValue) {
            if (newValue is LegacyVariantValue legacyVariantValue) {
                object oldVal = setLegacyVariantValue(variantChange, legacyVariantValue.Value, out actualNewValue);
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantTriggerManager", "setVariantValue was called with a LegacyVariantValue of " + legacyVariantValue.Value + " for " + variantChange
                    + "! It was turned into a " + actualNewValue.GetType() + " with value " + actualNewValue + ".");
                return oldVal;
            }

            AbstractExtendedVariant variant = ExtendedVariantsModule.Instance.VariantHandlers[variantChange];
            object oldValue = variant.GetVariantValue();
            variant.SetVariantValue(newValue);
            actualNewValue = variant.GetVariantValue();

            ExtendedVariantsModule.Instance.Randomizer.RefreshEnabledVariantsDisplayList();

            return oldValue;
        }


        /// <summary>
        /// Sets a "legacy" variant value ("everything is an integer", leading to some insane nonsensical rules).
        /// </summary>
        /// <param name="variantChange">The variant to change</param>
        /// <param name="newValue">The new value</param>
        /// <returns>The old value for this variant</returns>
        private object setLegacyVariantValue(ExtendedVariantsModule.Variant variantChange, int newValue, out object actualNewValue) {
            AbstractExtendedVariant variant = ExtendedVariantsModule.Instance.VariantHandlers[variantChange];
            object oldValue = variant.GetVariantValue();
            variant.SetLegacyVariantValue(newValue);
            actualNewValue = variant.GetVariantValue();

            ExtendedVariantsModule.Instance.Randomizer.RefreshEnabledVariantsDisplayList();

            return oldValue;
        }
    }
}
