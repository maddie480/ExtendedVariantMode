using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using Monocle;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

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
        public static TextMenu.Option<int> FallSpeedOption;
        public static TextMenu.Option<int> JumpHeightOption;
        public static TextMenu.Option<int> SpeedXOption;
        public static TextMenu.Option<int> StaminaOption;
        public static TextMenu.Option<int> DashSpeedOption;
        public static TextMenu.Option<int> DashCountOption;
        public static TextMenu.Option<int> FrictionOption;
        public static TextMenu.Option<bool> DisableWallJumpingOption;
        public static TextMenu.Option<int> JumpCountOption;
        public static TextMenu.Option<bool> UpsideDownOption;
        public static TextMenu.Option<int> HyperdashSpeedOption;
        public static TextMenu.Option<int> WallBouncingSpeedOption;
        public static TextMenu.Option<int> DashLengthOption;
        public static TextMenu.Option<bool> ForceDuckOnGroundOption;
        public static TextMenu.Option<bool> InvertDashesOption;
        public static TextMenu.Option<bool> DisableNeutralJumpingOption;
        public static TextMenu.Option<int> ChangeVariantsRandomlyOption;
        public static TextMenu.Option<int> ChangeVariantsIntervalOption;
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
            for (int index = 0; index < multiplierScale.Length - 1; index++) {
                if (multiplierScale[index + 1] > option) {
                    return index;
                }
            }

            return multiplierScale.Length - 1;
        }

        /// <summary>
        /// List of options shown for Change Variants Interval.
        /// </summary>
        private static int[] changeVariantsIntervalScale = new int[] {
            1, 2, 5, 10, 15, 30, 60
        };

        /// <summary>
        /// Finds out the index of an interval in the changeVariantsIntervalScale table.
        /// If it is not present, will return the previous option.
        /// (For example, 26s will return the index for 15s.)
        /// </summary>
        /// <param name="option">The interval</param>
        /// <returns>The index of the interval in the changeVariantsIntervalScale table</returns>
        private static int indexFromChangeVariantsInterval(int option) {
            for (int index = 0; index < changeVariantsIntervalScale.Length - 1; index++) {
                if (changeVariantsIntervalScale[index + 1] > option) {
                    return index;
                }
            }

            return changeVariantsIntervalScale.Length - 1;
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            base.CreateModMenuSection(menu, inGame, snapshot);

            // create every option
            GravityOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_GRAVITY"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.Gravity)).Change(i => Settings.Gravity = multiplierScale[i]);
            FallSpeedOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FALLSPEED"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.FallSpeed)).Change(i => Settings.FallSpeed = multiplierScale[i]);
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
            DisableWallJumpingOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLEWALLJUMPING"), Settings.DisableWallJumping)
                .Change(b => Settings.DisableWallJumping = b);
            JumpCountOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_JUMPCOUNT"),
                i => {
                    if (i == 6) {
                        return Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INFINITE");
                    }
                    return i.ToString();
                }, 0, 6, Settings.JumpCount).Change(i => Settings.JumpCount = i);
            UpsideDownOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_UPSIDEDOWN"), Settings.UpsideDown)
                .Change(b => Settings.UpsideDown = b);
            HyperdashSpeedOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HYPERDASHSPEED"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.HyperdashSpeed)).Change(i => Settings.HyperdashSpeed = multiplierScale[i]);
            WallBouncingSpeedOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_WALLBOUNCINGSPEED"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.WallBouncingSpeed)).Change(i => Settings.WallBouncingSpeed = multiplierScale[i]);
            DashLengthOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DASHLENGTH"),
                multiplierFormatter, 0, multiplierScale.Length - 1, indexFromMultiplier(Settings.DashLength)).Change(i => Settings.DashLength = multiplierScale[i]);
            ForceDuckOnGroundOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_FORCEDUCKONGROUND"), Settings.ForceDuckOnGround)
                .Change(b => Settings.ForceDuckOnGround = b);
            InvertDashesOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_INVERTDASHES"), Settings.InvertDashes)
                .Change(b => Settings.InvertDashes = b);
            DisableNeutralJumpingOption = new TextMenu.OnOff(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_DISABLENEUTRALJUMPING"), Settings.DisableNeutralJumping)
                .Change(b => Settings.DisableNeutralJumping = b);
            ChangeVariantsRandomlyOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHANGEVARIANTSRANDOMLY"),
                i => Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_" + new string[] { "OFF", "VANILLA", "EXTENDED", "BOTH" }[i]), 0, 3, Settings.ChangeVariantsRandomly)
                .Change(i => {
                    Settings.ChangeVariantsRandomly = i;
                    refreshOptionMenuEnabledStatus();
                });
            ChangeVariantsIntervalOption = new TextMenu.Slider(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_CHANGEVARIANTSINTERVAL"),
                i => $"{changeVariantsIntervalScale[i]}s", 0, changeVariantsIntervalScale.Length - 1, indexFromChangeVariantsInterval(Settings.ChangeVariantsInterval))
                .Change(i => {
                    Settings.ChangeVariantsInterval = changeVariantsIntervalScale[i];
                    changeVariantTimer = Settings.ChangeVariantsInterval;
                });

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
                refreshOptionMenuEnabledStatus();
            });

            refreshOptionMenuEnabledStatus();

            menu.Add(MasterSwitchOption);
            menu.Add(ResetToDefaultOption);

            addHeading(menu, "VERTICALSPEED");
            menu.Add(GravityOption);
            menu.Add(FallSpeedOption);

            addHeading(menu, "JUMPING");
            menu.Add(JumpHeightOption);
            menu.Add(WallBouncingSpeedOption);
            menu.Add(DisableWallJumpingOption);
            menu.Add(JumpCountOption);

            addHeading(menu, "DASHING");
            menu.Add(DashSpeedOption);
            menu.Add(DashLengthOption);
            menu.Add(HyperdashSpeedOption);
            menu.Add(DashCountOption);

            addHeading(menu, "MOVING");
            menu.Add(SpeedXOption);
            menu.Add(FrictionOption);

            addHeading(menu, "OTHER");
            menu.Add(StaminaOption);
            menu.Add(UpsideDownOption);
            menu.Add(DisableNeutralJumpingOption);

            addHeading(menu, "TROLL");
            menu.Add(ForceDuckOnGroundOption);
            menu.Add(InvertDashesOption);
            menu.Add(ChangeVariantsRandomlyOption);
            menu.Add(ChangeVariantsIntervalOption);
        }

        private static void addHeading(TextMenu menu, String headingNameResource) {
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HEADING") + " - " + Dialog.Clean("MODOPTIONS_EXTENDEDVARIANTS_HEADING_" + headingNameResource)));
        }

        private static void resetToDefaultSettings() {
            Settings.Gravity = 10;
            Settings.FallSpeed = 10;
            Settings.JumpHeight = 10;
            Settings.SpeedX = 10;
            Settings.Stamina = 11;
            Settings.DashSpeed = 10;
            Settings.DashCount = -1;
            Settings.Friction = 10;
            Settings.DisableWallJumping = false;
            Settings.JumpCount = 1;
            Settings.UpsideDown = false;
            Settings.HyperdashSpeed = 10;
            Settings.WallBouncingSpeed = 10;
            Settings.DashLength = 10;
            Settings.ForceDuckOnGround = false;
            Settings.InvertDashes = false;
            Settings.DisableNeutralJumping = false;
            Settings.ChangeVariantsRandomly = 0;
            Settings.ChangeVariantsInterval = 1;
        }

        private static void refreshOptionMenuValues() {
            setValue(GravityOption, 0, indexFromMultiplier(Settings.Gravity));
            setValue(FallSpeedOption, 0, indexFromMultiplier(Settings.FallSpeed));
            setValue(JumpHeightOption, 0, indexFromMultiplier(Settings.JumpHeight));
            setValue(SpeedXOption, 0, indexFromMultiplier(Settings.SpeedX));
            setValue(StaminaOption, 0, Settings.Stamina);
            setValue(DashSpeedOption, 0, indexFromMultiplier(Settings.DashSpeed));
            setValue(DashCountOption, -1, Settings.DashCount);
            setValue(FrictionOption, -1, Settings.Friction == -1 ? -1 : indexFromMultiplier(Settings.Friction));
            setValue(DisableWallJumpingOption, Settings.DisableWallJumping);
            setValue(JumpCountOption, 0, Settings.JumpCount);
            setValue(UpsideDownOption, Settings.UpsideDown);
            setValue(HyperdashSpeedOption, 0, indexFromMultiplier(Settings.HyperdashSpeed));
            setValue(WallBouncingSpeedOption, 0, indexFromMultiplier(Settings.WallBouncingSpeed));
            setValue(DashLengthOption, 0, indexFromMultiplier(Settings.DashLength));
            setValue(ForceDuckOnGroundOption, Settings.ForceDuckOnGround);
            setValue(InvertDashesOption, Settings.InvertDashes);
            setValue(DisableNeutralJumpingOption, Settings.DisableNeutralJumping);
            setValue(ChangeVariantsRandomlyOption, 0, Settings.ChangeVariantsRandomly);
            setValue(ChangeVariantsIntervalOption, 0, indexFromChangeVariantsInterval(Settings.ChangeVariantsInterval));
        }

        private static void refreshOptionMenuEnabledStatus() {
            GravityOption.Disabled = !Settings.MasterSwitch;
            FallSpeedOption.Disabled = !Settings.MasterSwitch;
            JumpHeightOption.Disabled = !Settings.MasterSwitch;
            SpeedXOption.Disabled = !Settings.MasterSwitch;
            StaminaOption.Disabled = !Settings.MasterSwitch;
            DashCountOption.Disabled = !Settings.MasterSwitch;
            DashSpeedOption.Disabled = !Settings.MasterSwitch;
            FrictionOption.Disabled = !Settings.MasterSwitch;
            DisableWallJumpingOption.Disabled = !Settings.MasterSwitch;
            JumpCountOption.Disabled = !Settings.MasterSwitch;
            ResetToDefaultOption.Disabled = !Settings.MasterSwitch;
            UpsideDownOption.Disabled = !Settings.MasterSwitch;
            HyperdashSpeedOption.Disabled = !Settings.MasterSwitch;
            WallBouncingSpeedOption.Disabled = !Settings.MasterSwitch;
            DashLengthOption.Disabled = !Settings.MasterSwitch;
            ForceDuckOnGroundOption.Disabled = !Settings.MasterSwitch;
            InvertDashesOption.Disabled = !Settings.MasterSwitch;
            DisableNeutralJumpingOption.Disabled = !Settings.MasterSwitch;
            ChangeVariantsRandomlyOption.Disabled = !Settings.MasterSwitch;
            ChangeVariantsIntervalOption.Disabled = !Settings.MasterSwitch || Settings.ChangeVariantsRandomly == 0;
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

        private static void setValue(TextMenu.Option<bool> option, bool newValue) {
            if (newValue != (option.Index == 1)) {
                // replicate the vanilla behaviour
                option.PreviousIndex = option.Index;
                option.Index = newValue ? 1 : 0;
                option.ValueWiggler.Start();
            }
        }

        // ================ Module loading ================

        public override void Load() {
            changeVariantTimer = Settings.ChangeVariantsInterval;

            // mod methods here
            IL.Celeste.Player.NormalUpdate += ModNormalUpdate;
            IL.Celeste.Player.ClimbUpdate += ModClimbUpdate;
            On.Celeste.Player.RefillStamina += ModRefillStamina;
            IL.Celeste.Player.SwimBegin += ModSwimBegin;
            IL.Celeste.Player.DreamDashBegin += ModDreamDashBegin;
            On.Celeste.Player.Update += ModUpdate;
            IL.Celeste.Player.ctor += ModPlayerConstructor;
            On.Celeste.SummitGem.SmashRoutine += ModSummitGemSmash;
            IL.Celeste.Player.UpdateSprite += ModUpdateSprite;
            On.Celeste.Player.RefillDash += ModRefillDash;
            IL.Celeste.Player.UseRefill += ModUseRefill;
            On.Celeste.Player.Added += ModAdded;
            IL.Celeste.Player.CallDashEvents += ModCallDashEvents;
            IL.Celeste.Player.UpdateHair += ModUpdateHair;
            IL.Celeste.Player.Jump += ModJump;
            IL.Celeste.Player.SuperJump += ModSuperJump;
            IL.Celeste.Player.SuperWallJump += ModSuperWallJump;
            IL.Celeste.Player.WallJump += ModWallJump;
            On.Celeste.Player.WallJump += ModOnWallJump;
            On.Celeste.AreaComplete.VersionNumberAndVariants += ModVersionNumberAndVariants;
            Everest.Events.Level.OnLoadEntity += new Everest.Events.Level.LoadEntityHandler(OnLoadEntity);
            Everest.Events.Player.OnSpawn += OnPlayerSpawn;
            Everest.Events.Level.OnTransitionTo += OnLevelTransitionTo;
            Everest.Events.Level.OnEnter += OnLevelEnter;
            Everest.Events.Level.OnExit += OnLevelExit;
            On.Celeste.SaveData.TryDelete += OnSaveDataDelete;
            IL.Celeste.ChangeRespawnTrigger.OnEnter += ModRespawnTrigger;
            IL.Celeste.Level.Render += ModLevelRender;
            IL.Celeste.Player.DashBegin += ModDashBegin;
            On.Celeste.Player.DashCoroutine += ModDashCoroutine;
            IL.Celeste.Player.DashUpdate += ModDashUpdate;

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
            On.Celeste.SummitGem.SmashRoutine -= ModSummitGemSmash;
            IL.Celeste.Player.UpdateSprite -= ModUpdateSprite;
            On.Celeste.Player.RefillDash -= ModRefillDash;
            IL.Celeste.Player.UseRefill -= ModUseRefill;
            On.Celeste.Player.Added -= ModAdded;
            IL.Celeste.Player.CallDashEvents -= ModCallDashEvents;
            IL.Celeste.Player.UpdateHair -= ModUpdateHair;
            IL.Celeste.Player.Jump -= ModJump;
            IL.Celeste.Player.SuperJump -= ModSuperJump;
            IL.Celeste.Player.SuperWallJump -= ModSuperWallJump;
            IL.Celeste.Player.WallJump -= ModWallJump;
            On.Celeste.Player.WallJump -= ModOnWallJump;
            On.Celeste.AreaComplete.VersionNumberAndVariants -= ModVersionNumberAndVariants;
            Everest.Events.Level.OnLoadEntity -= new Everest.Events.Level.LoadEntityHandler(OnLoadEntity);
            Everest.Events.Player.OnSpawn -= OnPlayerSpawn;
            Everest.Events.Level.OnTransitionTo -= OnLevelTransitionTo;
            Everest.Events.Level.OnEnter -= OnLevelEnter;
            Everest.Events.Level.OnExit -= OnLevelExit;
            On.Celeste.SaveData.TryDelete -= OnSaveDataDelete;
            IL.Celeste.ChangeRespawnTrigger.OnEnter -= ModRespawnTrigger;
            IL.Celeste.Level.Render -= ModLevelRender;
            IL.Celeste.Player.DashBegin -= ModDashBegin;
            On.Celeste.Player.DashCoroutine -= ModDashCoroutine;
            IL.Celeste.Player.DashUpdate -= ModDashUpdate;

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
                // (yet again, this is brtrue.s in XNA and brfalse.s in FNA. Thanks compiler.)
                if (cursor.TryGotoNext(MoveType.After, instr => (instr.OpCode == OpCodes.Brtrue_S || instr.OpCode == OpCodes.Brfalse_S))) {
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

                // find out where the constant 900 (downward acceleration) is loaded into the stack
                while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 900f)) {
                    Logger.Log("ExtendedVariantsModule", $"Applying gravity to constant at {cursor.Index} in CIL code for NormalUpdate");

                    // add two instructions to multiply those constants with the "gravity factor"
                    cursor.EmitDelegate<Func<float>>(DetermineGravityFactor);
                    cursor.Emit(OpCodes.Mul);
                }

                // chain every other NormalUpdate usage
                ModNormalUpdateFallSpeed(il);
                ModNormalUpdateSpeedX(il);
                ModNormalUpdateFriction(il);
                ModNormalUpdateJumpCount(il);
                ModNormalUpdateForceDuckOnGround(il);
            });
        }

        /// <summary>
        /// Returns the currently configured gravity factor.
        /// </summary>
        /// <returns>The gravity factor (1 = default gravity)</returns>
        public static float DetermineGravityFactor() {
            return Settings.GravityFactor;
        }

        // ================ Fall speed handling ================

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModNormalUpdateFallSpeed(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we will edit 2 constants here:
            // * 160 = max falling speed
            // * 240 = max falling speed when holding Down

            // find out where those constants are loaded into the stack
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && ((float)instr.Operand == 160f || (float)instr.Operand == 240f))) {
                Logger.Log("ExtendedVariantsModule", $"Applying max fall speed factor to constant at {cursor.Index} in CIL code for NormalUpdate");

                // add two instructions to multiply those constants with the "fall speed factor"
                cursor.EmitDelegate<Func<float>>(DetermineFallSpeedFactor);
                cursor.Emit(OpCodes.Mul);
            }

            cursor.Index = 0;

            // go back to the first 240f, then to the next "if" implying MoveY
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 240f)
                && cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldsfld && ((FieldReference)instr.Operand).Name.Contains("MoveY"))
                && cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Bne_Un || instr.OpCode == OpCodes.Brfalse)) {
                Logger.Log("ExtendedVariantsModule", $"Injecting code to fix animation with 0 fall speed at {cursor.Index} in CIL code for NormalUpdate");

                // save the target of this branch
                object label = cursor.Prev.Operand;

                // the goal here is to add another condition to the if: FallSpeedFactor should not be zero
                // so that the game does not try computing the animation (doing a nice division by 0 by the way)
                cursor.EmitDelegate<Func<float>>(DetermineFallSpeedFactor);
                cursor.Emit(OpCodes.Ldc_R4, 0f);
                cursor.Emit(OpCodes.Beq, label); // we jump (= skip the "if") if DetermineFallSpeedFactor is equal to 0.
            }
        }
        
        /// <summary>
        /// Edits the UpdateSprite method in Player (updating the player animation.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModUpdateSprite(ILContext il) {
            ModMethod("UpdateSprite", () => {
                ILCursor cursor = new ILCursor(il);

                // the goal is to multiply 160 (max falling speed) with the fall speed factor to fix the falling animation
                // let's search for all 160 occurrences in the IL code
                while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 160f)) {
                    Logger.Log("ExtendedVariantsModule", $"Applying fall speed and gravity to constant at {cursor.Index} in CIL code for UpdateSprite to fix animation");

                    // add two instructions to multiply those constants with a mix between fall speed and gravity
                    cursor.EmitDelegate<Func<float>>(MixFallSpeedAndGravity);
                    cursor.Emit(OpCodes.Mul);
                    // also remove 0.1 to prevent an animation glitch caused by rounding (I guess?) on very low fall speeds
                    cursor.Emit(OpCodes.Ldc_R4, 0.1f);
                    cursor.Emit(OpCodes.Sub);
                }

                // chain every other UpdateSprite usage
                ModUpdateSpriteFriction(il);
            });
        }

        /// <summary>
        /// Returns the currently configured fall speed factor.
        /// </summary>
        /// <returns>The fall speed factor (1 = default fall speed)</returns>
        public static float DetermineFallSpeedFactor() {
            return Settings.FallSpeedFactor;
        }

        public static float MixFallSpeedAndGravity() {
            return Math.Min(Settings.FallSpeedFactor, Settings.GravityFactor);
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
            // (actually, this is V_28 in the FNA version)
            if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 90f)
                && cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Stloc_S
                    && (((VariableDefinition)instr.Operand).Index == 6 || ((VariableDefinition)instr.Operand).Index == 28))) {

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
        /// Edits the SuperJump method in Player (called when super/hyperdashing.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModSuperJump(ILContext il) {
            ModMethod("SuperJump", () => {
                ILCursor cursor = new ILCursor(il);

                // we want to multiply 260f (speed given by a superdash) with the X speed factor
                while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 260f)) {
                    Logger.Log("ExtendedVariantsModule", $"Applying X speed to constant at {cursor.Index} in CIL code for SuperJump");
                    cursor.EmitDelegate<Func<float>>(DetermineSpeedXFactor);
                    cursor.Emit(OpCodes.Mul);
                    cursor.EmitDelegate<Func<float>>(DetermineHyperdashSpeedFactor);
                    cursor.Emit(OpCodes.Mul);
                }

                // chain every other SuperJump usage
                ModSuperJumpHeight(il);
            });
        }
        
        /// <summary>
        /// Edits the SuperJump method in Player (called when super/hyperdashing on a wall.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModSuperWallJump(ILContext il)  {
            ModMethod("SuperWallJump", () => {
                ILCursor cursor = new ILCursor(il);

                // we want to multiply 170f (X speed given by a superdash) with the X speed factor
                while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 170f)) {
                    Logger.Log("ExtendedVariantsModule", $"Applying X speed to constant at {cursor.Index} in CIL code for SuperWallJump");
                    cursor.EmitDelegate<Func<float>>(DetermineSpeedXFactor);
                    cursor.Emit(OpCodes.Mul);
                }

                // chain every other SuperWallJump usage
                ModSuperWallJumpHeight(il);
            });
        }

        /// <summary>
        /// Edits the WallJump method in Player (called when walljumping, obviously.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModWallJump(ILContext il) {
            ModMethod("WallJump", () => {
                ILCursor cursor = new ILCursor(il);

                // we want to multiply 130f (X speed given by a walljump) with the X speed factor
                while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 130f)) {
                    Logger.Log("ExtendedVariantsModule", $"Applying X speed to constant at {cursor.Index} in CIL code for WallJump");
                    cursor.EmitDelegate<Func<float>>(DetermineSpeedXFactor);
                    cursor.Emit(OpCodes.Mul);
                }

                // chain every other WallJump usage
                ModWallJumpHeight(il);
                ModWallJumpNeutralJumping(il);
            });
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
        /// Edits the ClimbUpdate method in Player (handling the player state when climbing).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModClimbUpdate(ILContext il) {
            ModMethod("ClimbUpdate", () => {
                patchOutStamina(il);
            });
        }

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
        /// Mods the SmashRoutine in SummitGem.
        /// </summary>
        /// <param name="orig">The original method</param>
        /// <param name="self">The SummitGem instance</param>
        /// <param name="player">The player</param>
        /// <param name="level">(unused)</param>
        /// <returns></returns>
        private IEnumerator ModSummitGemSmash(On.Celeste.SummitGem.orig_SmashRoutine orig, SummitGem self, Player player, Level level) {
            IEnumerator coroutine = orig.Invoke(self, player, level);

            // get the first value, this includes the code setting stamina back to 110f
            coroutine.MoveNext();
            yield return coroutine.Current;

            player.Stamina = DetermineBaseStamina();

            // leave the rest of the coroutine intact
            while (coroutine.MoveNext()) {
                yield return coroutine.Current;
            }
            yield break;
        }

        /// <summary>
        /// Wraps the Update method in the base game (used to refresh the player state).
        /// </summary>
        /// <param name="orig">The original Update method</param>
        /// <param name="self">The Player instance</param>
        public void ModUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            // since we cannot patch IL in orig_Update, we will wrap it and try to guess if the stamina was reset
            // this is **certainly** the case if the stamina changed and is now 110
            float staminaBeforeCall = self.Stamina;
            orig.Invoke(self);
            if (self.Stamina == 110f && staminaBeforeCall != 110f) {
                // reset it to the value we chose instead of 110
                self.Stamina = DetermineBaseStamina();
            }

            // chain the other functions of Update()
            ModUpdateChangeVariantsRandomly(orig, self);
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
            // invoking the original method is not really useful, but another mod may try to hook it, so don't break it if the Stamina variant is disabled
            orig.Invoke(self);

            if (Settings.Stamina != 11) {
                self.Stamina = DetermineBaseStamina();
            }
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
                // (those are actually brtrue in the XNA version and brfalse in the FNA version. Seriously?)
                if (cursor.TryGotoNext(MoveType.After, instr => (instr.OpCode == OpCodes.Brtrue || instr.OpCode == OpCodes.Brfalse)) && 
                    cursor.TryGotoNext(MoveType.After, instr => (instr.OpCode == OpCodes.Brtrue || instr.OpCode == OpCodes.Brfalse))) {
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

            // chain call to this
            ModifyDashSpeedInvertDashes(self);
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
                while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_I4_2 && (instr.Next.OpCode == OpCodes.Bne_Un_S || instr.Next.OpCode == OpCodes.Ceq))) {
                    Logger.Log("ExtendedVariantsModule", $"Fixing hair color when having more than 2 dashes by modding a check at {cursor.Index} in CIL code for UpdateHair");

                    if (cursor.Next.OpCode == OpCodes.Bne_Un_S) {
                        // XNA version: this is a branch
                        // small trap: the instruction in CIL code actually says "jump if **not** equal to 2". So we set it to "jump if lower than 2" instead
                        cursor.Next.OpCode = OpCodes.Blt_Un_S;
                    } else {
                        // FNA version: this is a boolean FOLLOWED by a branch
                        // we're turning this boolean from "Dashes == 2" to "Dashes > 1"
                        cursor.Prev.OpCode = OpCodes.Ldc_I4_1;
                        cursor.Next.OpCode = OpCodes.Cgt;
                    }
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
        /// Edits the SuperJump method in Player (called when super/hyperdashing.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModSuperJumpHeight(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply -105f (height given by a superdash) with the jump height factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == -105f)) {
                Logger.Log("ExtendedVariantsModule", $"Applying jump height to constant at {cursor.Index} in CIL code for SuperJump");
                cursor.EmitDelegate<Func<float>>(DetermineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Edits the WallJump method in Player (called when walljumping, obviously.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModWallJumpHeight(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply -105f (height given by a superdash) with the jump height factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == -105f)) {
                Logger.Log("ExtendedVariantsModule", $"Applying jump height to constant at {cursor.Index} in CIL code for WallJump");
                cursor.EmitDelegate<Func<float>>(DetermineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Edits the SuperWallJump method in Player (called when super/hyperdashing on a wall.)
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModSuperWallJumpHeight(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply -160f (height given by a superdash) with the jump height factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == -160f)) {
                Logger.Log("ExtendedVariantsModule", $"Applying jump height to constant at {cursor.Index} in CIL code for SuperWallJump");
                cursor.EmitDelegate<Func<float>>(DetermineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
                cursor.EmitDelegate<Func<float>>(DetermineWallBouncingSpeedFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        /// <summary>
        /// Returns the currently configured jump height factor.
        /// </summary>
        /// <returns>The jump height factor (1 = default jump height)</returns>
        public static float DetermineJumpHeightFactor() {
            return Settings.JumpHeightFactor;
        }

        // ================ Disable walljumping handling ================

        /// <summary>
        /// Detour the WallJump method in order to disable it if we want.
        /// </summary>
        /// <param name="orig">the original method</param>
        /// <param name="self">the player</param>
        /// <param name="dir">the wall jump direction</param>
        private void ModOnWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
            if(!Settings.DisableWallJumping) {
                orig.Invoke(self, dir);
            }
        }

        // ================ Jump count handling ================

        private static int jumpBuffer = 0;

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.) to apply jump count.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModNormalUpdateJumpCount(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to whenever jumpGraceTimer is retrieved
            if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldfld && ((FieldReference)instr.Operand).Name.Contains("jumpGraceTimer"))) {
                Logger.Log("ExtendedVariantsModule", $"Patching double jump in at {cursor.Index} in CIL code for NormalUpdate");

                // store a reference to it
                FieldReference refToJumpGraceTimer = ((FieldReference)cursor.Prev.Operand);

                // and proceed replacing it
                cursor.EmitDelegate<Func<float, float>>(CanJump);

                // go back to the beginning of the method
                cursor.Index = 0;
                // and add a call to RefillJumpBuffer so that we can reset the jumpBuffer even if we cannot access jumpGraceTimer (being private)
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, refToJumpGraceTimer);
                cursor.EmitDelegate<Action<float>>(RefillJumpBuffer);
            }
        }

        public static void RefillJumpBuffer(float jumpGraceTimer) {
            // JumpCount - 1 because the first jump is from vanilla Celeste
            if (jumpGraceTimer > 0f) jumpBuffer = Settings.JumpCount - 1;
        }

        /// <summary>
        /// Detour the WallJump method in order to disable it if we want.
        /// </summary>
        public static float CanJump(float initialJumpGraceTimer) {
            if(Settings.JumpCount == 0) {
                // we disabled jumping, so let's pretend the grace timer has run out
                return 0f;
            }
            if(Settings.JumpCount == 6) {
                // infinite jumping, yay
                return 1f;
            }
            if(Settings.JumpCount == 1 || initialJumpGraceTimer > 0f || jumpBuffer <= 0) {
                // return the default value because we don't want to change anything 
                // (we are disabled, our jump buffer ran out, or vanilla Celeste allows jumping anyway)
                return initialJumpGraceTimer;
            }
            // consume an Extended Variant Jump(TM)
            jumpBuffer--;
            return 1f;
        }


        // ================ Upside down handling ================

        /// <summary>
        /// Edits the Render method in Level (handling the whole level rendering).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModLevelRender(ILContext il) {
            ModMethod("LevelRender", () => {
                ILCursor cursor = new ILCursor(il);

                // jump right where Mirror Mode is handled
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldfld && ((FieldReference)instr.Operand).Name.Contains("MirrorMode"))) {
                    // move back 2 steps (we are between Instance and MirrorMode in "SaveData.Instance.MirrorMode" and we want to be before that)
                    cursor.Index -= 2;

                    VariableDefinition positionVector = seekReferenceTo(il, cursor.Index, 4);
                    VariableDefinition paddingVector = seekReferenceTo(il, cursor.Index, 8);

                    if(positionVector == null || paddingVector == null) {
                        positionVector = seekReferenceTo(il, cursor.Index, 7);
                        paddingVector = seekReferenceTo(il, cursor.Index, 11);
                    }

                    if(positionVector != null && paddingVector != null) {
                        // insert our delegates to do about the same thing as vanilla Celeste at about the same time
                        Logger.Log("ExtendedVariantsModule", $"Adding upside down delegate call at {cursor.Index} in CIL code for LevelRender");

                        cursor.Emit(OpCodes.Ldloca_S, paddingVector);
                        cursor.Emit(OpCodes.Ldloca_S, positionVector);
                        cursor.EmitDelegate<TwoRefVectorParameters>(ApplyUpsideDownEffect);
                    }
                }

                // move forward a bit to get after the MirrorMode loading
                cursor.Index += 3;

                // jump to the next MirrorMode usage again
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldfld && ((FieldReference)instr.Operand).Name.Contains("MirrorMode"))) {
                    // jump back 2 steps
                    cursor.Index -= 2;

                    Logger.Log("ExtendedVariantsModule", $"Adding upside down delegate call at {cursor.Index} in CIL code for LevelRender");

                    // erase "SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None"
                    // that's 3 instructions to load MirrorMode, and 4 assigning either 1 or 0 to it
                    cursor.RemoveRange(7);

                    // and replace it with a delegate call
                    cursor.EmitDelegate<Func<SpriteEffects>>(ApplyUpsideDownEffectToSprites);
                }
            });
        }

        /// <summary>
        /// Seeks any reference to a numbered variable in IL code.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        /// <param name="variableIndex">Index of the variable</param>
        /// <returns>A reference to the variable</returns>
        private static VariableDefinition seekReferenceTo(ILContext il, int startingPoint, int variableIndex) {
            ILCursor cursor = new ILCursor(il);
            cursor.Index = startingPoint;
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldloc_S && ((VariableDefinition)instr.Operand).Index == variableIndex)) {
                return (VariableDefinition)cursor.Next.Operand;
            }
            return null;
        }

        public delegate void TwoRefVectorParameters(ref Vector2 one, ref Vector2 two);
        
        public static void ApplyUpsideDownEffect(ref Vector2 paddingVector, ref Vector2 positionVector) {
            Input.Aim.InvertedY = (Input.MoveY.Inverted = Settings.UpsideDown);

            if(Settings.UpsideDown) {
                paddingVector.Y = -paddingVector.Y;
                positionVector.Y = 90f - (positionVector.Y - 90f);
            }
        }

        public static SpriteEffects ApplyUpsideDownEffectToSprites() {
            SpriteEffects effects = SpriteEffects.None;
            if (Settings.UpsideDown) effects |= SpriteEffects.FlipVertically;
            if (SaveData.Instance.Assists.MirrorMode) effects |= SpriteEffects.FlipHorizontally;
            return effects;
        }

        // ================ Hyperdash speed handling ================

        /// <summary>
        /// Returns the current hyperdash speed factor.
        /// </summary>
        /// <returns>The hyperdash speed factor (1 = default hyperdash speed)</returns>
        public static float DetermineHyperdashSpeedFactor() {
            return Settings.HyperdashSpeedFactor;
        }


        // ================ Wallbouncing speed handling ================

        /// <summary>
        /// Returns the current wallbounce speed factor.
        /// </summary>
        /// <returns>The wallbounce speed factor (1 = default wallbounce speed)</returns>
        public static float DetermineWallBouncingSpeedFactor()
        {
            return Settings.WallBouncingSpeedFactor;
        }

        // ================ Dash length handling ================

        private static float lastDashDuration = 0f;

        /// <summary>
        /// Edits the DashBegin method in Player (called when the player dashes).
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModDashBegin(ILContext il) {
            ModMethod("DashBegin", () => {
                ILCursor cursor = new ILCursor(il);

                // jump where 0.3 is loaded (0.3 is the dash timer)
                if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && (float)instr.Operand == 0.3f)) {
                    Logger.Log("ExtendedVariantsModule", $"Applying dash length to constant at {cursor.Index} in CIL code for DashBegin");

                    cursor.EmitDelegate<Func<float>>(DetermineDashLengthFactor);
                    cursor.Emit(OpCodes.Mul);
                }
            });
        }

        /// <summary>
        /// Returns the current dash length factor.
        /// </summary>
        /// <returns>The dash length factor (1 = default dash length)</returns>
        public static float DetermineDashLengthFactor() {
            return Settings.DashLengthFactor;
        }


        private IEnumerator ModDashCoroutine(On.Celeste.Player.orig_DashCoroutine orig, Player self) {
            // let's try and intercept whenever the DashCoroutine sends out 0.3f or 0.15f
            // because we should mod that
            IEnumerator coroutine = orig.Invoke(self);
            while(coroutine.MoveNext()) {
                object o = coroutine.Current;
                if(o != null && o.GetType() == typeof(float)) {
                    float f = (float)o;
                    if (f == 0.15f || f == 0.3f) {
                        f *= Settings.DashLengthFactor;
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
        public static void ModDashUpdate(ILContext il) {
            ModMethod("DashUpdate", () => {
                FieldReference dashTrailCounter = seekReferenceToVariable(il, "dashTrailCounter");

                if (dashTrailCounter != null) {
                    ILCursor cursor = new ILCursor(il);

                    Logger.Log("ExtendedVariantsModule", $"Patching dashTrailCounter to fix animation with long dashes at {cursor.Index} in CIL code for DashUpdate");

                    // add a delegate call to modify dashTrailCounter (private variable set in DashCoroutine we can't mod with IL)
                    // so that we add more trails if the dash is made longer than usual
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Ldfld, dashTrailCounter);
                    cursor.EmitDelegate<Func<int, int>>(ModDashTrailCounter);
                    cursor.Emit(OpCodes.Stfld, dashTrailCounter);
                }
            });
        }

        private static int ModDashTrailCounter(int dashTrailCounter) {
            if (Settings.DashLengthFactor != 1 && lastDashDuration != 0f) {
                float bakLastDashDuration = lastDashDuration;
                lastDashDuration = 0f;
                return (int)Math.Round(bakLastDashDuration * 10 * Settings.DashLengthFactor) - 1;
            }
            return dashTrailCounter;
        }

        /// <summary>
        /// Seeks any reference to a named variable in IL code.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        /// <param name="variableName">name of the variable</param>
        /// <returns>A reference to the variable</returns>
        private static FieldReference seekReferenceToVariable(ILContext il, String variableName) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldfld && ((FieldReference)instr.Operand).Name.Contains(variableName))) {
                return (FieldReference)cursor.Next.Operand;
            }
            return null;
        }

        // ================ Force duck on ground handling ================

        /// <summary>
        /// Edits the NormalUpdate method in Player (handling the player state when not doing anything like climbing etc.)
        /// to handle the "force duck on ground" variant.
        /// </summary>
        /// <param name="il">Object allowing CIL patching</param>
        public static void ModNormalUpdateForceDuckOnGround(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to "if(this.Ducking)" => that's a brfalse
            // (or, in the FNA version, "bool ducking = this.Ducking;" => that's a stloc.s)
            if (cursor.TryGotoNext(MoveType.After, 
                instr => instr.OpCode == OpCodes.Callvirt && ((MethodReference)instr.Operand).Name.Contains("get_Ducking"),
                instr => (instr.OpCode == OpCodes.Brfalse || instr.OpCode == OpCodes.Stloc_S))
                // in the XNA version, we get after the brfalse. In the FNA version, we have a ldloc.s and a brfalse after the cursor
                && (cursor.Prev.OpCode == OpCodes.Brfalse || cursor.Next.Next.OpCode == OpCodes.Brfalse)) {

                if(cursor.Prev.OpCode == OpCodes.Stloc_S) {
                    // get after the brfalse in order to line up with the XNA version
                    cursor.Index += 2;
                }

                Logger.Log("ExtendedVariantsModule", $"Inserting condition to enforce Force Duck On Ground at {cursor.Index} in CIL code for NormalUpdate");

                ILLabel target = (ILLabel)cursor.Prev.Operand;

                // basically, this turns the if into "if(this.Ducking && !Settings.ForceDuckOnGround)": this prevents unducking
                cursor.EmitDelegate<Func<bool>>(ForceDuckOnGroundEnabled);
                cursor.Emit(OpCodes.Brtrue, target);

                // jump to the "else" to modify this one too
                cursor.GotoLabel(target);

                // set ourselves just before the condition we want to mod
                if(cursor.TryGotoNext(MoveType.Before, instr => instr.OpCode == OpCodes.Ldsfld && ((FieldReference)instr.Operand).Name.Contains("MoveY"))) {
                    ILCursor cursorAfterCondition = cursor.Clone();

                    if (cursorAfterCondition.TryGotoNext(MoveType.After, instr => (instr.OpCode == OpCodes.Bne_Un || instr.OpCode == OpCodes.Bne_Un_S))) {
                        Logger.Log("ExtendedVariantsModule", $"Inserting condition to enforce Force Duck On Ground at {cursor.Index} in CIL code for NormalUpdate");

                        // so this is basically "if (this.onGround && (Settings.ForceDuckOnGround || Input.MoveY == 1) && this.Speed.Y >= 0f)"
                        // by telling IL "if Settings.ForceDuckOnGround is true, jump over the Input.MoveY check"
                        cursor.EmitDelegate<Func<bool>>(ForceDuckOnGroundEnabled);
                        cursor.Emit(OpCodes.Brtrue, cursorAfterCondition.Next);
                    }
                }
            }
        }

        private static bool ForceDuckOnGroundEnabled() => Settings.ForceDuckOnGround;

        // ================ Invert Dashes handling ================

        /// <summary>
        /// Inverts the dash direction of the player.
        /// </summary>
        /// <param name="self">A reference to the player</param>
        public static void ModifyDashSpeedInvertDashes(Player self) {
            if (Settings.InvertDashes) {
                self.Speed *= -1;
                self.DashDir *= -1;
            }
        }


        // ================ Disable Neutral Jumping handling ================

        private static void ModWallJumpNeutralJumping(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to the first MoveX usage (this.MoveX => ldarg.0 then ldfld MoveX basically)
            if(cursor.TryGotoNext(MoveType.Before,
                instr => instr.OpCode == OpCodes.Ldarg_0,
                instr => instr.OpCode == OpCodes.Ldfld && ((FieldReference)instr.Operand).Name.Contains("moveX"))) {

                ILCursor cursorAfterBranch = cursor.Clone();
                if(cursorAfterBranch.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Brfalse_S)) {
                    Logger.Log("ExtendedVariantsModule", $"Inserting condition to enforce Disable Neutral Jumping at {cursor.Index} in CIL code for WallJump");

                    // before the MoveX check, check if neutral jumping is enabled: if it is not, skip the MoveX check
                    cursor.EmitDelegate<Func<bool>>(NeutralJumpingEnabled);
                    cursor.Emit(OpCodes.Brfalse_S, cursorAfterBranch.Next);
                }
            }
        }

        /// <summary>
        /// Indicates if neutral jumping is enabled.
        /// </summary>
        public static bool NeutralJumpingEnabled() {
            return !Settings.DisableNeutralJumping;
        }

        // ================ Change Variants Randomly handling ================

        private float changeVariantTimer = 9999f;
        private Random randomGenerator = new Random();

        public void ModUpdateChangeVariantsRandomly(On.Celeste.Player.orig_Update orig, Player self) {
            // don't bother doing anything if Change Variants Randomly is off, or if it is set to Vanilla Only and vanilla Variant Mode is off
            if(Settings.ChangeVariantsRandomly != 0 && (Settings.ChangeVariantsRandomly != 1 || SaveData.Instance.VariantMode)) {
                changeVariantTimer -= Engine.DeltaTime;

                if(changeVariantTimer < 0) {
                    changeVariantTimer = Settings.ChangeVariantsInterval;

                    bool isVanilla;
                    switch(Settings.ChangeVariantsRandomly) {
                        case 1: isVanilla = true; break;
                        case 2: isVanilla = false; break;
                        default: isVanilla = randomGenerator.Next(2) == 0; break;
                    }

                    // never enable vanilla variants if Variant Mode is off, that would be odd
                    if (!SaveData.Instance.VariantMode) isVanilla = false;

                    if(isVanilla) {
                        switch (randomGenerator.Next(11)) {
                            case 0:
                                SaveData.Instance.Assists.MirrorMode = !SaveData.Instance.Assists.MirrorMode;
                                Input.Aim.InvertedX = SaveData.Instance.Assists.MirrorMode;
                                Input.MoveX.Inverted = SaveData.Instance.Assists.MirrorMode;
                                break;
                            case 1: SaveData.Instance.Assists.GameSpeed = new int[] { 5, 6, 7, 8, 9, 10, 12, 14, 16 }[randomGenerator.Next(9)]; break;
                            case 2: SaveData.Instance.Assists.Invincible = !SaveData.Instance.Assists.Invincible; break;
                            case 3: SaveData.Instance.Assists.DashMode = new Assists.DashModes[] { Assists.DashModes.Normal, Assists.DashModes.Two, Assists.DashModes.Infinite }[randomGenerator.Next(3)]; break;
                            case 4: SaveData.Instance.Assists.InfiniteStamina = !SaveData.Instance.Assists.InfiniteStamina; break;
                            case 5: SaveData.Instance.Assists.ThreeSixtyDashing = !SaveData.Instance.Assists.ThreeSixtyDashing; break;
                            case 6: SaveData.Instance.Assists.InvisibleMotion = !SaveData.Instance.Assists.InvisibleMotion; break;
                            case 7: SaveData.Instance.Assists.NoGrabbing = !SaveData.Instance.Assists.NoGrabbing; break;
                            case 8: SaveData.Instance.Assists.LowFriction = !SaveData.Instance.Assists.LowFriction; break;
                            case 9: SaveData.Instance.Assists.SuperDashing = !SaveData.Instance.Assists.SuperDashing; break;
                            case 10: SaveData.Instance.Assists.Hiccups = !SaveData.Instance.Assists.Hiccups; break;
                        }
                    } else {
                        switch (randomGenerator.Next(17)) {
                            case 0: Settings.Gravity = multiplierScale[randomGenerator.Next(23)]; break;
                            case 1: Settings.FallSpeed = multiplierScale[randomGenerator.Next(23)]; break;
                            case 2: Settings.JumpHeight = multiplierScale[randomGenerator.Next(23)]; break;
                            case 3: Settings.DisableWallJumping = !Settings.DisableWallJumping; break;
                            case 4: Settings.JumpCount = (Settings.JumpCount != 1 ? 1 : randomGenerator.Next(7)); break;
                            case 5: Settings.DashSpeed = multiplierScale[randomGenerator.Next(23)]; break;
                            case 6: Settings.DashLength = multiplierScale[randomGenerator.Next(23)]; break;
                            case 7: Settings.HyperdashSpeed = multiplierScale[randomGenerator.Next(23)]; break;
                            case 8: Settings.WallBouncingSpeed = multiplierScale[randomGenerator.Next(23)]; break;
                            case 9: Settings.DashCount = (Settings.DashCount != -1 ? -1 : randomGenerator.Next(6)); break;
                            case 10: Settings.SpeedX = multiplierScale[randomGenerator.Next(23)]; break;
                            case 11: Settings.Friction = multiplierScale[randomGenerator.Next(23)]; break;
                            case 12: Settings.Stamina = randomGenerator.Next(23); break;
                            case 13: Settings.UpsideDown = !Settings.UpsideDown; break;
                            case 14: Settings.ForceDuckOnGround = !Settings.ForceDuckOnGround; break;
                            case 15: Settings.InvertDashes = !Settings.InvertDashes; break;
                            case 16: Settings.DisableNeutralJumping = !Settings.DisableNeutralJumping; break;
                        }
                    }
                }
            }
        }
    }
}
