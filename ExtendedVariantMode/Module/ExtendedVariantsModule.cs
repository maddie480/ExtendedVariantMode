using System;
using System.Collections.Generic;
using Monocle;
using FMOD.Studio;
using System.Collections;
using Celeste.Mod;
using ExtendedVariants.UI;
using Celeste;
using ExtendedVariants.Variants;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using ExtendedVariants.Entities;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsModule : EverestModule {

        public static ExtendedVariantsModule Instance;

        public bool DJMapHelperInstalled { get; private set; }
        public bool SpringCollab2020Installed { get; private set; }
        public bool MaxHelpingHandInstalled { get; private set; }

        private bool stuffIsHooked = false;
        private bool triggerIsHooked = false;
        private bool showForcedVariantsPostcard = false;
        private Postcard forceEnabledVariantsPostcard;
        private bool isLevelEnding = false;

        public override Type SettingsType => typeof(ExtendedVariantsSettings);
        public override Type SessionType => typeof(ExtendedVariantsSession);

        public static ExtendedVariantsSettings Settings => (ExtendedVariantsSettings) Instance._Settings;
        public static ExtendedVariantsSession Session => (ExtendedVariantsSession) Instance._Session;

        public VariantRandomizer Randomizer;

        public enum Variant {
            Gravity, FallSpeed, JumpHeight, WallBouncingSpeed, DisableWallJumping, DisableClimbJumping, JumpCount, RefillJumpsOnDashRefill, DashSpeed, DashLength,
            HyperdashSpeed, ExplodeLaunchSpeed, DashCount, HeldDash, DontRefillDashOnGround, SpeedX, Friction, AirFriction, BadelineChasersEverywhere, ChaserCount,
            AffectExistingChasers, BadelineBossesEverywhere, BadelineAttackPattern, ChangePatternsOfExistingBosses, FirstBadelineSpawnRandom,
            BadelineBossCount, BadelineBossNodeCount, BadelineLag, DelayBetweenBadelines, OshiroEverywhere, OshiroCount, ReverseOshiroCount, DisableOshiroSlowdown,
            WindEverywhere, SnowballsEverywhere, SnowballDelay, AddSeekers, DisableSeekerSlowdown, TheoCrystalsEverywhere, AllowThrowingTheoOffscreen, AllowLeavingTheoBehind,
            Stamina, UpsideDown, DisableNeutralJumping, RegularHiccups, HiccupStrength, RoomLighting, RoomBloom, GlitchEffect, EverythingIsUnderwater, ForceDuckOnGround,
            InvertDashes, InvertGrab, AllStrawberriesAreGoldens, GameSpeed, ColorGrading, JellyfishEverywhere, RisingLavaEverywhere, RisingLavaSpeed, InvertHorizontalControls,
            BounceEverywhere, SuperdashSteeringSpeed, ScreenShakeIntensity, AnxietyEffect, BlurLevel, ZoomLevel, DashDirection, BackgroundBrightness, DisableMadelineSpotlight,
            ForegroundEffectOpacity, MadelineIsSilhouette, DashTrailAllTheTime, DisableClimbingUpOrDown, SwimmingSpeed, BoostMultiplier, FriendlyBadelineFollower,
            DisableRefillsOnScreenTransition, RestoreDashesOnRespawn, DisableSuperBoosts, DisplayDashCount
        }

        public Dictionary<Variant, AbstractExtendedVariant> VariantHandlers = new Dictionary<Variant, AbstractExtendedVariant>();

        public ExtendedVariantTriggerManager TriggerManager;

        // ================ Module loading ================

        public ExtendedVariantsModule() {
            Instance = this;
            Randomizer = new VariantRandomizer();
            TriggerManager = new ExtendedVariantTriggerManager();

            DashCount dashCount;
            ZoomLevel zoomLevel;
            VariantHandlers[Variant.Gravity] = new Gravity();
            VariantHandlers[Variant.FallSpeed] = new FallSpeed();
            VariantHandlers[Variant.JumpHeight] = new JumpHeight();
            VariantHandlers[Variant.SpeedX] = new SpeedX();
            VariantHandlers[Variant.Stamina] = new Stamina();
            VariantHandlers[Variant.DashSpeed] = new DashSpeed();
            VariantHandlers[Variant.DashCount] = (dashCount = new DashCount());
            VariantHandlers[Variant.HeldDash] = new HeldDash();
            VariantHandlers[Variant.Friction] = new Friction();
            VariantHandlers[Variant.AirFriction] = new AirFriction();
            VariantHandlers[Variant.DisableWallJumping] = new DisableWallJumping();
            VariantHandlers[Variant.DisableClimbJumping] = new DisableClimbJumping();
            VariantHandlers[Variant.JumpCount] = new JumpCount(dashCount);
            VariantHandlers[Variant.ZoomLevel] = (zoomLevel = new ZoomLevel());
            VariantHandlers[Variant.UpsideDown] = new UpsideDown(zoomLevel);
            VariantHandlers[Variant.HyperdashSpeed] = new HyperdashSpeed();
            VariantHandlers[Variant.ExplodeLaunchSpeed] = new ExplodeLaunchSpeed();
            // DisableSuperBoosts is not a variant
            VariantHandlers[Variant.WallBouncingSpeed] = new WallbouncingSpeed();
            VariantHandlers[Variant.DashLength] = new DashLength();
            VariantHandlers[Variant.ForceDuckOnGround] = new ForceDuckOnGround();
            VariantHandlers[Variant.InvertDashes] = new InvertDashes();
            VariantHandlers[Variant.InvertGrab] = new InvertGrab();
            VariantHandlers[Variant.DisableNeutralJumping] = new DisableNeutralJumping();
            VariantHandlers[Variant.BadelineChasersEverywhere] = new BadelineChasersEverywhere();
            // ChaserCount is not a variant
            // AffectExistingChasers is not a variant
            VariantHandlers[Variant.BadelineBossesEverywhere] = new BadelineBossesEverywhere();
            // BadelineAttackPattern is not a variant
            // ChangePatternsOfExistingBosses is not a variant
            // FirstBadelineSpawnRandom is not a variant
            // BadelineBossCount is not a variant
            // BadelineBossNodeCount is not a variant
            VariantHandlers[Variant.RegularHiccups] = new RegularHiccups();
            // HiccupStrength is not a variant
            // RefillJumpsOnDashRefill is not a variant
            VariantHandlers[Variant.RoomLighting] = new RoomLighting();
            VariantHandlers[Variant.RoomBloom] = new RoomBloom();
            VariantHandlers[Variant.GlitchEffect] = new GlitchEffect();
            VariantHandlers[Variant.AnxietyEffect] = new AnxietyEffect();
            VariantHandlers[Variant.BlurLevel] = new BlurLevel();
            VariantHandlers[Variant.EverythingIsUnderwater] = new EverythingIsUnderwater();
            VariantHandlers[Variant.OshiroEverywhere] = new OshiroEverywhere();
            // OshiroCount is not a variant
            // ReverseOshiroCount is not a variant
            // DisableOshiroSlowdown is not a variant
            VariantHandlers[Variant.WindEverywhere] = new WindEverywhere();
            VariantHandlers[Variant.SnowballsEverywhere] = new SnowballsEverywhere();
            // SnowballDelay is not a variant
            VariantHandlers[Variant.AddSeekers] = new AddSeekers();
            // DisableSeekerSlowdown is not a variant
            VariantHandlers[Variant.TheoCrystalsEverywhere] = new TheoCrystalsEverywhere();
            // AllowThrowingTheoOffscreen is not a variant
            // AllowLeavingTheoBehind is not a variant
            VariantHandlers[Variant.RisingLavaEverywhere] = new RisingLavaEverywhere();
            // RisingLavaSpeed is not a variant
            // BadelineLag is not a variant
            // DelayBetweenBadelines is not a variant
            VariantHandlers[Variant.AllStrawberriesAreGoldens] = new AllStrawberriesAreGoldens();
            VariantHandlers[Variant.DontRefillDashOnGround] = new DontRefillDashOnGround();
            VariantHandlers[Variant.GameSpeed] = new GameSpeed();
            VariantHandlers[Variant.ColorGrading] = new ColorGrading();
            VariantHandlers[Variant.JellyfishEverywhere] = new JellyfishEverywhere();
            VariantHandlers[Variant.InvertHorizontalControls] = new InvertHorizontalControls();
            VariantHandlers[Variant.BounceEverywhere] = new BounceEverywhere();
            VariantHandlers[Variant.SuperdashSteeringSpeed] = new SuperdashSteeringSpeed();
            VariantHandlers[Variant.ScreenShakeIntensity] = new ScreenShakeIntensity();
            VariantHandlers[Variant.DashDirection] = new DashDirection();
            VariantHandlers[Variant.BackgroundBrightness] = new BackgroundBrightness();
            VariantHandlers[Variant.DisableMadelineSpotlight] = new DisableMadelineSpotlight();
            VariantHandlers[Variant.ForegroundEffectOpacity] = new ForegroundEffectOpacity();
            // MadelineIsSilhouette is instanciated in Initialize
            VariantHandlers[Variant.DashTrailAllTheTime] = new DashTrailAllTheTime();
            VariantHandlers[Variant.DisableClimbingUpOrDown] = new DisableClimbingUpOrDown();
            VariantHandlers[Variant.SwimmingSpeed] = new SwimmingSpeed();
            VariantHandlers[Variant.BoostMultiplier] = new BoostMultiplier();
            VariantHandlers[Variant.FriendlyBadelineFollower] = new FriendlyBadelineFollower();
            VariantHandlers[Variant.DisableRefillsOnScreenTransition] = new DisableRefillsOnScreenTransition();
            VariantHandlers[Variant.RestoreDashesOnRespawn] = new RestoreDashesOnRespawn();
            VariantHandlers[Variant.DisplayDashCount] = new DisplayDashCount();
        }

        // ================ Mod options setup ================

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            base.CreateModMenuSection(menu, inGame, snapshot);

            if (Settings.OptionsOutOfModOptionsMenuEnabled && inGame) {
                // build the menu with only the master switch
                new ModOptionsEntries().CreateAllOptions(ModOptionsEntries.VariantCategory.None, true, false, false, false,
                    null /* don't care, no submenu */, menu, inGame, triggerIsHooked);
            } else if (Settings.SubmenusForEachCategoryEnabled) {
                // build the menu with the master switch + submenus + randomizer options
                new ModOptionsEntries().CreateAllOptions(ModOptionsEntries.VariantCategory.None, true, true, true, false,
                    () => OuiModOptions.Instance.Overworld.Goto<OuiModOptions>(), menu, inGame, triggerIsHooked);
            } else if (Settings.OptionsOutOfModOptionsMenuEnabled) {
                // build the menu with the master switch + the button to open the submenu
                new ModOptionsEntries().CreateAllOptions(ModOptionsEntries.VariantCategory.None, true, false, false, true,
                    null /* don't care, no submenu */, menu, inGame, triggerIsHooked);
            } else {
                // build the good old full menu directly in Mod Options (master switch + all variant categories + randomizer)
                new ModOptionsEntries().CreateAllOptions(ModOptionsEntries.VariantCategory.All, true, false, true, false,
                    () => OuiModOptions.Instance.Overworld.Goto<OuiModOptions>(), menu, inGame, triggerIsHooked);
            }
        }

        private void onCreatePauseMenuButtons(Level level, TextMenu menu, bool minimal) {
            int optionsIndex = menu.GetItems().FindIndex(item =>
                item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button) item).Label == Dialog.Clean("menu_pause_options"));

            // insert ourselves just before Options if required (this is below Variants if variant mode is enabled)
            if (Settings.OptionsOutOfModOptionsMenuEnabled) {
                menu.Insert(optionsIndex, AbstractSubmenu.BuildOpenMenuButton<OuiExtendedVariantsSubmenu>(menu, true,
                    null /* this is not used when in-game anyway */, new object[] { true }));
            }
        }

        // ================ Variants hooking / unhooking ================

        public override void Load() {
            Logger.SetLogLevel("ExtendedVariantMode", LogLevel.Info);
            Logger.SetLogLevel("ExtendedVariantMode/ExtendedVariantTriggerManager", LogLevel.Verbose);
            Logger.SetLogLevel("ExtendedVariantMode/ExtendedVariantTriggerManager-fade", LogLevel.Info);

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "Initializing Extended Variant Mode");

            if (Settings.LegacyDashSpeedBehavior) {
                VariantHandlers[Variant.DashSpeed] = new DashSpeedOld();
            }

            On.Celeste.LevelEnter.Go += checkForceEnableVariantsOnLevelEnter;
            On.Celeste.LevelLoader.ctor += checkForceEnableVariantsOnLevelLoad;
            On.Celeste.LevelExit.ctor += checkForTriggerUnhooking;

            if (Settings.MasterSwitch) {
                // variants are enabled: we want to hook them on startup.
                HookStuff();
            }
        }

        public override void Unload() {
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "Unloading Extended Variant Mode");

            On.Celeste.LevelEnter.Go -= checkForceEnableVariantsOnLevelEnter;
            On.Celeste.LevelLoader.ctor -= checkForceEnableVariantsOnLevelLoad;
            On.Celeste.LevelExit.ctor -= checkForTriggerUnhooking;

            if (stuffIsHooked) {
                UnhookStuff();
            }
        }

        public override void Initialize() {
            base.Initialize();

            DashCountIndicator.Initialize();
            (VariantHandlers[Variant.ExplodeLaunchSpeed] as ExplodeLaunchSpeed).Initialize();

            DJMapHelperInstalled = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "DJMapHelper", Version = new Version(1, 7, 10) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"DJ Map Helper installed = {DJMapHelperInstalled}");
            SpringCollab2020Installed = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "SpringCollab2020", Version = new Version(1, 0, 0) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Spring Collab 2020 installed = {SpringCollab2020Installed}");
            MaxHelpingHandInstalled = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "MaxHelpingHand", Version = new Version(1, 12, 2) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Max Helping Hand installed = {MaxHelpingHandInstalled}");

            if (!DJMapHelperInstalled) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Force-disabling Reverse Oshiros");
                Settings.ReverseOshiroCount = 0;
                SaveSettings();
            }
            if (!SpringCollab2020Installed && !MaxHelpingHandInstalled) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Force-disabling Madeline Is Silhouette");
                Settings.MadelineIsSilhouette = false;
                SaveSettings();
            } else {
                // let's add this variant in now.
                VariantHandlers[Variant.MadelineIsSilhouette] = new MadelineIsSilhouette();

                if (stuffIsHooked) {
                    // and activate it if all others are already active!
                    VariantHandlers[Variant.MadelineIsSilhouette].Load();
                }
            }
        }

        private ILHook hookOnVersionNumberAndVariants;

        public void HookStuff() {
            if (stuffIsHooked) return;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Loading variant common methods...");
            On.Celeste.AreaComplete.VersionNumberAndVariants += modVersionNumberAndVariants;
            Everest.Events.Level.OnExit += onLevelExit;
            On.Celeste.BadelineBoost.BoostRoutine += modBadelineBoostRoutine;
            On.Celeste.CS00_Ending.OnBegin += onPrologueEndingCutsceneBegin;
            Everest.Events.Level.OnCreatePauseMenuButtons += onCreatePauseMenuButtons;
            hookOnVersionNumberAndVariants = new ILHook(typeof(AreaComplete).GetMethod("orig_VersionNumberAndVariants"), ilModVersionNumberAndVariants);

            On.Celeste.Level.LoadLevel += onLoadLevel;
            On.Celeste.Level.EndPauseEffects += onUnpause;
            On.Celeste.Level.End += onLevelEnd;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Loading variant randomizer...");
            Randomizer.Load();

            foreach (Variant variant in VariantHandlers.Keys) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Loading variant {variant}...");
                VariantHandlers[variant].Load();
            }

            LeakPreventionHack.Load();

            Logger.Log(LogLevel.Info, "ExtendedVariantMode/ExtendedVariantsModule", "Done hooking stuff.");

            stuffIsHooked = true;
        }

        public void UnhookStuff() {
            if (!stuffIsHooked) return;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant common methods...");
            On.Celeste.AreaComplete.VersionNumberAndVariants -= modVersionNumberAndVariants;
            Everest.Events.Level.OnExit -= onLevelExit;
            On.Celeste.BadelineBoost.BoostRoutine -= modBadelineBoostRoutine;
            On.Celeste.CS00_Ending.OnBegin -= onPrologueEndingCutsceneBegin;
            Everest.Events.Level.OnCreatePauseMenuButtons -= onCreatePauseMenuButtons;
            hookOnVersionNumberAndVariants?.Dispose();
            hookOnVersionNumberAndVariants = null;

            On.Celeste.Level.LoadLevel -= onLoadLevel;
            On.Celeste.Level.EndPauseEffects -= onUnpause;
            On.Celeste.Level.End -= onLevelEnd;

            // unset flags
            onLevelExit();

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant randomizer...");
            Randomizer.Unload();

            foreach (Variant variant in VariantHandlers.Keys) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant {variant}...");
                VariantHandlers[variant].Unload();
            }

            LeakPreventionHack.Unload();

            Logger.Log(LogLevel.Info, "ExtendedVariantMode/ExtendedVariantsModule", "Done unhooking stuff.");

            stuffIsHooked = false;
        }

        private void hookTrigger() {
            if (triggerIsHooked) return;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Loading variant trigger manager...");
            TriggerManager.Load();
            ExtendedVariantTrigger.Load();

            On.Celeste.LevelEnter.Routine += addForceEnabledVariantsPostcard;
            On.Celeste.LevelEnter.BeforeRender += addForceEnabledVariantsPostcardRendering;

            Logger.Log(LogLevel.Info, "ExtendedVariantMode/ExtendedVariantsModule", $"Done loading variant trigger manager.");

            triggerIsHooked = true;
        }

        private void unhookTrigger() {
            if (!triggerIsHooked) return;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant trigger manager...");
            TriggerManager.Unload();
            ExtendedVariantTrigger.Unload();

            On.Celeste.LevelEnter.Routine -= addForceEnabledVariantsPostcard;
            On.Celeste.LevelEnter.BeforeRender -= addForceEnabledVariantsPostcardRendering;

            Logger.Log(LogLevel.Info, "ExtendedVariantMode/ExtendedVariantsModule", $"Done unloading variant trigger manager.");

            triggerIsHooked = false;
        }

        private static HashSet<string> extendedVariantsEntities = new HashSet<string> {
            "ExtendedVariantTrigger", "ExtendedVariantMode/ExtendedVariantTrigger", "ExtendedVariantMode/ColorGradeTrigger",
            "ExtendedVariantMode/JumpRefill", "ExtendedVariantMode/RecoverJumpRefill", "ExtendedVariantMode/ExtraJumpRefill",
            "ExtendedVariantMode/ExtendedVariantFadeTrigger", "ExtendedVariantMode/ExtendedVariantController", "ExtendedVariantMode/ToggleDashDirectionTrigger"
        };

        // this one is for the normal case (entering a level from the overworld or opening a save)
        private void checkForceEnableVariantsOnLevelEnter(On.Celeste.LevelEnter.orig_Go orig, Session session, bool fromSaveData) {
            checkForceEnableVariants(session);

            orig(session, fromSaveData);
        }

        // this one is for breaking in a level directly (typically due to a console load command)
        // this is a fallback, and since we don't go through the level enter routine, we can't show the postcard.
        private void checkForceEnableVariantsOnLevelLoad(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            if (triggerIsHooked && Engine.Scene is Level) {
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantsModule", "Loading level from level (console load?), running level exit hooks!");
                TriggerManager.ResetVariantsOnLevelExit();
                unhookTrigger();
            }

            if (!triggerIsHooked) {
                checkForceEnableVariants(session);
                showForcedVariantsPostcard = false;
            }

            orig(self, session, startPosition);
        }

        private void checkForceEnableVariants(Session session) {
            if (AreaData.Areas.Count > session.Area.ID && AreaData.Areas[session.Area.ID].Mode.Length > (int) session.Area.Mode
                && AreaData.Areas[session.Area.ID].Mode[(int) session.Area.Mode] != null
                && session.MapData.Levels.Exists(levelData =>
                levelData.Triggers.Exists(entityData => extendedVariantsEntities.Contains(entityData.Name)) ||
                levelData.Entities.Exists(entityData => extendedVariantsEntities.Contains(entityData.Name)))) {

                // the level we're entering has an Extended Variant Trigger: load the trigger on-demand.
                hookTrigger();

                bool variantsWereDisabled = !Settings.MasterSwitch;

                // if variants are disabled, we want to enable them as well, with default values
                // (so that we don't get variants that were enabled long ago).
                if (!stuffIsHooked) {
                    showForcedVariantsPostcard = true;
                    Settings.MasterSwitch = true;
                    HookStuff();
                }

                // reset settings to be sure we match what the mapper wants, without anything the user enabled before playing their map.
                bool settingsChanged = false;
                if (variantsWereDisabled || Settings.AutomaticallyResetVariants) {
                    settingsChanged = ResetToDefaultSettings();
                    SaveSettings();
                }
                showForcedVariantsPostcard = showForcedVariantsPostcard || settingsChanged;
            }
        }

        private IEnumerator addForceEnabledVariantsPostcard(On.Celeste.LevelEnter.orig_Routine orig, LevelEnter self) {
            if (showForcedVariantsPostcard) {
                showForcedVariantsPostcard = false;

                // let's show a postcard to let the player know Extended Variants have been enabled.
                self.Add(forceEnabledVariantsPostcard = new Postcard(Dialog.Get("POSTCARD_EXTENDEDVARIANTS_FORCEENABLED"), "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out"));
                yield return forceEnabledVariantsPostcard.DisplayRoutine();
                forceEnabledVariantsPostcard = null;
            }

            // just go on with vanilla behavior (other postcards, B-side intro, etc)
            IEnumerator origEnum = orig(self);
            while (origEnum.MoveNext()) yield return origEnum.Current;
        }

        private void addForceEnabledVariantsPostcardRendering(On.Celeste.LevelEnter.orig_BeforeRender orig, LevelEnter self) {
            orig(self);

            if (forceEnabledVariantsPostcard != null) forceEnabledVariantsPostcard.BeforeRender();
        }

        private void checkForTriggerUnhooking(On.Celeste.LevelExit.orig_ctor orig, LevelExit self, LevelExit.Mode mode, Session session, HiresSnow snow) {
            orig(self, mode, session, snow);

            // SaveAndQuit => leaving
            // GiveUp => leaving
            // Restart => restarting
            // GoldenBerryRestart => restarting
            // Completed => leaving
            // CompletedInterlude => leaving
            // we want to unhook the trigger if and only if we are actually leaving the level.
            if (triggerIsHooked && mode != LevelExit.Mode.Restart && mode != LevelExit.Mode.GoldenBerryRestart) {
                // we want to get rid of the trigger now.
                unhookTrigger();
            }
        }

        public bool ResetToDefaultSettings() {
            if (Settings.RoomLighting != -1 && Engine.Scene.GetType() == typeof(Level)) {
                // currently in level, change lighting right away
                Level lvl = (Engine.Scene as Level);
                lvl.Lighting.Alpha = (lvl.DarkRoom ? lvl.Session.DarkRoomAlpha : lvl.BaseLightingAlpha + lvl.Session.LightingAlphaAdd);
            }

            if (Settings.LegacyDashSpeedBehavior) {
                // restore the "new" dash speed behavior
                Instance.VariantHandlers[Variant.DashSpeed].Unload();
                Instance.VariantHandlers[Variant.DashSpeed] = new DashSpeed();
                Instance.VariantHandlers[Variant.DashSpeed].Load();
            }

            bool settingChanged = false;

            // reset all proper variants to their default values
            foreach (AbstractExtendedVariant variant in VariantHandlers.Values) {
                if (variant.GetDefaultValue() != variant.GetValue()) {
                    settingChanged = true;
                }

                variant.SetValue(variant.GetDefaultValue());
            }

            if (Settings.ChaserCount != 1
                || Settings.AffectExistingChasers
                || Settings.HiccupStrength != 10
                || Settings.RefillJumpsOnDashRefill
                || Settings.AllowThrowingTheoOffscreen
                || Settings.AllowLeavingTheoBehind
                || Settings.SnowballDelay != 8
                || Settings.BadelineLag != 0
                || Settings.DelayBetweenBadelines != 4
                || Settings.RisingLavaSpeed != 10
                || Settings.OshiroCount != 1
                || Settings.ReverseOshiroCount != 0
                || Settings.DisableOshiroSlowdown
                || Settings.DisableSeekerSlowdown
                || Settings.BadelineAttackPattern != 0
                || Settings.ChangePatternsOfExistingBosses
                || Settings.FirstBadelineSpawnRandom
                || Settings.BadelineBossCount != 1
                || Settings.BadelineBossNodeCount != 1
                || Settings.LegacyDashSpeedBehavior
                || Settings.DisableSuperBoosts) {

                settingChanged = true;
            }

            // reset variant customization options as well
            Settings.ChaserCount = 1;
            Settings.AffectExistingChasers = false;
            Settings.HiccupStrength = 10;
            Settings.RefillJumpsOnDashRefill = false;
            Settings.AllowThrowingTheoOffscreen = false;
            Settings.AllowLeavingTheoBehind = false;
            Settings.SnowballDelay = 8;
            Settings.BadelineLag = 0;
            Settings.DelayBetweenBadelines = 4;
            Settings.RisingLavaSpeed = 10;
            Settings.OshiroCount = 1;
            Settings.ReverseOshiroCount = 0;
            Settings.DisableOshiroSlowdown = false;
            Settings.DisableSeekerSlowdown = false;
            Settings.BadelineAttackPattern = 0;
            Settings.ChangePatternsOfExistingBosses = false;
            Settings.FirstBadelineSpawnRandom = false;
            Settings.BadelineBossCount = 1;
            Settings.BadelineBossNodeCount = 1;
            Settings.LegacyDashSpeedBehavior = false;
            Settings.DisableSuperBoosts = false;

            if (settingChanged && SaveData.Instance != null) {
                Randomizer.RefreshEnabledVariantsDisplayList();
            }

            return settingChanged;
        }

        public static void ResetVanillaVariants() {
            // from SaveData.AssistModeChecks() when both assist and variant mode are disabled
            SaveData.Instance.Assists = default(Assists);
            SaveData.Instance.Assists.GameSpeed = 10;

            // apply the Other Self variant right now.
            Player p = Engine.Scene.Tracker.GetEntity<Player>();
            if (p != null) {
                PlayerSpriteMode mode = p.DefaultSpriteMode;
                if (p.Active) {
                    p.ResetSpriteNextFrame(mode);
                } else {
                    p.ResetSprite(mode);
                }
            }

            if (SaveData.Instance != null) {
                Instance.Randomizer.RefreshEnabledVariantsDisplayList();
            }
        }

        // ==================== Reset Variants commands =====================

        [Command("reset_vanilla_variants", "[from Extended Variant Mode] resets vanilla variants to their default values")]
        public static void CmdResetVanillaVariants() {
            if (SaveData.Instance == null) {
                Engine.Commands.Log("This command only works when a save is loaded!");
            } else {
                ResetVanillaVariants();
            }
        }

        [Command("reset_extended_variants", "[from Extended Variant Mode] resets extended variants to their default values")]
        public static void CmdResetExtendedVariants() {
            Instance.ResetToDefaultSettings();
        }

        // ================ Stamp on Chapter Complete screen ================

        private void onLevelEnd(On.Celeste.Level.orig_End orig, Level self) {
            isLevelEnding = true;
            orig(self);
            isLevelEnding = false;
        }

        private void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            checkForUsedVariants();
        }

        private void onUnpause(On.Celeste.Level.orig_EndPauseEffects orig, Level self) {
            orig(self);

            if (!isLevelEnding) {
                checkForUsedVariants();
            }
        }

        private void checkForUsedVariants() {
            if (!Session.ExtendedVariantsWereUsed) {
                // check if extended variants are used.
                foreach (Variant variant in Enum.GetValues(typeof(Variant))) {
                    if (variant == Variant.MadelineIsSilhouette && !MaxHelpingHandInstalled && !SpringCollab2020Installed) {
                        // this variant cannot be enabled, because it does not exist.
                        continue;
                    }
                    int expectedValue = TriggerManager.GetExpectedVariantValue(variant);
                    int actualValue = TriggerManager.GetCurrentVariantValue(variant);
                    if (expectedValue != actualValue) {
                        Logger.Log(LogLevel.Warn, "ExtendedVariantTrigger/ExtendedVariantsModule", $"/!\\ Variants have been used! {variant} is {actualValue} instead of {expectedValue}. Tagging session as dirty!");
                        Session.ExtendedVariantsWereUsed = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Wraps the VersionNumberAndVariants in the base game in order to add the Variant Mode logo if Extended Variants are enabled.
        /// </summary>
        private void modVersionNumberAndVariants(On.Celeste.AreaComplete.orig_VersionNumberAndVariants orig, string version, float ease, float alpha) {
            if (Session.ExtendedVariantsWereUsed) {
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

        private void ilModVersionNumberAndVariants(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("cs_variantmode"))) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Injecting method to mod Variant Mode logo at {cursor.Index} in IL for AreaComplete.orig_VersionNumberAndVariants");
                cursor.EmitDelegate<Func<string, string>>(modVariantModeLogo);
            }
        }

        private string modVariantModeLogo(string orig) {
            if (Session.ExtendedVariantsWereUsed) {
                return "ExtendedVariantMode/complete_screen_stamp";
            }
            return orig;
        }

        // ================ Common methods for multiple variants ================

        private static bool badelineBoosting = false;
        private static bool prologueEndingCutscene = false;

        public static bool ShouldIgnoreCustomDelaySettings() {
            if (Engine.Scene.GetType() == typeof(Level)) {
                Player player = (Engine.Scene as Level).Tracker.GetEntity<Player>();
                // those states are "Intro Walk", "Intro Jump" and "Intro Wake Up". Getting killed during such an intro is annoying but can also **crash the game**
                if (player != null && (player.StateMachine.State == 12 || player.StateMachine.State == 13 || player.StateMachine.State == 15)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Generates a new EntityData instance, linked to the level given, an ID which will be the same if and only if generated
        /// in the same room with the same entityNumber, and an empty map of attributes.
        /// </summary>
        /// <param name="level">The level the entity belongs to</param>
        /// <param name="entityNumber">An entity number, between 0 and 19 inclusive</param>
        /// <returns>A fresh EntityData linked to the level, and with an ID</returns>
        public static EntityData GenerateBasicEntityData(Level level, int entityNumber) {
            EntityData entityData = new EntityData();

            // we hash the current level name, so we will get a hopefully-unique "room hash" for each room in the level
            // the resulting hash should be between 0 and 49_999_999 inclusive
            int roomHash = Math.Abs(level.Session.Level.GetHashCode()) % 50_000_000;

            // generate an ID, minimum 1_000_000_000 (to minimize chances of conflicting with existing entities)
            // and maximum 1_999_999_999 inclusive (1_000_000_000 + 49_999_999 * 20 + 19) => max value for int32 is 2_147_483_647
            // => if the same entity (same entityNumber) is generated in the same room, it will have the same ID, like any other entity would
            entityData.ID = 1_000_000_000 + roomHash * 20 + entityNumber;

            entityData.Level = level.Session.LevelData;
            entityData.Values = new Dictionary<string, object>();

            return entityData;
        }

        public static bool ShouldEntitiesAutoDestroy(Player player) {
            return (player != null && (player.StateMachine.State == 10 || player.StateMachine.State == 11) && !badelineBoosting)
                || prologueEndingCutscene // this kills Oshiro, that prevents the Prologue ending cutscene from even triggering.
                || !Instance.stuffIsHooked; // this makes all the mess instant vanish when Extended Variants are disabled entirely.
        }

        private IEnumerator modBadelineBoostRoutine(On.Celeste.BadelineBoost.orig_BoostRoutine orig, BadelineBoost self, Player player) {
            badelineBoosting = true;
            IEnumerator coroutine = orig(self, player);
            while (coroutine.MoveNext()) {
                yield return coroutine.Current;
            }
            badelineBoosting = false;
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            onLevelExit();
        }

        private void onLevelExit() {
            badelineBoosting = false;
            prologueEndingCutscene = false;
        }

        private void onPrologueEndingCutsceneBegin(On.Celeste.CS00_Ending.orig_OnBegin orig, CS00_Ending self, Level level) {
            orig(self, level);

            prologueEndingCutscene = true;
        }
    }
}
