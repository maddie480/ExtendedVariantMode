using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;

namespace ExtendedVariants {
    public class ExtendedVariantTriggerManager {

        private Dictionary<ExtendedVariantsModule.Variant, int> overridenVariantsInRoom = new Dictionary<ExtendedVariantsModule.Variant, int>();
        private Dictionary<ExtendedVariantsModule.Variant, int> oldVariantsInRoom = new Dictionary<ExtendedVariantsModule.Variant, int>();
        private Dictionary<ExtendedVariantsModule.Variant, int> variantValuesBeforeOverride = new Dictionary<ExtendedVariantsModule.Variant, int>();

        public void Load() {
            Everest.Events.Level.OnLoadEntity += onLoadEntity;
            Everest.Events.Level.OnEnter += onLevelEnter;
            Everest.Events.Player.OnSpawn += onPlayerSpawn;
            Everest.Events.Level.OnTransitionTo += onLevelTransitionTo;
            Everest.Events.Level.OnExit += onLevelExit;
            IL.Celeste.ChangeRespawnTrigger.OnEnter += modRespawnTriggerOnEnter;
        }

        public void Unload() {
            Everest.Events.Level.OnLoadEntity -= onLoadEntity;
            Everest.Events.Level.OnEnter -= onLevelEnter;
            Everest.Events.Player.OnSpawn -= onPlayerSpawn;
            Everest.Events.Level.OnTransitionTo -= onLevelTransitionTo;
            Everest.Events.Level.OnExit -= onLevelExit;
            IL.Celeste.ChangeRespawnTrigger.OnEnter -= modRespawnTriggerOnEnter;
        }
        
        /// <summary>
        /// Handles ExtendedVariantTrigger constructing when loading a level.
        /// </summary>
        /// <param name="level">The level being loaded</param>
        /// <param name="levelData">unused</param>
        /// <param name="offset">offset passed to the trigger</param>
        /// <param name="entityData">the entity parameters</param>
        /// <returns>true if the trigger was loaded, false otherwise</returns>
        private bool onLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if(entityData.Name == "ExtendedVariantTrigger") {
                level.Add(new ExtendedVariantTrigger(entityData, offset));
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Restore extended variants values when entering a saved level.
        /// </summary>
        /// <param name="session">unused</param>
        /// <param name="fromSaveData">true if loaded from save data, false otherwise</param>
        private void onLevelEnter(Session session, bool fromSaveData) {
            // failsafe: if VariantsEnabledViaTrigger is null, initialize it. THIS SHOULD NEVER HAPPEN, but already happened in a case of a corrupted save.
            if ((ExtendedVariantsModule.Session?.VariantsEnabledViaTrigger ?? null) == null) {
                Logger.Log("ExtendedVariantsModule/OnLevelEnter", "WARNING: Session was null. This should not happen. Initializing it to an empty session.");
                ExtendedVariantsModule.Instance._Session = new ExtendedVariantsSession();
            }
            foreach (ExtendedVariantsModule.Variant v in ExtendedVariantsModule.Session.VariantsEnabledViaTrigger.Keys) {
                Logger.Log("ExtendedVariantsModule/OnLevelEnter", $"Loading save: restoring {v} to {ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[v]}");
                int oldValue = setVariantValue(v, ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[v]);
                variantValuesBeforeOverride[v] = oldValue;
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
                    Logger.Log("ExtendedVariantsModule/OnPlayerSpawn", $"Died in room: resetting {v} to {oldVariantsInRoom[v]}");
                    setVariantValue(v, oldVariantsInRoom[v]);
                }

                // clear values
                Logger.Log("ExtendedVariantsModule/OnPlayerSpawn", "Room state reset");
                oldVariantsInRoom.Clear();
                overridenVariantsInRoom.Clear();
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
                Logger.Log("ExtendedVariantsModule", $"Inserting call to CommitVariantChanges at index {cursor.Index} in CIL code for OnEnter in ChangeRespawnTrigger");
                cursor.EmitDelegate<Action>(commitVariantChanges);
            }
        }

