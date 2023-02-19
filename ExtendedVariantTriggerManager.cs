using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Monocle;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedVariants {
    public class ExtendedVariantTriggerManager {

        private Dictionary<ExtendedVariantsModule.Variant, object> overridenVariantsInRoom = new Dictionary<ExtendedVariantsModule.Variant, object>();
        private Dictionary<ExtendedVariantsModule.Variant, object> overridenVariantsInRoomRevertOnLeave = new Dictionary<ExtendedVariantsModule.Variant, object>();

        public void Load() {
            On.Celeste.Player.ctor += onPlayerSpawn;
            Everest.Events.Level.OnExit += onLevelExit;
            Everest.Events.Level.OnTransitionTo += onLevelTransitionTo;
            IL.Celeste.ChangeRespawnTrigger.OnEnter += modRespawnTriggerOnEnter;
        }

        public void Unload() {
            On.Celeste.Player.ctor -= onPlayerSpawn;
            Everest.Events.Level.OnExit -= onLevelExit;
            Everest.Events.Level.OnTransitionTo -= onLevelTransitionTo;
            IL.Celeste.ChangeRespawnTrigger.OnEnter -= modRespawnTriggerOnEnter;
        }

        private void onPlayerSpawn(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);
            roomStateReset();
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            roomStateReset();
        }

        private void onLevelTransitionTo(Level level, LevelData next, Vector2 direction) {
            commitVariantChanges();
        }

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

        private void commitVariantChanges() {
            if (overridenVariantsInRoom.Count != 0) {
                // "commit" variants set in the room to save slot
                foreach (ExtendedVariantsModule.Variant v in overridenVariantsInRoom.Keys) {
                    Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Committing variant change {v} to {overridenVariantsInRoom[v]}");
                    setVariantValue(ExtendedVariantsModule.Session.VariantsEnabledViaTrigger, v, overridenVariantsInRoom[v]);
                }

                // clear values
                roomStateReset();
            }
        }

        public object OnEnteredInTrigger(ExtendedVariantsModule.Variant variantChange, object newValue, bool revertOnLeave, bool isFade, bool revertOnDeath, bool legacy) {
            // change the variant value
            if (legacy) {
                Logger.Log(LogLevel.Debug, "ExtendedVariantMode/ExtendedVariantTriggerManager", $"Encountered a trigger for changing {variantChange} to legacy value {newValue}");
                newValue = ExtendedVariantsModule.Instance.VariantHandlers[variantChange].ConvertLegacyVariantValue((int) newValue);
            }

            object oldValue;

            if (revertOnDeath || revertOnLeave) {
                // store the fact that the variant was changed within the room
                // so that it can be reverted if we die, or saved if we save & quit later
                // fade triggers get a special tag, because it can very quickly flood logs (1 line per frame) and needs to be turned on only when necessary.
                if (revertOnLeave) {
                    oldValue = setVariantValue(overridenVariantsInRoomRevertOnLeave, variantChange, newValue);
                } else {
                    oldValue = setVariantValue(overridenVariantsInRoom, variantChange, newValue);
                }

                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager" + (isFade ? "-fade" : ""), $"Triggered ExtendedVariantTrigger: changed {variantChange} from {oldValue} to {newValue} (revertOnLeave = {revertOnLeave})");
            } else {
                // remove the variant from the room state if it was in there...
                overridenVariantsInRoom.Remove(variantChange);
                overridenVariantsInRoomRevertOnLeave.Remove(variantChange);

                // ... and save it straight into session.
                oldValue = setVariantValue(ExtendedVariantsModule.Session.VariantsEnabledViaTrigger, variantChange, newValue);

                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Triggered ExtendedVariantTrigger: changed and committed {variantChange} from {oldValue} to {newValue}");
            }

            return oldValue;
        }

        public void OnExitedRevertOnLeaveTrigger(ExtendedVariantsModule.Variant variantChange, object oldValueToRevertOnLeave) {
            setVariantValue(overridenVariantsInRoomRevertOnLeave, variantChange, oldValueToRevertOnLeave);
            Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Left ExtendedVariantTrigger: reverted {variantChange} to {oldValueToRevertOnLeave}");
        }

        public static object GetDefaultValueForVariant(ExtendedVariantsModule.Variant variant) {
            return ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue();
        }

        public object GetCurrentVariantValue(ExtendedVariantsModule.Variant variant) {
            if (ExtendedVariantsModule.Session != null && !ExtendedVariantsModule.Session.VariantsOverridenByUser.Contains(variant) && !(Engine.Scene is Overworld)) {
                if (overridenVariantsInRoomRevertOnLeave.TryGetValue(variant, out object revertOnLeaveValue)) {
                    return revertOnLeaveValue;
                } else if (overridenVariantsInRoom.TryGetValue(variant, out object roomValue)) {
                    return roomValue;
                } else if (ExtendedVariantsModule.Session.VariantsEnabledViaTrigger.TryGetValue(variant, out object sessionValue)) {
                    return sessionValue;
                }
            }

            if (ExtendedVariantsModule.Settings.EnabledVariants.TryGetValue(variant, out object settingsValue)) {
                return settingsValue;
            }

            return GetDefaultValueForVariant(variant);
        }

        public object GetCurrentMapDefinedVariantValue(ExtendedVariantsModule.Variant variant) {
            if (ExtendedVariantsModule.Session != null && !(Engine.Scene is Overworld)) {
                if (overridenVariantsInRoomRevertOnLeave.TryGetValue(variant, out object revertOnLeaveValue)) {
                    return revertOnLeaveValue;
                } else if (overridenVariantsInRoom.TryGetValue(variant, out object roomValue)) {
                    return roomValue;
                } else if (ExtendedVariantsModule.Session.VariantsEnabledViaTrigger.TryGetValue(variant, out object sessionValue)) {
                    return sessionValue;
                }
            }

            return GetDefaultValueForVariant(variant);
        }

        public void ResetAllVariantsToDefault(bool isVanilla) {
            // reset the variants themselves
            ExtendedVariantsModule.Instance.ResetVariantsToDefaultSettings(isVanilla);

            // reset the session
            resetVariantsInList(ExtendedVariantsModule.Session.VariantsEnabledViaTrigger, isVanilla);

            // reset the variants set in the room
            resetVariantsInList(overridenVariantsInRoom, isVanilla);
            resetVariantsInList(overridenVariantsInRoomRevertOnLeave, isVanilla);
        }

        private void resetVariantsInList(Dictionary<ExtendedVariantsModule.Variant, object> list, bool isVanilla) {
            foreach (ExtendedVariantsModule.Variant variant in list.Keys.ToList()) {
                if (ExtendedVariantsModule.Instance.VariantHandlers.TryGetValue(variant, out AbstractExtendedVariant variantHandler) && variantHandler.IsVanilla()) {
                    if (isVanilla) {
                        resetVariantValue(list, variant);
                    }
                } else {
                    if (!isVanilla) {
                        resetVariantValue(list, variant);
                    }
                }
            }
        }

        public static bool AreValuesIdentical(object one, object two) {
            if (one is bool[][] oneBoolArray && two is bool[][] twoBoolArray) {
                for (int i = 0; i < 3; i++) {
                    for (int j = 0; j < 3; j++) {
                        if (i == 1 && j == 1) continue;
                        if (oneBoolArray[i][j] != twoBoolArray[i][j]) return false;
                    }
                }

                return true;
            }

            return one.Equals(two);
        }

        private object setVariantValue(Dictionary<ExtendedVariantsModule.Variant, object> target, ExtendedVariantsModule.Variant variantChange, object newValue) {
            object oldValue = GetCurrentMapDefinedVariantValue(variantChange);
            if (AreValuesIdentical(newValue, GetDefaultValueForVariant(variantChange))) {
                Logger.Log("ExtendedVariantsModule/ExtendedVariantTriggerManager", $"Variant value {variantChange} = {newValue} was equal to the default, so it was removed.");
                target.Remove(variantChange);
            } else {
                Logger.Log("ExtendedVariantsModule/ExtendedVariantTriggerManager", $"Variant value {variantChange} = {newValue} was set.");
                target[variantChange] = newValue;
            }

            ExtendedVariantsModule.Instance.VariantHandlers[variantChange].VariantValueChanged();
            ExtendedVariantsModule.Instance.Randomizer.RefreshEnabledVariantsDisplayList();

            return oldValue;
        }

        private void resetVariantValue(Dictionary<ExtendedVariantsModule.Variant, object> target, ExtendedVariantsModule.Variant variantChange) {
            setVariantValue(target, variantChange, GetDefaultValueForVariant(variantChange));
        }

        private void roomStateReset() {
            Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", "Room state reset");
            foreach (ExtendedVariantsModule.Variant variant in overridenVariantsInRoom.Keys.ToList()) {
                resetVariantValue(overridenVariantsInRoom, variant);
            }
            foreach (ExtendedVariantsModule.Variant variant in overridenVariantsInRoomRevertOnLeave.Keys.ToList()) {
                resetVariantValue(overridenVariantsInRoomRevertOnLeave, variant);
            }

            if (overridenVariantsInRoom.Count != 0 || overridenVariantsInRoomRevertOnLeave.Count != 0) {
                Logger.Log(LogLevel.Error, "ExtendedVariantMode/ExtendedVariantTriggerManager", "Room state reset did not reset all variants!");
            }
        }
    }
}
