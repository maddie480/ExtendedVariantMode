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

        private Dictionary<ExtendedVariantsModule.Variant, object> overridenVariantsInRoom = new Dictionary<ExtendedVariantsModule.Variant, object>();
        private Dictionary<ExtendedVariantsModule.Variant, object> overridenVariantsInRoomRevertOnLeave = new Dictionary<ExtendedVariantsModule.Variant, object>();
        private Dictionary<ExtendedVariantsModule.Variant, object> oldVariantsInRoom = new Dictionary<ExtendedVariantsModule.Variant, object>();
        private Dictionary<ExtendedVariantsModule.Variant, object> variantValuesBeforeOverride = new Dictionary<ExtendedVariantsModule.Variant, object>();

        public void Load() {
            Everest.Events.Level.OnEnter += onLevelEnter;
            Everest.Events.Player.OnSpawn += onPlayerSpawn;
            Everest.Events.Level.OnTransitionTo += onLevelTransitionTo;
            Everest.Events.Level.OnExit += onLevelExit;
            IL.Celeste.ChangeRespawnTrigger.OnEnter += modRespawnTriggerOnEnter;
        }

        public void Unload() {
            Everest.Events.Level.OnEnter -= onLevelEnter;
            Everest.Events.Player.OnSpawn -= onPlayerSpawn;
            Everest.Events.Level.OnTransitionTo -= onLevelTransitionTo;
            Everest.Events.Level.OnExit -= onLevelExit;
            IL.Celeste.ChangeRespawnTrigger.OnEnter -= modRespawnTriggerOnEnter;
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
                foreach (ExtendedVariantsModule.Variant v in ExtendedVariantsModule.Session.VariantsEnabledViaTrigger.Keys) {
                    Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Loading save: restoring {v} to {ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[v]}");

                    object oldValue = setVariantValue(v, ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[v], out _);

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

            if (revertOnDeath) {
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
            switch (variant) {
                case ExtendedVariantsModule.Variant.ChaserCount: return 1;
                case ExtendedVariantsModule.Variant.AffectExistingChasers: return false;
                case ExtendedVariantsModule.Variant.HiccupStrength: return 1f;
                case ExtendedVariantsModule.Variant.RefillJumpsOnDashRefill: return false;
                case ExtendedVariantsModule.Variant.SnowballDelay: return 0.8f;
                case ExtendedVariantsModule.Variant.BadelineLag: return 1.55f;
                case ExtendedVariantsModule.Variant.DelayBetweenBadelines: return 0.4f;
                case ExtendedVariantsModule.Variant.OshiroCount: return 1;
                case ExtendedVariantsModule.Variant.ReverseOshiroCount: return 0;
                case ExtendedVariantsModule.Variant.DisableOshiroSlowdown: return false;
                case ExtendedVariantsModule.Variant.DisableSeekerSlowdown: return false;
                case ExtendedVariantsModule.Variant.BadelineAttackPattern: return 0;
                case ExtendedVariantsModule.Variant.ChangePatternsOfExistingBosses: return false;
                case ExtendedVariantsModule.Variant.FirstBadelineSpawnRandom: return false;
                case ExtendedVariantsModule.Variant.BadelineBossCount: return 1;
                case ExtendedVariantsModule.Variant.BadelineBossNodeCount: return 1;
                case ExtendedVariantsModule.Variant.RisingLavaSpeed: return 1f;
                case ExtendedVariantsModule.Variant.AllowThrowingTheoOffscreen: return false;
                case ExtendedVariantsModule.Variant.AllowLeavingTheoBehind: return false;
                case ExtendedVariantsModule.Variant.DisableSuperBoosts: return false;
                case ExtendedVariantsModule.Variant.DontRefillStaminaOnGround: return false;
                default: return ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue();
            }
        }

        public object GetCurrentVariantValue(ExtendedVariantsModule.Variant variant) {
            switch (variant) {
                case ExtendedVariantsModule.Variant.ChaserCount: return ExtendedVariantsModule.Settings.ChaserCount;
                case ExtendedVariantsModule.Variant.AffectExistingChasers: return ExtendedVariantsModule.Settings.AffectExistingChasers;
                case ExtendedVariantsModule.Variant.RefillJumpsOnDashRefill: return ExtendedVariantsModule.Settings.RefillJumpsOnDashRefill;
                case ExtendedVariantsModule.Variant.HiccupStrength: return ExtendedVariantsModule.Settings.HiccupStrength;
                case ExtendedVariantsModule.Variant.SnowballDelay: return ExtendedVariantsModule.Settings.SnowballDelay;
                case ExtendedVariantsModule.Variant.BadelineLag: return ExtendedVariantsModule.Settings.BadelineLag;
                case ExtendedVariantsModule.Variant.DelayBetweenBadelines: return ExtendedVariantsModule.Settings.DelayBetweenBadelines;
                case ExtendedVariantsModule.Variant.OshiroCount: return ExtendedVariantsModule.Settings.OshiroCount;
                case ExtendedVariantsModule.Variant.ReverseOshiroCount: return ExtendedVariantsModule.Settings.ReverseOshiroCount;
                case ExtendedVariantsModule.Variant.DisableOshiroSlowdown: return ExtendedVariantsModule.Settings.DisableOshiroSlowdown;
                case ExtendedVariantsModule.Variant.DisableSeekerSlowdown: return ExtendedVariantsModule.Settings.DisableSeekerSlowdown;
                case ExtendedVariantsModule.Variant.BadelineAttackPattern: return ExtendedVariantsModule.Settings.BadelineAttackPattern;
                case ExtendedVariantsModule.Variant.ChangePatternsOfExistingBosses: return ExtendedVariantsModule.Settings.ChangePatternsOfExistingBosses;
                case ExtendedVariantsModule.Variant.FirstBadelineSpawnRandom: return ExtendedVariantsModule.Settings.FirstBadelineSpawnRandom;
                case ExtendedVariantsModule.Variant.BadelineBossCount: return ExtendedVariantsModule.Settings.BadelineBossCount;
                case ExtendedVariantsModule.Variant.BadelineBossNodeCount: return ExtendedVariantsModule.Settings.BadelineBossNodeCount;
                case ExtendedVariantsModule.Variant.RisingLavaSpeed: return ExtendedVariantsModule.Settings.RisingLavaSpeed;
                case ExtendedVariantsModule.Variant.AllowThrowingTheoOffscreen: return ExtendedVariantsModule.Settings.AllowThrowingTheoOffscreen;
                case ExtendedVariantsModule.Variant.AllowLeavingTheoBehind: return ExtendedVariantsModule.Settings.AllowLeavingTheoBehind;
                case ExtendedVariantsModule.Variant.DisableSuperBoosts: return ExtendedVariantsModule.Settings.DisableSuperBoosts;
                case ExtendedVariantsModule.Variant.DontRefillStaminaOnGround: return ExtendedVariantsModule.Settings.DontRefillStaminaOnGround;
                default: return ExtendedVariantsModule.Instance.VariantHandlers[variant].GetVariantValue();
            }
        }

        /// <summary>
        /// Sets a variant value.
        /// </summary>
        /// <param name="variantChange">The variant to change</param>
        /// <param name="newValue">The new value</param>
        /// <returns>The old value for this variant</returns>
        private object setVariantValue(ExtendedVariantsModule.Variant variantChange, object newValue, out object actualNewValue) {
            object oldValue;

            switch (variantChange) {
                case ExtendedVariantsModule.Variant.ChaserCount:
                    oldValue = ExtendedVariantsModule.Settings.ChaserCount;
                    ExtendedVariantsModule.Settings.ChaserCount = (int) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.AffectExistingChasers:
                    oldValue = ExtendedVariantsModule.Settings.AffectExistingChasers;
                    ExtendedVariantsModule.Settings.AffectExistingChasers = (bool) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.RefillJumpsOnDashRefill:
                    oldValue = ExtendedVariantsModule.Settings.RefillJumpsOnDashRefill;
                    ExtendedVariantsModule.Settings.RefillJumpsOnDashRefill = (bool) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.HiccupStrength:
                    oldValue = ExtendedVariantsModule.Settings.HiccupStrength;
                    ExtendedVariantsModule.Settings.HiccupStrength = (float) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.SnowballDelay:
                    oldValue = ExtendedVariantsModule.Settings.SnowballDelay;
                    ExtendedVariantsModule.Settings.SnowballDelay = (float) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.BadelineLag:
                    oldValue = ExtendedVariantsModule.Settings.BadelineLag;
                    ExtendedVariantsModule.Settings.BadelineLag = (float) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.DelayBetweenBadelines:
                    oldValue = ExtendedVariantsModule.Settings.DelayBetweenBadelines;
                    ExtendedVariantsModule.Settings.DelayBetweenBadelines = (float) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.OshiroCount:
                    oldValue = ExtendedVariantsModule.Settings.OshiroCount;
                    ExtendedVariantsModule.Settings.OshiroCount = (int) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.ReverseOshiroCount:
                    oldValue = ExtendedVariantsModule.Settings.ReverseOshiroCount;
                    ExtendedVariantsModule.Settings.ReverseOshiroCount = (int) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.DisableOshiroSlowdown:
                    oldValue = ExtendedVariantsModule.Settings.DisableOshiroSlowdown;
                    ExtendedVariantsModule.Settings.DisableOshiroSlowdown = (bool) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.DisableSeekerSlowdown:
                    oldValue = ExtendedVariantsModule.Settings.DisableSeekerSlowdown;
                    ExtendedVariantsModule.Settings.DisableSeekerSlowdown = (bool) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.BadelineAttackPattern:
                    oldValue = ExtendedVariantsModule.Settings.BadelineAttackPattern;
                    ExtendedVariantsModule.Settings.BadelineAttackPattern = (int) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.ChangePatternsOfExistingBosses:
                    oldValue = ExtendedVariantsModule.Settings.ChangePatternsOfExistingBosses;
                    ExtendedVariantsModule.Settings.ChangePatternsOfExistingBosses = (bool) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.FirstBadelineSpawnRandom:
                    oldValue = ExtendedVariantsModule.Settings.FirstBadelineSpawnRandom;
                    ExtendedVariantsModule.Settings.FirstBadelineSpawnRandom = (bool) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.BadelineBossCount:
                    oldValue = ExtendedVariantsModule.Settings.BadelineBossCount;
                    ExtendedVariantsModule.Settings.BadelineBossCount = (int) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.BadelineBossNodeCount:
                    oldValue = ExtendedVariantsModule.Settings.BadelineBossNodeCount;
                    ExtendedVariantsModule.Settings.BadelineBossNodeCount = (int) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.RisingLavaSpeed:
                    oldValue = ExtendedVariantsModule.Settings.RisingLavaSpeed;
                    ExtendedVariantsModule.Settings.RisingLavaSpeed = (float) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.AllowThrowingTheoOffscreen:
                    oldValue = ExtendedVariantsModule.Settings.AllowThrowingTheoOffscreen;
                    ExtendedVariantsModule.Settings.AllowThrowingTheoOffscreen = (bool) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.AllowLeavingTheoBehind:
                    oldValue = ExtendedVariantsModule.Settings.AllowLeavingTheoBehind;
                    ExtendedVariantsModule.Settings.AllowLeavingTheoBehind = (bool) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.DisableSuperBoosts:
                    oldValue = ExtendedVariantsModule.Settings.DisableSuperBoosts;
                    ExtendedVariantsModule.Settings.DisableSuperBoosts = (bool) newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.DontRefillStaminaOnGround:
                    oldValue = ExtendedVariantsModule.Settings.DontRefillStaminaOnGround;
                    ExtendedVariantsModule.Settings.DontRefillStaminaOnGround = (bool) newValue;
                    actualNewValue = newValue;
                    break;
                default:
                    AbstractExtendedVariant variant = ExtendedVariantsModule.Instance.VariantHandlers[variantChange];
                    oldValue = variant.GetVariantValue();
                    variant.SetVariantValue(newValue);
                    actualNewValue = variant.GetVariantValue();
                    break;
            }

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
            object oldValue;

            switch (variantChange) {
                case ExtendedVariantsModule.Variant.ChaserCount:
                    oldValue = ExtendedVariantsModule.Settings.ChaserCount;
                    ExtendedVariantsModule.Settings.ChaserCount = newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.AffectExistingChasers:
                    oldValue = ExtendedVariantsModule.Settings.AffectExistingChasers;
                    ExtendedVariantsModule.Settings.AffectExistingChasers = (newValue != 0);
                    actualNewValue = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.RefillJumpsOnDashRefill:
                    oldValue = ExtendedVariantsModule.Settings.RefillJumpsOnDashRefill;
                    ExtendedVariantsModule.Settings.RefillJumpsOnDashRefill = (newValue != 0);
                    actualNewValue = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.HiccupStrength:
                    oldValue = ExtendedVariantsModule.Settings.HiccupStrength;
                    ExtendedVariantsModule.Settings.HiccupStrength = newValue / 10f;
                    actualNewValue = newValue / 10f;
                    break;
                case ExtendedVariantsModule.Variant.SnowballDelay:
                    oldValue = ExtendedVariantsModule.Settings.SnowballDelay;
                    ExtendedVariantsModule.Settings.SnowballDelay = newValue / 10f;
                    actualNewValue = newValue / 10f;
                    break;
                case ExtendedVariantsModule.Variant.BadelineLag:
                    oldValue = ExtendedVariantsModule.Settings.BadelineLag;
                    // The delay between the player and the first Badeline, multiplied by 10, default is 0 (actually 1.55s).
                    ExtendedVariantsModule.Settings.BadelineLag = (newValue == 0 ? 1.55f : newValue / 10f);
                    actualNewValue = (newValue == 0 ? 1.55f : newValue / 10f);
                    break;
                case ExtendedVariantsModule.Variant.DelayBetweenBadelines:
                    oldValue = ExtendedVariantsModule.Settings.DelayBetweenBadelines;
                    ExtendedVariantsModule.Settings.DelayBetweenBadelines = newValue / 10f;
                    actualNewValue = newValue / 10f;
                    break;
                case ExtendedVariantsModule.Variant.OshiroCount:
                    oldValue = ExtendedVariantsModule.Settings.OshiroCount;
                    ExtendedVariantsModule.Settings.OshiroCount = newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.ReverseOshiroCount:
                    oldValue = ExtendedVariantsModule.Settings.ReverseOshiroCount;
                    ExtendedVariantsModule.Settings.ReverseOshiroCount = newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.DisableOshiroSlowdown:
                    oldValue = ExtendedVariantsModule.Settings.DisableOshiroSlowdown;
                    ExtendedVariantsModule.Settings.DisableOshiroSlowdown = (newValue != 0);
                    actualNewValue = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.DisableSeekerSlowdown:
                    oldValue = ExtendedVariantsModule.Settings.DisableSeekerSlowdown;
                    ExtendedVariantsModule.Settings.DisableSeekerSlowdown = (newValue != 0);
                    actualNewValue = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.BadelineAttackPattern:
                    oldValue = ExtendedVariantsModule.Settings.BadelineAttackPattern;
                    ExtendedVariantsModule.Settings.BadelineAttackPattern = newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.ChangePatternsOfExistingBosses:
                    oldValue = ExtendedVariantsModule.Settings.ChangePatternsOfExistingBosses;
                    ExtendedVariantsModule.Settings.ChangePatternsOfExistingBosses = (newValue != 0);
                    actualNewValue = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.FirstBadelineSpawnRandom:
                    oldValue = ExtendedVariantsModule.Settings.FirstBadelineSpawnRandom;
                    ExtendedVariantsModule.Settings.FirstBadelineSpawnRandom = (newValue != 0);
                    actualNewValue = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.BadelineBossCount:
                    oldValue = ExtendedVariantsModule.Settings.BadelineBossCount;
                    ExtendedVariantsModule.Settings.BadelineBossCount = newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.BadelineBossNodeCount:
                    oldValue = ExtendedVariantsModule.Settings.BadelineBossNodeCount;
                    ExtendedVariantsModule.Settings.BadelineBossNodeCount = newValue;
                    actualNewValue = newValue;
                    break;
                case ExtendedVariantsModule.Variant.RisingLavaSpeed:
                    oldValue = ExtendedVariantsModule.Settings.RisingLavaSpeed;
                    ExtendedVariantsModule.Settings.RisingLavaSpeed = newValue / 10f;
                    actualNewValue = newValue / 10f;
                    break;
                case ExtendedVariantsModule.Variant.AllowThrowingTheoOffscreen:
                    oldValue = ExtendedVariantsModule.Settings.AllowThrowingTheoOffscreen;
                    ExtendedVariantsModule.Settings.AllowThrowingTheoOffscreen = (newValue != 0);
                    actualNewValue = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.AllowLeavingTheoBehind:
                    oldValue = ExtendedVariantsModule.Settings.AllowLeavingTheoBehind;
                    ExtendedVariantsModule.Settings.AllowLeavingTheoBehind = (newValue != 0);
                    actualNewValue = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.DisableSuperBoosts:
                    oldValue = ExtendedVariantsModule.Settings.DisableSuperBoosts;
                    ExtendedVariantsModule.Settings.DisableSuperBoosts = (newValue != 0);
                    actualNewValue = (newValue != 0);
                    break;
                case ExtendedVariantsModule.Variant.DontRefillStaminaOnGround:
                    oldValue = ExtendedVariantsModule.Settings.DontRefillStaminaOnGround;
                    ExtendedVariantsModule.Settings.DontRefillStaminaOnGround = (newValue != 0);
                    actualNewValue = (newValue != 0);
                    break;
                default:
                    AbstractExtendedVariant variant = ExtendedVariantsModule.Instance.VariantHandlers[variantChange];
                    oldValue = variant.GetVariantValue();
                    variant.SetLegacyVariantValue(newValue);
                    actualNewValue = variant.GetVariantValue();
                    break;
            }

            ExtendedVariantsModule.Instance.Randomizer.RefreshEnabledVariantsDisplayList();

            return oldValue;
        }
    }
}
