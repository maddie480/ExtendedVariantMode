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
using ExtendedVariants.Variants.Vanilla;

namespace ExtendedVariants {
    public class ExtendedVariantTriggerManager {
        public void Load() {
            On.Celeste.Player.ctor += onPlayerCreate;
            Everest.Events.Player.OnSpawn += onPlayerSpawn;
            Everest.Events.Level.OnExit += onLevelExit;
            Everest.Events.Level.OnTransitionTo += onLevelTransitionTo;
            IL.Celeste.ChangeRespawnTrigger.OnEnter += modRespawnTriggerOnEnter;
        }

        public void Unload() {
            On.Celeste.Player.ctor -= onPlayerCreate;
            Everest.Events.Player.OnSpawn -= onPlayerSpawn;
            Everest.Events.Level.OnExit -= onLevelExit;
            Everest.Events.Level.OnTransitionTo -= onLevelTransitionTo;
            IL.Celeste.ChangeRespawnTrigger.OnEnter -= modRespawnTriggerOnEnter;
        }

        private void onPlayerCreate(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);
            roomStateReset();
        }

        private void onPlayerSpawn(Player player) {
            // This should already be done by onPlayerCreate... except in the case of a teleport.
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
            if (ExtendedVariantsModule.Session.OverriddenVariantsInRoom.Count != 0) {
                // "commit" variants set in the room to save slot
                foreach (ExtendedVariantsModule.Variant v in ExtendedVariantsModule.Session.OverriddenVariantsInRoom.Keys) {
                    Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Committing variant change {v} to {ExtendedVariantsModule.Session.OverriddenVariantsInRoom[v]}");
                    setVariantValueInSession(v, ExtendedVariantsModule.Session.OverriddenVariantsInRoom[v]);
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

            if (revertOnDeath || revertOnLeave) {
                // store the fact that the variant was changed within the room
                // so that it can be reverted if we die, or saved if we save & quit later
                // fade triggers get a special tag, because it can very quickly flood logs (1 line per frame) and needs to be turned on only when necessary.
                if (revertOnLeave) {
                    setVariantValueAsRevertOnLeave(variantChange, newValue);
                } else {
                    setVariantValueInRoom(variantChange, newValue);
                }

                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager" + (isFade ? "-fade" : ""), $"Triggered ExtendedVariantTrigger: changed {variantChange} to {newValue} (revertOnLeave = {revertOnLeave})");
            } else {
                // remove the variant from the room state if it was in there...
                ExtendedVariantsModule.Session.OverriddenVariantsInRoom.Remove(variantChange);
                ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive.Remove(variantChange);

                // ... and save it straight into session.
                setVariantValueInSession(variantChange, newValue);

                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Triggered ExtendedVariantTrigger: changed {variantChange} to {newValue} and committed");
            }

            // the return value is useless, but I broke enough mods already so I'm not changing the signature.
            return null;
        }

        public void OnExitedRevertOnLeaveTrigger(ExtendedVariantsModule.Variant variantChange, object triggerValue) {
            if (ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive.TryGetValue(variantChange, out List<object> valuesRevertOnLeave)) {
                int index = valuesRevertOnLeave.FindIndex(v => AreValuesIdentical(v, triggerValue));
                if (index >= 0) {
                    valuesRevertOnLeave.RemoveAt(index);

                    if (valuesRevertOnLeave.Count == 0) {
                        ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive.Remove(variantChange);
                        Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Left ExtendedVariantTrigger: reverted {variantChange} from {triggerValue}. There are no values left!");
                    } else {
                        Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Left ExtendedVariantTrigger: reverted {variantChange} from {triggerValue}. There are now {valuesRevertOnLeave.Count} value(s).");
                    }

                    onVariantValueChanged(variantChange);
                }
            }
        }

        public static object GetDefaultValueForVariant(ExtendedVariantsModule.Variant variant) {
            return ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue();
        }

        public object GetCurrentVariantValue(ExtendedVariantsModule.Variant variant) {
            if (ExtendedVariantsModule.Session != null && !ExtendedVariantsModule.Session.VariantsOverridenByUser.Contains(variant) && !(Engine.Scene is Overworld)) {
                if (ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive.TryGetValue(variant, out List<object> revertOnLeaveValues)) {
                    return revertOnLeaveValues[revertOnLeaveValues.Count - 1];
                } else if (ExtendedVariantsModule.Session.OverriddenVariantsInRoom.TryGetValue(variant, out object roomValue)) {
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
                if (ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive.TryGetValue(variant, out List<object> revertOnLeaveValues)) {
                    return revertOnLeaveValues[revertOnLeaveValues.Count - 1];
                } else if (ExtendedVariantsModule.Session.OverriddenVariantsInRoom.TryGetValue(variant, out object roomValue)) {
                    return roomValue;
                } else if (ExtendedVariantsModule.Session.VariantsEnabledViaTrigger.TryGetValue(variant, out object sessionValue)) {
                    return sessionValue;
                }
            }

            return GetDefaultValueForVariant(variant);
        }

        public void ResetAllVariantsToDefault(bool isVanilla) {
            // reset the session
            resetVariantsInList(ExtendedVariantsModule.Session.VariantsEnabledViaTrigger, isVanilla);

            // reset the variants set in the room
            resetVariantsInList(ExtendedVariantsModule.Session.OverriddenVariantsInRoom, isVanilla);
            resetVariantsInList(ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive, isVanilla);
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

        private void roomStateReset() {
            Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", "Room state reset");
            foreach (ExtendedVariantsModule.Variant variant in ExtendedVariantsModule.Session.OverriddenVariantsInRoom.Keys.ToList()) {
                resetVariantValue(ExtendedVariantsModule.Session.OverriddenVariantsInRoom, variant);
            }
            foreach (ExtendedVariantsModule.Variant variant in ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive.Keys.ToList()) {
                resetVariantValue(ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive, variant);
            }

            if (ExtendedVariantsModule.Session.OverriddenVariantsInRoom.Count != 0 || ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive.Count != 0) {
                Logger.Log(LogLevel.Error, "ExtendedVariantMode/ExtendedVariantTriggerManager", "Room state reset did not reset all variants!");
            }
        }

        private void setVariantValueInSession(ExtendedVariantsModule.Variant variantChange, object newValue) {
            if (AreValuesIdentical(newValue, GetDefaultValueForVariant(variantChange))) {
                Logger.Log("ExtendedVariantsModule/ExtendedVariantTriggerManager", $"Variant value {variantChange} = {newValue} was equal to the default, so it was removed from the session.");
                ExtendedVariantsModule.Session.VariantsEnabledViaTrigger.Remove(variantChange);
            } else {
                Logger.Log("ExtendedVariantsModule/ExtendedVariantTriggerManager", $"Variant value {variantChange} = {newValue} was set in the session.");
                ExtendedVariantsModule.Session.VariantsEnabledViaTrigger[variantChange] = newValue;
            }

            onVariantValueChanged(variantChange);
        }

        private void setVariantValueInRoom(ExtendedVariantsModule.Variant variantChange, object newValue) {
            Logger.Log("ExtendedVariantsModule/ExtendedVariantTriggerManager", $"Variant value {variantChange} = {newValue} was set in the room overrides.");
            ExtendedVariantsModule.Session.OverriddenVariantsInRoom[variantChange] = newValue;

            onVariantValueChanged(variantChange);
        }

        private void setVariantValueAsRevertOnLeave(ExtendedVariantsModule.Variant variantChange, object newValue) {
            if (!ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive.ContainsKey(variantChange)) {
                ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive[variantChange] = new List<object>();
            }
            List<object> valuesRevertOnLeave = ExtendedVariantsModule.Session.RevertOnLeaveVariantsActive[variantChange];
            valuesRevertOnLeave.Add(newValue);

            Logger.Log("ExtendedVariantsModule/ExtendedVariantTriggerManager", $"Variant value {variantChange} = {newValue} was set as revert on leave. There are now {valuesRevertOnLeave.Count} value(s).");

            onVariantValueChanged(variantChange);
        }

        private void resetVariantsInList<T>(Dictionary<ExtendedVariantsModule.Variant, T> list, bool isVanilla) {
            foreach (ExtendedVariantsModule.Variant variant in list.Keys.ToList()) {
                if (ExtendedVariantsModule.Instance.VariantHandlers.TryGetValue(variant, out AbstractExtendedVariant variantHandler) && variantHandler is AbstractVanillaVariant) {
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

        private void resetVariantValue<T>(Dictionary<ExtendedVariantsModule.Variant, T> target, ExtendedVariantsModule.Variant variantChange) {
            target.Remove(variantChange);
            onVariantValueChanged(variantChange);
        }

        private void onVariantValueChanged(ExtendedVariantsModule.Variant variantChange) {
            ExtendedVariantsModule.Instance.VariantHandlers[variantChange].VariantValueChanged();
            ExtendedVariantsModule.Instance.Randomizer.RefreshEnabledVariantsDisplayList();
        }
    }
}
