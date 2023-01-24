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
using System.Linq;
using ExtendedVariants.Variants.Vanilla;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsModule : EverestModule {

        public static ExtendedVariantsModule Instance;

        public bool DJMapHelperInstalled { get; private set; }
        public bool SpringCollab2020Installed { get; private set; }
        public bool MaxHelpingHandInstalled { get; private set; }
        public bool JungleHelperInstalled { get; private set; }
        public bool XaphanHelperInstalled { get; private set; }

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
            AffectExistingChasers, BadelineBossesEverywhere, BadelineAttackPattern, ChangePatternsOfExistingBosses, FirstBadelineSpawnRandom, LegacyDashSpeedBehavior,
            BadelineBossCount, BadelineBossNodeCount, BadelineLag, DelayBetweenBadelines, OshiroEverywhere, OshiroCount, ReverseOshiroCount, DisableOshiroSlowdown,
            WindEverywhere, SnowballsEverywhere, SnowballDelay, AddSeekers, DisableSeekerSlowdown, TheoCrystalsEverywhere, AllowThrowingTheoOffscreen, AllowLeavingTheoBehind,
            Stamina, UpsideDown, DisableNeutralJumping, RegularHiccups, HiccupStrength, RoomLighting, RoomBloom, GlitchEffect, EverythingIsUnderwater, ForceDuckOnGround,
            InvertDashes, InvertGrab, AllStrawberriesAreGoldens, GameSpeed, ColorGrading, JellyfishEverywhere, RisingLavaEverywhere, RisingLavaSpeed, InvertHorizontalControls,
            BounceEverywhere, SuperdashSteeringSpeed, ScreenShakeIntensity, AnxietyEffect, BlurLevel, ZoomLevel, DashDirection, BackgroundBrightness, DisableMadelineSpotlight,
            ForegroundEffectOpacity, MadelineIsSilhouette, DashTrailAllTheTime, DisableClimbingUpOrDown, SwimmingSpeed, BoostMultiplier, FriendlyBadelineFollower,
            DisableRefillsOnScreenTransition, RestoreDashesOnRespawn, DisableSuperBoosts, DisplayDashCount, MadelineHasPonytail, MadelineBackpackMode, InvertVerticalControls,
            DontRefillStaminaOnGround, EveryJumpIsUltra, CoyoteTime, BackgroundBlurLevel, NoFreezeFrames, PreserveExtraDashesUnderwater, AlwaysInvisible, DisplaySpeedometer,
            WallSlidingSpeed, DisableJumpingOutOfWater, DisableDashCooldown, DisableKeysSpotlight, JungleSpidersEverywhere, CornerCorrection, PickupDuration,
            MinimumDelayBeforeThrowing, DelayBeforeRegrabbing, DashTimerMultiplier, JumpDuration, HorizontalSpringBounceDuration, HorizontalWallJumpDuration,
            ResetJumpCountOnGround,

            // vanilla variants
            AirDashes, DashAssist, VanillaGameSpeed, Hiccups, InfiniteStamina, Invincible, InvisibleMotion, LowFriction, MirrorMode, NoGrabbing, PlayAsBadeline,
            SuperDashing, ThreeSixtyDashing
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
            VariantHandlers[Variant.JumpDuration] = new JumpDuration();
            VariantHandlers[Variant.SpeedX] = new SpeedX();
            VariantHandlers[Variant.Stamina] = new Stamina();
            VariantHandlers[Variant.DashSpeed] = new DashSpeed();
            VariantHandlers[Variant.DashCount] = (dashCount = new DashCount());
            VariantHandlers[Variant.CornerCorrection] = new CornerCorrection();
            VariantHandlers[Variant.DisableDashCooldown] = new DisableDashCooldown();
            VariantHandlers[Variant.HeldDash] = new HeldDash();
            VariantHandlers[Variant.Friction] = new Friction();
            VariantHandlers[Variant.AirFriction] = new AirFriction();
            VariantHandlers[Variant.DisableWallJumping] = new DisableWallJumping();
            VariantHandlers[Variant.DisableClimbJumping] = new DisableClimbJumping();
            VariantHandlers[Variant.DisableJumpingOutOfWater] = new DisableJumpingOutOfWater();
            VariantHandlers[Variant.JumpCount] = new JumpCount(dashCount);
            VariantHandlers[Variant.ZoomLevel] = (zoomLevel = new ZoomLevel());
            VariantHandlers[Variant.UpsideDown] = new UpsideDown(zoomLevel);
            VariantHandlers[Variant.HyperdashSpeed] = new HyperdashSpeed();
            VariantHandlers[Variant.ExplodeLaunchSpeed] = new ExplodeLaunchSpeed();
            VariantHandlers[Variant.HorizontalSpringBounceDuration] = new HorizontalSpringBounceDuration();
            VariantHandlers[Variant.HorizontalWallJumpDuration] = new HorizontalWallJumpDuration();
            VariantHandlers[Variant.DisableSuperBoosts] = new DisableSuperBoosts();
            VariantHandlers[Variant.WallBouncingSpeed] = new WallbouncingSpeed();
            VariantHandlers[Variant.WallSlidingSpeed] = new WallSlidingSpeed();
            VariantHandlers[Variant.DashLength] = new DashLength();
            VariantHandlers[Variant.DashTimerMultiplier] = new DashTimerMultiplier();
            VariantHandlers[Variant.ForceDuckOnGround] = new ForceDuckOnGround();
            VariantHandlers[Variant.InvertDashes] = new InvertDashes();
            VariantHandlers[Variant.InvertGrab] = new InvertGrab();
            VariantHandlers[Variant.DisableNeutralJumping] = new DisableNeutralJumping();
            VariantHandlers[Variant.BadelineChasersEverywhere] = new BadelineChasersEverywhere();
            VariantHandlers[Variant.ChaserCount] = new ChaserCount();
            VariantHandlers[Variant.AffectExistingChasers] = new AffectExistingChasers();
            VariantHandlers[Variant.BadelineBossesEverywhere] = new BadelineBossesEverywhere();
            VariantHandlers[Variant.BadelineAttackPattern] = new BadelineAttackPattern();
            VariantHandlers[Variant.ChangePatternsOfExistingBosses] = new ChangePatternsOfExistingBosses();
            VariantHandlers[Variant.FirstBadelineSpawnRandom] = new FirstBadelineSpawnRandom();
            VariantHandlers[Variant.BadelineBossCount] = new BadelineBossCount();
            VariantHandlers[Variant.BadelineBossNodeCount] = new BadelineBossNodeCount();
            VariantHandlers[Variant.RegularHiccups] = new RegularHiccups();
            VariantHandlers[Variant.HiccupStrength] = new HiccupStrength();
            VariantHandlers[Variant.RefillJumpsOnDashRefill] = new RefillJumpsOnDashRefill();
            VariantHandlers[Variant.ResetJumpCountOnGround] = new ResetJumpCountOnGround();
            VariantHandlers[Variant.RoomLighting] = new RoomLighting();
            VariantHandlers[Variant.RoomBloom] = new RoomBloom();
            VariantHandlers[Variant.GlitchEffect] = new GlitchEffect();
            VariantHandlers[Variant.AnxietyEffect] = new AnxietyEffect();
            VariantHandlers[Variant.BlurLevel] = new BlurLevel();
            VariantHandlers[Variant.BackgroundBlurLevel] = new BackgroundBlurLevel();
            VariantHandlers[Variant.EverythingIsUnderwater] = new EverythingIsUnderwater();
            VariantHandlers[Variant.OshiroEverywhere] = new OshiroEverywhere();
            VariantHandlers[Variant.OshiroCount] = new OshiroCount();
            // ReverseOshiroCount is instanciated in Initialize
            VariantHandlers[Variant.DisableOshiroSlowdown] = new DisableOshiroSlowdown();
            VariantHandlers[Variant.WindEverywhere] = new WindEverywhere();
            VariantHandlers[Variant.SnowballsEverywhere] = new SnowballsEverywhere();
            VariantHandlers[Variant.SnowballDelay] = new SnowballDelay();
            VariantHandlers[Variant.AddSeekers] = new AddSeekers();
            VariantHandlers[Variant.DisableSeekerSlowdown] = new DisableSeekerSlowdown();
            VariantHandlers[Variant.TheoCrystalsEverywhere] = new TheoCrystalsEverywhere();
            VariantHandlers[Variant.AllowThrowingTheoOffscreen] = new AllowThrowingTheoOffscreen();
            VariantHandlers[Variant.AllowLeavingTheoBehind] = new AllowLeavingTheoBehind();
            VariantHandlers[Variant.RisingLavaEverywhere] = new RisingLavaEverywhere();
            VariantHandlers[Variant.RisingLavaSpeed] = new RisingLavaSpeed();
            VariantHandlers[Variant.BadelineLag] = new BadelineLag();
            VariantHandlers[Variant.DelayBetweenBadelines] = new DelayBetweenBadelines();
            VariantHandlers[Variant.AllStrawberriesAreGoldens] = new AllStrawberriesAreGoldens();
            VariantHandlers[Variant.DontRefillDashOnGround] = new DontRefillDashOnGround();
            VariantHandlers[Variant.DontRefillStaminaOnGround] = new DontRefillStaminaOnGround();
            VariantHandlers[Variant.GameSpeed] = new Variants.GameSpeed();
            VariantHandlers[Variant.ColorGrading] = new ColorGrading();
            VariantHandlers[Variant.JellyfishEverywhere] = new JellyfishEverywhere();
            VariantHandlers[Variant.InvertHorizontalControls] = new InvertHorizontalControls();
            VariantHandlers[Variant.InvertVerticalControls] = new InvertVerticalControls();
            VariantHandlers[Variant.BounceEverywhere] = new BounceEverywhere();
            // JungleSpidersEverywhere is instanciated in Initialize
            VariantHandlers[Variant.SuperdashSteeringSpeed] = new SuperdashSteeringSpeed();
            VariantHandlers[Variant.ScreenShakeIntensity] = new ScreenShakeIntensity();
            VariantHandlers[Variant.DashDirection] = new DashDirection();
            VariantHandlers[Variant.BackgroundBrightness] = new BackgroundBrightness();
            VariantHandlers[Variant.DisableMadelineSpotlight] = new DisableMadelineSpotlight();
            VariantHandlers[Variant.DisableKeysSpotlight] = new DisableKeysSpotlight();
            VariantHandlers[Variant.ForegroundEffectOpacity] = new ForegroundEffectOpacity();
            // MadelineIsSilhouette is instanciated in Initialize
            // MadelineHasPonytail is instanciated in Initialize
            VariantHandlers[Variant.DashTrailAllTheTime] = new DashTrailAllTheTime();
            VariantHandlers[Variant.DisableClimbingUpOrDown] = new DisableClimbingUpOrDown();
            VariantHandlers[Variant.PickupDuration] = new PickupDuration();
            VariantHandlers[Variant.MinimumDelayBeforeThrowing] = new MinimumDelayBeforeThrowing();
            VariantHandlers[Variant.DelayBeforeRegrabbing] = new DelayBeforeRegrabbing();
            VariantHandlers[Variant.SwimmingSpeed] = new SwimmingSpeed();
            VariantHandlers[Variant.BoostMultiplier] = new BoostMultiplier();
            VariantHandlers[Variant.FriendlyBadelineFollower] = new FriendlyBadelineFollower();
            VariantHandlers[Variant.DisableRefillsOnScreenTransition] = new DisableRefillsOnScreenTransition();
            VariantHandlers[Variant.RestoreDashesOnRespawn] = new RestoreDashesOnRespawn();
            VariantHandlers[Variant.DisplayDashCount] = new DisplayDashCount();
            VariantHandlers[Variant.MadelineBackpackMode] = new MadelineBackpackMode();
            VariantHandlers[Variant.EveryJumpIsUltra] = new EveryJumpIsUltra();
            VariantHandlers[Variant.CoyoteTime] = new CoyoteTime();
            VariantHandlers[Variant.NoFreezeFrames] = new NoFreezeFrames();
            VariantHandlers[Variant.PreserveExtraDashesUnderwater] = new PreserveExtraDashesUnderwater();
            VariantHandlers[Variant.AlwaysInvisible] = new AlwaysInvisible();
            VariantHandlers[Variant.DisplaySpeedometer] = new DisplaySpeedometer();
            VariantHandlers[Variant.LegacyDashSpeedBehavior] = new LegacyDashSpeedBehavior();

            // vanilla variants
            VariantHandlers[Variant.AirDashes] = new AirDashes();
            VariantHandlers[Variant.DashAssist] = new DashAssist();
            VariantHandlers[Variant.VanillaGameSpeed] = new Variants.Vanilla.GameSpeed();
            VariantHandlers[Variant.Hiccups] = new Hiccups();
            VariantHandlers[Variant.InfiniteStamina] = new InfiniteStamina();
            VariantHandlers[Variant.Invincible] = new Invincible();
            VariantHandlers[Variant.InvisibleMotion] = new InvisibleMotion();
            VariantHandlers[Variant.LowFriction] = new LowFriction();
            VariantHandlers[Variant.MirrorMode] = new MirrorMode();
            VariantHandlers[Variant.NoGrabbing] = new NoGrabbing();
            VariantHandlers[Variant.PlayAsBadeline] = new PlayAsBadeline();
            VariantHandlers[Variant.SuperDashing] = new SuperDashing();
            VariantHandlers[Variant.ThreeSixtyDashing] = new ThreeSixtyDashing();
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
            JumpIndicator.Initialize();
            (VariantHandlers[Variant.ExplodeLaunchSpeed] as ExplodeLaunchSpeed).Initialize();

            DJMapHelperInstalled = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "DJMapHelper", Version = new Version(1, 8, 35) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"DJ Map Helper installed = {DJMapHelperInstalled}");
            SpringCollab2020Installed = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "SpringCollab2020", Version = new Version(1, 0, 0) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Spring Collab 2020 installed = {SpringCollab2020Installed}");
            MaxHelpingHandInstalled = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "MaxHelpingHand", Version = new Version(1, 17, 3) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Max Helping Hand installed = {MaxHelpingHandInstalled}");
            JungleHelperInstalled = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "JungleHelper", Version = new Version(1, 1, 2) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Jungle Helper installed = {JungleHelperInstalled}");
            XaphanHelperInstalled = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "XaphanHelper", Version = new Version(1, 0, 51) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Xaphan Helper installed = {XaphanHelperInstalled}");

            UpsideDown.Initialize();

            if (DJMapHelperInstalled) {
                // let's add this variant in now.
                VariantHandlers[Variant.ReverseOshiroCount] = new ReverseOshiroCount();
            } else {
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

            if (!MaxHelpingHandInstalled) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Force-disabling Madeline Has Ponytail");
                Settings.MadelineHasPonytail = false;
                SaveSettings();
            } else {
                // let's add this variant in now.
                VariantHandlers[Variant.MadelineHasPonytail] = new MadelineHasPonytail();

                if (stuffIsHooked) {
                    // and activate it if all others are already active!
                    VariantHandlers[Variant.MadelineHasPonytail].Load();
                }
            }

            if (!JungleHelperInstalled) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Force-disabling Jungle Spiders Everywhere");
                Settings.JungleSpidersEverywhere = JungleSpidersEverywhere.SpiderType.Disabled;
                SaveSettings();
            } else {
                // let's add this variant in now.
                VariantHandlers[Variant.JungleSpidersEverywhere] = new JungleSpidersEverywhere();

                if (stuffIsHooked) {
                    // and activate it if all others are already active!
                    VariantHandlers[Variant.JungleSpidersEverywhere].Load();
                }
            }

            if (Settings.AutoResetVariantsOnFirstStartup) {
                // this means the settings were never saved in version 0.22.2+ and are potentially in the older format: reset them!
                // we will then set ShouldAutoResetVariants to false to record we did that, and won't do it ever again.
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantsModule", "Automatically resetting settings to default!");

                ResetVariantsToDefaultSettings(isVanilla: false);
                Settings.AutoResetVariantsOnFirstStartup = false;
            }
        }

        public override void PrepareMapDataProcessors(MapDataFixup context) {
            base.PrepareMapDataProcessors(context);

            context.Add<ExtendedVariantsMapDataProcessor>();
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
            Entities.Legacy.ExtendedVariantTrigger.Load();
            Entities.ForMappers.AbstractExtendedVariantTriggerTeleportHandler.Load();

            On.Celeste.LevelEnter.Routine += addForceEnabledVariantsPostcard;
            On.Celeste.LevelEnter.BeforeRender += addForceEnabledVariantsPostcardRendering;

            Logger.Log(LogLevel.Info, "ExtendedVariantMode/ExtendedVariantsModule", $"Done loading variant trigger manager.");

            triggerIsHooked = true;
        }

        private void unhookTrigger() {
            if (!triggerIsHooked) return;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant trigger manager...");
            TriggerManager.Unload();
            Entities.Legacy.ExtendedVariantTrigger.Unload();

            On.Celeste.LevelEnter.Routine -= addForceEnabledVariantsPostcard;
            On.Celeste.LevelEnter.BeforeRender -= addForceEnabledVariantsPostcardRendering;

            Logger.Log(LogLevel.Info, "ExtendedVariantMode/ExtendedVariantsModule", $"Done unloading variant trigger manager.");

            triggerIsHooked = false;
        }

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

        private bool isExtendedVariantEntity(string name) {
            return name == "ExtendedVariantTrigger" || name.StartsWith("ExtendedVariantMode/");
        }

        private void checkForceEnableVariants(Session session) {
            if (AreaData.Areas.Count > session.Area.ID && AreaData.Areas[session.Area.ID].Mode.Length > (int) session.Area.Mode
                && AreaData.Areas[session.Area.ID].Mode[(int) session.Area.Mode] != null
                && session.MapData.Levels.Exists(levelData =>
                levelData.Triggers.Exists(entityData => isExtendedVariantEntity(entityData.Name)) ||
                levelData.Entities.Exists(entityData => isExtendedVariantEntity(entityData.Name)))) {

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
                    settingsChanged = ResetVariantsToDefaultSettings(isVanilla: false);
                    settingsChanged = ResetVariantsToDefaultSettings(isVanilla: true) || settingsChanged;
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
            yield return new SwapImmediately(orig(self));
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

        /// <summary>
        /// Resets both vanilla and extended variants to default settings.
        /// </summary>
        /// <returns>true if any variant changed, false otherwise.</returns>
        public bool ResetToDefaultSettings() {
            bool vanillaVariantReset = ResetVariantsToDefaultSettings(isVanilla: true);
            bool extendedVariantReset = ResetVariantsToDefaultSettings(isVanilla: false);
            return vanillaVariantReset || extendedVariantReset;
        }

        /// <summary>
        /// Resets either vanilla or extended variants to default settings.
        /// </summary>
        /// <param name="isVanilla">Whether to reset vanilla variants (true) or extended variants (false)</param>
        /// <returns>true if any variant changed, false otherwise.</returns>
        public bool ResetVariantsToDefaultSettings(bool isVanilla) {
            bool settingChanged = false;

            // reset all proper variants to their default values
            foreach (AbstractExtendedVariant variant in VariantHandlers.Values.ToList()) {
                if (variant.IsVanilla() == isVanilla) {
                    if (variant.GetType() == typeof(DashDirection)) {
                        if (ModOptionsEntries.GetDashDirectionIndex() != 0) {
                            settingChanged = true;
                        }
                    } else {
                        if (!variant.GetDefaultVariantValue().Equals(variant.GetVariantValue())) {
                            settingChanged = true;
                        }
                    }

                    variant.SetVariantValue(variant.GetDefaultVariantValue());
                }
            }

            if (settingChanged && SaveData.Instance != null) {
                Randomizer.RefreshEnabledVariantsDisplayList();
            }

            return settingChanged;
        }

        // ==================== Reset Variants commands =====================

        [Command("reset_vanilla_variants", "[from Extended Variant Mode] resets vanilla variants to their default values")]
        public static void CmdResetVanillaVariants() {
            if (SaveData.Instance == null) {
                Engine.Commands.Log("This command only works when a save is loaded!");
            } else {
                Instance.ResetVariantsToDefaultSettings(isVanilla: true);
            }
        }

        [Command("reset_extended_variants", "[from Extended Variant Mode] resets extended variants to their default values")]
        public static void CmdResetExtendedVariants() {
            Instance.ResetVariantsToDefaultSettings(isVanilla: false);
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
                    if (variant == Variant.MadelineHasPonytail && !MaxHelpingHandInstalled) {
                        // this variant cannot be enabled, because it does not exist.
                        continue;
                    }
                    if (variant == Variant.ReverseOshiroCount && !DJMapHelperInstalled) {
                        // this variant cannot be enabled, because it does not exist.
                        continue;
                    }
                    if (variant == Variant.JungleSpidersEverywhere && !JungleHelperInstalled) {
                        // this variant cannot be enabled, because it does not exist.
                        continue;
                    }
                    if (VariantHandlers[variant].IsVanilla()) {
                        // do not take vanilla variants into account for showing the extended variants icon.
                        continue;
                    }

                    object expectedValue = TriggerManager.GetExpectedVariantValue(variant);
                    object actualValue = TriggerManager.GetCurrentVariantValue(variant);

                    if (variant == Variant.DashDirection) {
                        bool equal = true;
                        for (int i = 0; i < 3; i++) {
                            for (int j = 0; j < 3; j++) {
                                if (((bool[][]) expectedValue)[i][j] != ((bool[][]) actualValue)[i][j]) {
                                    equal = false;
                                }
                            }
                        }

                        if (!equal) {
                            Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantsModule", $"/!\\ Variants have been used! {variant} is {actualValue} instead of {expectedValue}. Tagging session as dirty!");
                            Session.ExtendedVariantsWereUsed = true;
                            break;
                        }
                    } else {
                        if (!expectedValue.Equals(actualValue)) {
                            Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantsModule", $"/!\\ Variants have been used! {variant} is {actualValue} instead of {expectedValue}. Tagging session as dirty!");
                            Session.ExtendedVariantsWereUsed = true;
                            break;
                        }
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

        // ================ Fix types for deserialized sessions ================

        // when you save an array of objects with YamlDotNet and load them back...
        // they all turn into strings. aaaaaaaaaaaaaaaa

        public override byte[] SerializeSession(int index) {
            foreach (Variant v in Session.VariantsEnabledViaTrigger.Keys.ToList()) {
                object value = Session.VariantsEnabledViaTrigger[v];

                switch (value) {
                    case string castValue:
                        Session.VariantsEnabledViaTrigger[v] = "string:" + castValue;
                        break;
                    case int castValue:
                        Session.VariantsEnabledViaTrigger[v] = "int:" + castValue;
                        break;
                    case float castValue:
                        Session.VariantsEnabledViaTrigger[v] = "float:" + castValue;
                        break;
                    case bool castValue:
                        Session.VariantsEnabledViaTrigger[v] = "bool:" + castValue;
                        break;
                    case bool[][] castValue:
                        Session.VariantsEnabledViaTrigger[v] = "DashDirection:" +
                            castValue[0][0] + "," + castValue[0][1] + "," + castValue[0][2] + "," +
                            castValue[1][0] + "," + castValue[1][1] + "," + castValue[1][2] + "," +
                            castValue[2][0] + "," + castValue[2][1] + "," + castValue[2][2];
                        break;
                    case DisplaySpeedometer.SpeedometerConfiguration castValue:
                        Session.VariantsEnabledViaTrigger[v] = "DisplaySpeedometer:" + castValue;
                        break;
                    case DontRefillDashOnGround.DashRefillOnGroundConfiguration castValue:
                        Session.VariantsEnabledViaTrigger[v] = "DontRefillDashOnGround:" + castValue;
                        break;
                    case MadelineBackpackMode.MadelineBackpackModes castValue:
                        Session.VariantsEnabledViaTrigger[v] = "MadelineBackpackMode:" + castValue;
                        break;
                    case WindEverywhere.WindPattern castValue:
                        Session.VariantsEnabledViaTrigger[v] = "WindEverywhere:" + castValue;
                        break;
                    case JungleSpidersEverywhere.SpiderType castValue:
                        Session.VariantsEnabledViaTrigger[v] = "JungleSpidersEverywhere:" + castValue;
                        break;
                    case Assists.DashModes castValue:
                        Session.VariantsEnabledViaTrigger[v] = "DashModes:" + castValue;
                        break;
                    case DisableClimbingUpOrDown.ClimbUpOrDownOptions castValue:
                        Session.VariantsEnabledViaTrigger[v] = "DisableClimbingUpOrDown:" + castValue;
                        break;
                    default:
                        Logger.Log(LogLevel.Error, "ExtendedVariantMode/ExtendedVariantModule", "Cannot serialize value of type " + value.GetType() + "!");
                        break;
                }
            }

            byte[] result = base.SerializeSession(index);
            turnTypesIntoTheirRealCounterparts();

            return result;
        }

        public override void DeserializeSession(int index, byte[] data) {
            base.DeserializeSession(index, data);
            turnTypesIntoTheirRealCounterparts();
        }

        private void turnTypesIntoTheirRealCounterparts() {
            try {
                foreach (Variant v in Session.VariantsEnabledViaTrigger.Keys.ToList()) {
                    string value = Session.VariantsEnabledViaTrigger[v].ToString();

                    // handle sessions created by older Extended Variants versions (no prefix, and everything is an integer)
                    if (!value.Contains(":") && int.TryParse(value, out int legacyValue)) {
                        Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantsModule", "Encountered a legacy variant value in session: " + v + " = " + legacyValue);
                        Session.VariantsEnabledViaTrigger[v] = new ExtendedVariantTriggerManager.LegacyVariantValue(legacyValue);
                        continue;
                    }

                    string type = value.Substring(0, value.IndexOf(":"));
                    value = value.Substring(type.Length + 1);

                    switch (type) {
                        case "string":
                            Session.VariantsEnabledViaTrigger[v] = value;
                            break;
                        case "int":
                            Session.VariantsEnabledViaTrigger[v] = int.Parse(value);
                            break;
                        case "float":
                            Session.VariantsEnabledViaTrigger[v] = float.Parse(value);
                            break;
                        case "bool":
                            Session.VariantsEnabledViaTrigger[v] = bool.Parse(value);
                            break;
                        case "DashDirection":
                            string[] split = value.Split(',');
                            Session.VariantsEnabledViaTrigger[v] = new bool[][] {
                                new bool[] { bool.Parse(split[0]),bool.Parse(split[1]),bool.Parse(split[2]) },
                                new bool[] { bool.Parse(split[3]),bool.Parse(split[4]),bool.Parse(split[5]) },
                                new bool[] { bool.Parse(split[6]),bool.Parse(split[7]),bool.Parse(split[8]) }
                            };
                            break;
                        case "DisplaySpeedometer":
                            Session.VariantsEnabledViaTrigger[v] = Enum.Parse(typeof(DisplaySpeedometer.SpeedometerConfiguration), value);
                            break;
                        case "DontRefillDashOnGround":
                            Session.VariantsEnabledViaTrigger[v] = Enum.Parse(typeof(DontRefillDashOnGround.DashRefillOnGroundConfiguration), value);
                            break;
                        case "MadelineBackpackMode":
                            Session.VariantsEnabledViaTrigger[v] = Enum.Parse(typeof(MadelineBackpackMode.MadelineBackpackModes), value);
                            break;
                        case "WindEverywhere":
                            Session.VariantsEnabledViaTrigger[v] = Enum.Parse(typeof(WindEverywhere.WindPattern), value);
                            break;
                        case "JungleSpidersEverywhere":
                            Session.VariantsEnabledViaTrigger[v] = Enum.Parse(typeof(JungleSpidersEverywhere.SpiderType), value);
                            break;
                        case "DashModes":
                            Session.VariantsEnabledViaTrigger[v] = Enum.Parse(typeof(Assists.DashModes), value);
                            break;
                        case "DisableClimbingUpOrDown":
                            Session.VariantsEnabledViaTrigger[v] = Enum.Parse(typeof(DisableClimbingUpOrDown.ClimbUpOrDownOptions), value);
                            break;
                        default:
                            throw new NotImplementedException("Cannot deserialize value of type " + type + "!");
                    }
                }
            } catch (Exception e) {
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantModule", $"Failed to parse a value in session!");
                Logger.LogDetailed(e);
            }
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
            yield return new SwapImmediately(orig(self, player));
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