        /// <summary>
        /// Make the changes in variant settings permanent (even if the player dies).
        /// </summary>
        private void commitVariantChanges() {
            if (overridenVariantsInRoom.Count != 0) {
                // "commit" variants set in the room to save slot
                foreach (ExtendedVariantsModule.Variant v in overridenVariantsInRoom.Keys) {
                    Logger.Log("ExtendedVariantsModule/CommitVariantChanges", $"Committing variant change {v} to {overridenVariantsInRoom[v]}");
                    ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[v] = overridenVariantsInRoom[v];
                }

                // clear values
                Logger.Log("ExtendedVariantsModule/CommitVariantChanges", "Room state reset");
                oldVariantsInRoom.Clear();
                overridenVariantsInRoom.Clear();
            }
        }
        
        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            if (variantValuesBeforeOverride.Count != 0) {
                // reset all variants that got set during the session
                foreach (ExtendedVariantsModule.Variant v in variantValuesBeforeOverride.Keys) {
                    Logger.Log("ExtendedVariantsModule/OnLevelExit", $"Ending session: resetting {v} to {variantValuesBeforeOverride[v]}");
                    setVariantValue(v, variantValuesBeforeOverride[v]);
                }
            }

            // exiting level: clear state
            Logger.Log("ExtendedVariantsModule/OnLevelExit", "Room and session state reset");
            overridenVariantsInRoom.Clear();
            oldVariantsInRoom.Clear();
            variantValuesBeforeOverride.Clear();
        }
        
        public int OnEnteredInTrigger(ExtendedVariantsModule.Variant variantChange, int newValue, bool revertOnLeave) {
            // change the variant value
            int oldValue = setVariantValue(variantChange, newValue);

            // store the fact that the variant was changed within the room
            // so that it can be reverted if we die, or saved if we save & quit later
            Logger.Log("ExtendedVariantTrigger", $"Triggered ExtendedVariantTrigger: changed {variantChange} from {oldValue} to {newValue} (revertOnLeave = {revertOnLeave})");

            if (!oldVariantsInRoom.ContainsKey(variantChange)) {
                oldVariantsInRoom[variantChange] = oldValue;
            }
            if (!variantValuesBeforeOverride.ContainsKey(variantChange)) {
                variantValuesBeforeOverride[variantChange] = oldValue;
            }
            if (!revertOnLeave) { // we don't want the value to be committed when leaving the room, since this is temporary
                overridenVariantsInRoom[variantChange] = newValue;
            }

            return oldValue;
        }

        public void OnExitedRevertOnLeaveTrigger(ExtendedVariantsModule.Variant variantChange, int oldValueToRevertOnLeave) {
            setVariantValue(variantChange, oldValueToRevertOnLeave);
            Logger.Log("ExtendedVariantTrigger", $"Left ExtendedVariantTrigger: reverted {variantChange} to {oldValueToRevertOnLeave}");
        }

        /// <summary>
        /// Sets a variant value.
        /// </summary>
        /// <param name="variantChange">The variant to change</param>
        /// <param name="newValue">The new value</param>
        /// <returns>The old value for this variant</returns>
        private int setVariantValue(ExtendedVariantsModule.Variant variantChange, int newValue) {
            int oldValue;

            switch(variantChange) {
                case ExtendedVariantsModule.Variant.ChaserCount:
                    oldValue = ExtendedVariantsModule.Settings.ChaserCount;
                    ExtendedVariantsModule.Settings.ChaserCount = newValue;
                    break;
                case ExtendedVariantsModule.Variant.AffectExistingChasers:
                    oldValue = ExtendedVariantsModule.Settings.AffectExistingChasers ? 1 : 0;
                    ExtendedVariantsModule.Settings.AffectExistingChasers = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.RefillJumpsOnDashRefill:
                    oldValue = ExtendedVariantsModule.Settings.RefillJumpsOnDashRefill ? 1 : 0;
                    ExtendedVariantsModule.Settings.RefillJumpsOnDashRefill = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.HiccupStrength:
                    oldValue = ExtendedVariantsModule.Settings.HiccupStrength;
                    ExtendedVariantsModule.Settings.HiccupStrength = newValue;
                    break;
                case ExtendedVariantsModule.Variant.SnowballDelay:
                    oldValue = ExtendedVariantsModule.Settings.SnowballDelay;
                    ExtendedVariantsModule.Settings.SnowballDelay = newValue;
                    break;
                case ExtendedVariantsModule.Variant.BadelineLag:
                    oldValue = ExtendedVariantsModule.Settings.BadelineLag;
                    ExtendedVariantsModule.Settings.BadelineLag = newValue;
                    break;
                default:
                    AbstractExtendedVariant variant = ExtendedVariantsModule.Instance.VariantHandlers[variantChange];
                    oldValue = variant.GetValue();
                    variant.SetValue(newValue);
                    break;
            }

            return oldValue;
        }
    }
}
