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
using MonoMod.ModInterop;
using System.IO;
using Celeste.Mod.Helpers;
using Celeste.Mod.Core;
using System.Reflection;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsModule : EverestModule {

        public static ExtendedVariantsModule Instance;

        public bool DJMapHelperInstalled { get; private set; }
        public bool SpringCollab2020Installed { get; private set; }
        public bool MaxHelpingHandInstalled { get; private set; }
        public bool JungleHelperInstalled { get; private set; }

        private bool stuffIsHooked = false;
        private bool forceEnabled = false;
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
            ResetJumpCountOnGround, UltraSpeedMultiplier, JumpCooldown, SpinnerColor, WallJumpDistance, WallBounceDistance, DashRestriction, CorrectedMirrorMode,
            FastFallAcceleration, AlwaysFeather, PermanentDashAttack, PermanentBinoStorage, WalllessWallbounce, TrueNoGrabbing, BufferableGrab, UltraProtection, LiftboostProtection,
            CornerboostProtection, CrouchDashFix, AlternativeBuffering, MultiBuffering, SaferDiagonalSmuggle, DashBeforePickup, ThrowIgnoresForcedMove, MidairTech,
            NoFreezeFramesAdvanceCassetteBlocks, PreserveWallbounceSpeed, StretchUpDashes,

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
            JumpCooldown jumpCooldown;
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
            VariantHandlers[Variant.JumpCooldown] = (jumpCooldown = new JumpCooldown());
            VariantHandlers[Variant.JumpCount] = new JumpCount(dashCount, jumpCooldown);
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
            VariantHandlers[Variant.DashRestriction] = new DashRestriction();
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
            VariantHandlers[Variant.UltraSpeedMultiplier] = new UltraSpeedMultiplier();
            VariantHandlers[Variant.CoyoteTime] = new CoyoteTime();
            VariantHandlers[Variant.NoFreezeFrames] = new NoFreezeFrames();
            VariantHandlers[Variant.NoFreezeFramesAdvanceCassetteBlocks] = new NoFreezeFramesAdvanceCassetteBlocks();
            VariantHandlers[Variant.PreserveExtraDashesUnderwater] = new PreserveExtraDashesUnderwater();
            VariantHandlers[Variant.AlwaysInvisible] = new AlwaysInvisible();
            VariantHandlers[Variant.DisplaySpeedometer] = new DisplaySpeedometer();
            VariantHandlers[Variant.LegacyDashSpeedBehavior] = new LegacyDashSpeedBehavior();
            VariantHandlers[Variant.SpinnerColor] = new SpinnerColor();
            VariantHandlers[Variant.WallJumpDistance] = new WallJumpDistance();
            VariantHandlers[Variant.WallBounceDistance] = new WallBounceDistance();
            VariantHandlers[Variant.CorrectedMirrorMode] = new CorrectedMirrorMode();
            VariantHandlers[Variant.FastFallAcceleration] = new FastFallAcceleration();
            VariantHandlers[Variant.AlwaysFeather] = new AlwaysFeather();
            VariantHandlers[Variant.PermanentDashAttack] = new PermanentDashAttack();
            VariantHandlers[Variant.PermanentBinoStorage] = new PermanentBinoStorage();
            VariantHandlers[Variant.WalllessWallbounce] = new WalllessWallbounce();
            VariantHandlers[Variant.TrueNoGrabbing] = new TrueNoGrabbing();
            VariantHandlers[Variant.BufferableGrab] = new BufferableGrab();
            VariantHandlers[Variant.UltraProtection] = new UltraProtection();
            VariantHandlers[Variant.LiftboostProtection] = new LiftboostProtection();
            VariantHandlers[Variant.CornerboostProtection] = new CornerboostProtection();
            VariantHandlers[Variant.CrouchDashFix] = new CrouchDashFix();
            VariantHandlers[Variant.AlternativeBuffering] = new AlternativeBuffering();
            VariantHandlers[Variant.MultiBuffering] = new MultiBuffering();
            VariantHandlers[Variant.SaferDiagonalSmuggle] = new SaferDiagonalSmuggle();
            VariantHandlers[Variant.DashBeforePickup] = new DashBeforePickup();
            VariantHandlers[Variant.ThrowIgnoresForcedMove] = new ThrowIgnoresForcedMove();
            VariantHandlers[Variant.MidairTech] = new MidairTech();
            VariantHandlers[Variant.PreserveWallbounceSpeed] = new PreserveWallbounceSpeed();
            VariantHandlers[Variant.StretchUpDashes] = new StretchUpDashes();

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

            if (inGame) {
                // build the menu with only the master switch
                new ModOptionsEntries().CreateAllOptions(ModOptionsEntries.VariantCategory.None, true, false, false,
                    null /* don't care, no submenu */, menu, inGame, forceEnabled);
            } else {
                // build the menu with the master switch + submenus + randomizer options
                new ModOptionsEntries().CreateAllOptions(ModOptionsEntries.VariantCategory.None, true, true, true,
                    () => OuiModOptions.Instance.Overworld.Goto<OuiModOptions>(), menu, inGame, forceEnabled);
            }
        }

        private void onCreatePauseMenuButtons(Level level, TextMenu menu, bool minimal) {
            int optionsIndex = menu.Items.FindIndex(item =>
                item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button) item).Label == Dialog.Clean("menu_pause_options"));

            // insert ourselves just before Options if required (this is below Variants if variant mode is enabled)
            menu.Insert(optionsIndex, AbstractSubmenu.BuildOpenMenuButton<OuiExtendedVariantsSubmenu>(menu, true,
                null /* this is not used when in-game anyway */, new object[] { true }));
        }

        // ================ Variants hooking / unhooking ================

        public override void Load() {
            Logger.SetLogLevel("ExtendedVariantMode", LogLevel.Info);

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "Initializing Extended Variant Mode");

            On.Celeste.LevelLoader.ctor += checkForceEnableVariants;

            typeof(LuaCutscenesUtils).ModInterop();

            if (Settings.MasterSwitch) {
                // variants are enabled: we want to hook them on startup.
                HookStuff();
            }
        }

        public override void Unload() {
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "Unloading Extended Variant Mode");

            On.Celeste.LevelLoader.ctor -= checkForceEnableVariants;

            if (stuffIsHooked) {
                unhookStuffRightNow();
            }
        }

        private static bool gameInitialized => (bool) typeof(Everest).GetField("_Initialized", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

        public override void Initialize() {
            base.Initialize();

            DashCountIndicator.Initialize();
            JumpIndicator.Initialize();
            (VariantHandlers[Variant.ExplodeLaunchSpeed] as ExplodeLaunchSpeed).Initialize();
            (VariantHandlers[Variant.SpinnerColor] as SpinnerColor).Initialize();

            DJMapHelperInstalled = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "DJMapHelper", Version = new Version(1, 8, 35) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"DJ Map Helper installed = {DJMapHelperInstalled}");
            SpringCollab2020Installed = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "SpringCollab2020", Version = new Version(1, 0, 0) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Spring Collab 2020 installed = {SpringCollab2020Installed}");
            MaxHelpingHandInstalled = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "MaxHelpingHand", Version = new Version(1, 17, 3) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Max Helping Hand installed = {MaxHelpingHandInstalled}");
            JungleHelperInstalled = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "JungleHelper", Version = new Version(1, 1, 2) });
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Jungle Helper installed = {JungleHelperInstalled}");

            if (Settings.MasterSwitch) {
                initializeStuff();
            }

            if (DJMapHelperInstalled) {
                // let's add this variant in now.
                VariantHandlers[Variant.ReverseOshiroCount] = new ReverseOshiroCount();
            } else {
                Settings.EnabledVariants.Remove(Variant.ReverseOshiroCount);
            }

            if (SpringCollab2020Installed || MaxHelpingHandInstalled) {
                // let's add this variant in now.
                VariantHandlers[Variant.MadelineIsSilhouette] = new MadelineIsSilhouette();

                if (stuffIsHooked) {
                    // and activate it if all others are already active!
                    VariantHandlers[Variant.MadelineIsSilhouette].Load();
                }
            } else {
                Settings.EnabledVariants.Remove(Variant.MadelineIsSilhouette);
            }

            if (MaxHelpingHandInstalled) {
                // let's add this variant in now.
                VariantHandlers[Variant.MadelineHasPonytail] = new MadelineHasPonytail();

                if (stuffIsHooked) {
                    // and activate it if all others are already active!
                    VariantHandlers[Variant.MadelineHasPonytail].Load();
                }
            } else {
                Settings.EnabledVariants.Remove(Variant.MadelineHasPonytail);
            }

            if (JungleHelperInstalled) {
                // let's add this variant in now.
                VariantHandlers[Variant.JungleSpidersEverywhere] = new JungleSpidersEverywhere();

                if (stuffIsHooked) {
                    // and activate it if all others are already active!
                    VariantHandlers[Variant.JungleSpidersEverywhere].Load();
                }
            } else {
                Settings.EnabledVariants.Remove(Variant.JungleSpidersEverywhere);
            }

            // filter out settings for variants that don't exist (optional dependencies that got disabled / uninstalled).
            foreach (Variant variantId in Settings.EnabledVariants.Keys.ToList()) {
                if (!VariantHandlers.ContainsKey(variantId)) {
                    Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantModule", $"Removed non-existing variant {variantId} from settings");
                }
            }
        }

        public override void PrepareMapDataProcessors(MapDataFixup context) {
            base.PrepareMapDataProcessors(context);

            context.Add<ExtendedVariantsMapDataProcessor>();
        }

        private ILHook hookOnVersionNumberAndVariants;

        // A simple component that allows us to hook/unhook all the variants outside of the Engine.Update method...
        // since some hooks hook Engine.Update.
        private class DoTheThingLater : GameComponent {
            public Action Thing { get; set; }

            public DoTheThingLater() : base(Celeste.Celeste.Instance) {
            }

            public override void Update(GameTime gameTime) {
                Thing();
                Celeste.Celeste.Instance.Components.Remove(this);
            }
        }

        public void HookStuff() {
            if (gameInitialized) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "We're not in game startup, deferring the hooking of all the things");
                Celeste.Celeste.Instance.Components.Add(new DoTheThingLater { Thing = hookStuffRightNow });
            } else {
                hookStuffRightNow();
            }
        }

        public void UnhookStuff() {
            if (gameInitialized) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "We're not in game startup, deferring the unhooking of all the things");
                Celeste.Celeste.Instance.Components.Add(new DoTheThingLater { Thing = unhookStuffRightNow });
            } else {
                unhookStuffRightNow();
            }
        }

        private void hookStuffRightNow() {
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

            VanillaVariantOptions.Load();

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Loading variant randomizer...");
            Randomizer.Load();

            foreach (Variant variant in VariantHandlers.Keys.ToList()) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Loading variant {variant}...");
                VariantHandlers[variant].Load();
            }

            LeakPreventionHack.Load();

            TriggerManager.Load();
            Entities.Legacy.ExtendedVariantTrigger.Load();
            Entities.ForMappers.AbstractExtendedVariantTriggerTeleportHandler.Load();
            Entities.ForMappers.ExtendedVariantTheoCrystal.Load();

            Logger.Log(LogLevel.Info, "ExtendedVariantMode/ExtendedVariantsModule", "Done hooking stuff.");

            if (gameInitialized) {
                initializeStuff();
            }

            stuffIsHooked = true;
        }

        private void initializeStuff() {
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "Loading mod hooks...");
            UpsideDown.Initialize();
            AbstractVanillaVariant.Initialize();
            VanillaVariantOptions.Initialize();
        }

        public void unhookStuffRightNow() {
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

            VanillaVariantOptions.Unload();

            // unset flags
            onLevelExit();

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant randomizer...");
            Randomizer.Unload();

            foreach (Variant variant in VariantHandlers.Keys) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant {variant}...");
                VariantHandlers[variant].Unload();
            }

            LeakPreventionHack.Unload();

            TriggerManager.Unload();
            Entities.Legacy.ExtendedVariantTrigger.Unload();
            Entities.ForMappers.AbstractExtendedVariantTriggerTeleportHandler.Unload();
            Entities.ForMappers.ExtendedVariantTheoCrystal.Unload();

            Logger.Log(LogLevel.Info, "ExtendedVariantMode/ExtendedVariantsModule", "Done unhooking stuff.");

            stuffIsHooked = false;
        }

        private bool isExtendedVariantEntity(string name) {
            return name == "ExtendedVariantTrigger" || name.StartsWith("ExtendedVariantMode/");
        }

        private void checkForceEnableVariants(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            forceEnabled = false;

            if (AreaData.Areas.Count > session.Area.ID && AreaData.Areas[session.Area.ID].Mode.Length > (int) session.Area.Mode
                && AreaData.Areas[session.Area.ID].Mode[(int) session.Area.Mode] != null
                && session.MapData.Levels.Exists(levelData =>
                    levelData.Triggers.Exists(entityData => isExtendedVariantEntity(entityData.Name)) ||
                    levelData.Entities.Exists(entityData => isExtendedVariantEntity(entityData.Name)))) {

                // the level we're entering has an Extended Variant Trigger: force-enable extended variants.
                forceEnabled = true;

                // if variants are disabled, we want to enable them as well, with default values
                // (so that we don't get variants that were enabled long ago).
                if (!stuffIsHooked) {
                    Settings.MasterSwitch = true;
                    HookStuff();
                }
            }

            orig(self, session, startPosition);
        }

        /// <summary>
        /// Resets both vanilla and extended variants to default settings.
        /// </summary>
        /// <returns>true if any variant changed, false otherwise.</returns>
        public void ResetToDefaultSettings() {
            ResetVanillaVariantsToDefaultSettings();
            ResetExtendedVariantsToDefaultSettings();
        }

        /// <summary>
        /// Resets extended variants to default settings.
        /// </summary>
        public void ResetExtendedVariantsToDefaultSettings() {
            foreach (Variant variantId in Settings.EnabledVariants.Keys.ToList()) {
                AbstractExtendedVariant variant = VariantHandlers[variantId];

                Settings.EnabledVariants.Remove(variantId);
                Session?.VariantsOverridenByUser.Remove(variantId);
                variant.VariantValueChanged();
                Randomizer.RefreshEnabledVariantsDisplayList();
            }
        }

        /// <summary>
        /// Resets vanilla variants to default settings.
        /// </summary>
        public void ResetVanillaVariantsToDefaultSettings() {
            foreach (Variant variantId in VariantHandlers.Keys) {
                AbstractExtendedVariant variant = VariantHandlers[variantId];

                if (variant is AbstractVanillaVariant vanillaVariant) {
                    Session?.VariantsOverridenByUser.Remove(variantId);
                    vanillaVariant.VariantValueChangedByPlayer(TriggerManager.GetCurrentVariantValue(variantId));
                    variant.VariantValueChanged();
                    Randomizer.RefreshEnabledVariantsDisplayList();
                }
            }

            if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "IsaGrabBag", Version = new Version(1, 6, 10) })) {
                resetIsaGrabBagToDefault();
            }
        }

        private void resetIsaGrabBagToDefault() {
            typeof(Celeste.Mod.IsaGrabBag.ForceVariants).GetMethod("set_Variants_Default", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { new bool[11] { false, false, false, false, false, false, false, false, false, false, false } });
        }

        // ==================== Reset Variants commands =====================

        [Command("reset_vanilla_variants", "[from Extended Variant Mode] resets vanilla variants to their default values")]
        public static void CmdResetVanillaVariants() {
            if (SaveData.Instance == null) {
                Engine.Commands.Log("This command only works when a save is loaded!");
            } else {
                Instance.ResetVanillaVariantsToDefaultSettings();
            }
        }

        [Command("reset_extended_variants", "[from Extended Variant Mode] resets extended variants to their default values")]
        public static void CmdResetExtendedVariants() {
            Instance.ResetExtendedVariantsToDefaultSettings();
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
            if (!Session.ExtendedVariantsWereUsed && (Settings.EnabledVariants.Count != 0 || Session.VariantsOverridenByUser.Count != 0)) {
                Logger.Log(LogLevel.Warn, "ExtendedVariantMode/ExtendedVariantsModule", "/!\\ Variants have been used! Tagging session as dirty!");
                Session.ExtendedVariantsWereUsed = true;
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

        // ================ Fix types for deserialized settings ================

        // when you save an array of objects with YamlDotNet and load them back...
        // they all turn into strings. aaaaaaaaaaaaaaaa

        public override void SaveSettings() {
            ExtendedVariantsSettings savableSettings = new ExtendedVariantsSettings() {
                MasterSwitch = Settings.MasterSwitch,
                EnabledVariants = ExtendedVariantSerializationUtils.ToSavableFormat(Settings.EnabledVariants),
                ChangeVariantsRandomly = Settings.ChangeVariantsRandomly,
                VariantSet = Settings.VariantSet,
                ChangeVariantsInterval = Settings.ChangeVariantsInterval,
                RandomizerEnabledVariants = Settings.RandomizerEnabledVariants,
                RerollMode = Settings.RerollMode,
                MaxEnabledVariants = Settings.MaxEnabledVariants,
                RandoSetSeed = Settings.RandoSetSeed,
                Vanillafy = Settings.Vanillafy,
                DisplayEnabledVariantsToScreen = Settings.DisplayEnabledVariantsToScreen,
                OnScreenDisplayTextOpacity = Settings.OnScreenDisplayTextOpacity,
                OnScreenDisplayBackgroundOpacity = Settings.OnScreenDisplayBackgroundOpacity
            };

            try {
                string path = UserIO.GetSaveFilePath("modsettings-" + Metadata.Name);
                if (File.Exists(path)) File.Delete(path);

                using (FileStream stream = File.OpenWrite(path))
                using (StreamWriter writer = new StreamWriter(stream)) {
                    YamlHelper.Serializer.Serialize(writer, savableSettings, SettingsType);
                    if ((CoreModule.Settings.SaveDataFlush ?? true) && !MainThreadHelper.IsMainThread)
                        stream.Flush(true);
                }
            } catch (Exception e) {
                Logger.Log(LogLevel.Warn, "EverestModule", $"Failed to save the settings of {Metadata.Name}!");
                Logger.LogDetailed(e);
            }
        }

        public override void LoadSettings() {
            base.LoadSettings();
            Settings.EnabledVariants = ExtendedVariantSerializationUtils.FromSavableFormat(Settings.EnabledVariants);
        }

        // ================ Fix types for deserialized sessions ================

        public override byte[] SerializeSession(int index) {
            ExtendedVariantsSession savableSession = new ExtendedVariantsSession() {
                VariantsEnabledViaTrigger = ExtendedVariantSerializationUtils.ToSavableFormat(Session.VariantsEnabledViaTrigger),
                VariantsOverridenByUser = Session.VariantsOverridenByUser,
                ExtendedVariantsWereUsed = Session.ExtendedVariantsWereUsed,
                DashCountOnLatestRespawn = Session.DashCountOnLatestRespawn,
                ExtendedVariantsDisplayedOnScreenViaTrigger = Session.ExtendedVariantsDisplayedOnScreenViaTrigger
            };

            using (MemoryStream stream = new MemoryStream()) {
                using (StreamWriter writer = new StreamWriter(new UndisposableStream(stream)))
                    YamlHelper.Serializer.Serialize(writer, savableSession, SessionType);

                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }

        public override void DeserializeSession(int index, byte[] data) {
            base.DeserializeSession(index, data);
            Session.VariantsEnabledViaTrigger = ExtendedVariantSerializationUtils.FromSavableFormat(Session.VariantsEnabledViaTrigger);
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
