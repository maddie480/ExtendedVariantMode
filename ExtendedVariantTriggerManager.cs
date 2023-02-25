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
                    setVariantValue(ExtendedVariantsModule.Session.VariantsEnabledViaTrigger, true, v, ExtendedVariantsModule.Session.OverriddenVariantsInRoom[v]);
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
                    oldValue = setVariantValue(ExtendedVariantsModule.Session.OverriddenVariantsInRoomRevertOnLeave, false, variantChange, newValue);
                } else {
                    oldValue = setVariantValue(ExtendedVariantsModule.Session.OverriddenVariantsInRoom, false, variantChange, newValue);
                }

                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager" + (isFade ? "-fade" : ""), $"Triggered ExtendedVariantTrigger: changed {variantChange} from {oldValue} to {newValue} (revertOnLeave = {revertOnLeave})");
            } else {
                // remove the variant from the room state if it was in there...
                ExtendedVariantsModule.Session.OverriddenVariantsInRoom.Remove(variantChange);
                ExtendedVariantsModule.Session.OverriddenVariantsInRoomRevertOnLeave.Remove(variantChange);

                // ... and save it straight into session.
                oldValue = setVariantValue(ExtendedVariantsModule.Session.VariantsEnabledViaTrigger, true, variantChange, newValue);

                Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Triggered ExtendedVariantTrigger: changed and committed {variantChange} from {oldValue} to {newValue}");
            }

            return oldValue;
        }

        public void OnExitedRevertOnLeaveTrigger(ExtendedVariantsModule.Variant variantChange, object oldValueToRevertOnLeave) {
            setVariantValue(ExtendedVariantsModule.Session.OverriddenVariantsInRoomRevertOnLeave, true, variantChange, oldValueToRevertOnLeave);
            Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", $"Left ExtendedVariantTrigger: reverted {variantChange} to {oldValueToRevertOnLeave}");
        }

        public static object GetDefaultValueForVariant(ExtendedVariantsModule.Variant variant) {
            return ExtendedVariantsModule.Instance.VariantHandlers[variant].GetDefaultVariantValue();
        }

        public object GetCurrentVariantValue(ExtendedVariantsModule.Variant variant) {
            if (ExtendedVariantsModule.Session != null && !ExtendedVariantsModule.Session.VariantsOverridenByUser.Contains(variant) && !(Engine.Scene is Overworld)) {
                if (ExtendedVariantsModule.Session.OverriddenVariantsInRoomRevertOnLeave.TryGetValue(variant, out object revertOnLeaveValue)) {
                    return revertOnLeaveValue;
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
                if (ExtendedVariantsModule.Session.OverriddenVariantsInRoomRevertOnLeave.TryGetValue(variant, out object revertOnLeaveValue)) {
                    return revertOnLeaveValue;
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
            resetVariantsInList(ExtendedVariantsModule.Session.OverriddenVariantsInRoomRevertOnLeave, isVanilla);
        }

        private void resetVariantsInList(Dictionary<ExtendedVariantsModule.Variant, object> list, bool isVanilla) {
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

        private object setVariantValue(Dictionary<ExtendedVariantsModule.Variant, object> target, bool allowRemove, ExtendedVariantsModule.Variant variantChange, object newValue) {
            object oldValue = GetCurrentMapDefinedVariantValue(variantChange);
            if (allowRemove && AreValuesIdentical(newValue, GetDefaultValueForVariant(variantChange))) {
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
            setVariantValue(target, true, variantChange, GetDefaultValueForVariant(variantChange));
        }

        private void roomStateReset() {
            Logger.Log("ExtendedVariantMode/ExtendedVariantTriggerManager", "Room state reset");
            foreach (ExtendedVariantsModule.Variant variant in ExtendedVariantsModule.Session.OverriddenVariantsInRoom.Keys.ToList()) {
                resetVariantValue(ExtendedVariantsModule.Session.OverriddenVariantsInRoom, variant);
            }
            foreach (ExtendedVariantsModule.Variant variant in ExtendedVariantsModule.Session.OverriddenVariantsInRoomRevertOnLeave.Keys.ToList()) {
                resetVariantValue(ExtendedVariantsModule.Session.OverriddenVariantsInRoomRevertOnLeave, variant);
            }

            if (ExtendedVariantsModule.Session.OverriddenVariantsInRoom.Count != 0 || ExtendedVariantsModule.Session.OverriddenVariantsInRoomRevertOnLeave.Count != 0) {
                Logger.Log(LogLevel.Error, "ExtendedVariantMode/ExtendedVariantTriggerManager", "Room state reset did not reset all variants!");
            }
        }
    }
}
