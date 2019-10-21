using System;
using System.Collections.Generic;
using Monocle;
using FMOD.Studio;
using System.Collections;
using Celeste.Mod;
using ExtendedVariants.UI;
using Celeste;
using ExtendedVariants.Variants;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsModule : EverestModule {

        public static ExtendedVariantsModule Instance;

        public override Type SettingsType => typeof(ExtendedVariantsSettings);
        public override Type SessionType => typeof(ExtendedVariantsSession);

        public static ExtendedVariantsSettings Settings => (ExtendedVariantsSettings)Instance._Settings;
        public static ExtendedVariantsSession Session => (ExtendedVariantsSession)Instance._Session;

        public VariantRandomizer Randomizer;

        public enum Variant {
            Gravity, FallSpeed, JumpHeight, WallBouncingSpeed, DisableWallJumping, JumpCount, RefillJumpsOnDashRefill, DashSpeed, DashLength,
            HyperdashSpeed, DashCount, HeldDash, SpeedX, Friction, AirFriction, BadelineChasersEverywhere, ChaserCount, AffectExistingChasers,
            BadelineLag, OshiroEverywhere, DisableOshiroSlowdown, WindEverywhere, SnowballsEverywhere, SnowballDelay, AddSeekers, DisableSeekerSlowdown,
            TheoCrystalsEverywhere, Stamina, UpsideDown, DisableNeutralJumping, RegularHiccups, HiccupStrength, RoomLighting, ForceDuckOnGround, InvertDashes
        }

        public Dictionary<Variant, AbstractExtendedVariant> VariantHandlers = new Dictionary<Variant, AbstractExtendedVariant>();

        public ExtendedVariantTriggerManager TriggerManager;

        // ================ Module loading ================

        public ExtendedVariantsModule() {
            Instance = this;
            Randomizer = new VariantRandomizer();
            TriggerManager = new ExtendedVariantTriggerManager();

            DashCount dashCount;
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
            VariantHandlers[Variant.JumpCount] = new JumpCount(dashCount);
            VariantHandlers[Variant.UpsideDown] = new UpsideDown();
            VariantHandlers[Variant.HyperdashSpeed] = new HyperdashSpeed();
            VariantHandlers[Variant.WallBouncingSpeed] = new WallbouncingSpeed();
            VariantHandlers[Variant.DashLength] = new DashLength();
            VariantHandlers[Variant.ForceDuckOnGround] = new ForceDuckOnGround();
            VariantHandlers[Variant.InvertDashes] = new InvertDashes();
            VariantHandlers[Variant.DisableNeutralJumping] = new DisableNeutralJumping();
            VariantHandlers[Variant.BadelineChasersEverywhere] = new BadelineChasersEverywhere();
            // ChaserCount is not a variant
            // AffectExistingChasers is not a variant
            VariantHandlers[Variant.RegularHiccups] = new RegularHiccups();
            // HiccupStrength is not a variant
            // RefillJumpsOnDashRefill is not a variant
            VariantHandlers[Variant.RoomLighting] = new RoomLighting();
            VariantHandlers[Variant.OshiroEverywhere] = new OshiroEverywhere();
            // DisableOshiroSlowdown is not a variant
            VariantHandlers[Variant.WindEverywhere] = new WindEverywhere();
            VariantHandlers[Variant.SnowballsEverywhere] = new SnowballsEverywhere();
            // SnowballDelay is not a variant
            VariantHandlers[Variant.AddSeekers] = new AddSeekers();
            // DisableSeekerSlowdown is not a variant
            VariantHandlers[Variant.TheoCrystalsEverywhere] = new TheoCrystalsEverywhere();
            // BadelineLag is not a variant
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            base.CreateModMenuSection(menu, inGame, snapshot);

            new ModOptionsEntries().CreateAllOptions(menu, inGame);
        }

        public override void Load() {
            // mod methods here
            Logger.Log("ExtendedVariantsModule", $"Loading variant common methods...");
            On.Celeste.AreaComplete.VersionNumberAndVariants += modVersionNumberAndVariants;
            Everest.Events.Level.OnExit += onLevelExit;
            On.Celeste.BadelineBoost.BoostRoutine += modBadelineBoostRoutine;

            // if master switch is disabled, ensure all values are the default ones. (variants are disabled even if the yml file has been edited.)
            if (!Settings.MasterSwitch) {
                ResetToDefaultSettings();
            }

            Logger.Log("ExtendedVariantsModule", $"Loading variant randomizer...");
            Randomizer.Load();

            Logger.Log("ExtendedVariantsModule", $"Loading variant trigger manager...");
            TriggerManager.Load();

            foreach(Variant variant in VariantHandlers.Keys) {
                Logger.Log("ExtendedVariantsModule", $"Loading variant {variant}...");
                VariantHandlers[variant].Load();
            }
        }

        public override void Unload() {
            // unmod methods here
            Logger.Log("ExtendedVariantsModule", $"Unloading variant common methods...");
            On.Celeste.AreaComplete.VersionNumberAndVariants -= modVersionNumberAndVariants;
            Everest.Events.Level.OnExit -= onLevelExit;
            On.Celeste.BadelineBoost.BoostRoutine -= modBadelineBoostRoutine;

            Logger.Log("ExtendedVariantsModule", $"Unloading variant randomizer...");
            Randomizer.Unload();

            Logger.Log("ExtendedVariantsModule", $"Unloading variant trigger manager...");
            TriggerManager.Unload();
            
            foreach(Variant variant in VariantHandlers.Keys) {
                Logger.Log("ExtendedVariantsModule", $"Unloading variant {variant}...");
                VariantHandlers[variant].Unload();
            }
        }
        
        public void ResetToDefaultSettings() {
            if(Settings.RoomLighting != -1 && Engine.Scene.GetType() == typeof(Level)) {
                // currently in level, change lighting right away
                Level lvl = (Engine.Scene as Level);
                lvl.Lighting.Alpha = lvl.BaseLightingAlpha + lvl.Session.LightingAlphaAdd;
            }
            
            // reset all proper variants to their default values
            foreach(AbstractExtendedVariant variant in VariantHandlers.Values) {
                variant.SetValue(variant.GetDefaultValue());
            }

            // reset variant customization options as well
            Settings.ChaserCount = 1;
            Settings.AffectExistingChasers = false;
            Settings.HiccupStrength = 10;
            Settings.RefillJumpsOnDashRefill = false;
            Settings.SnowballDelay = 8;
            Settings.BadelineLag = 0;
            Settings.ChangeVariantsRandomly = false;
            Settings.DisableOshiroSlowdown = false;
            Settings.DisableSeekerSlowdown = false;
        }
        
        // ================ Stamp on Chapter Complete screen ================

        /// <summary>
        /// Wraps the VersionNumberAndVariants in the base game in order to add the Variant Mode logo if Extended Variants are enabled.
        /// </summary>
        private void modVersionNumberAndVariants(On.Celeste.AreaComplete.orig_VersionNumberAndVariants orig, string version, float ease, float alpha) {
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

        // ================ Common methods for multiple variants ================

        private static bool badelineBoosting = false;

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
            return player != null && (player.StateMachine.State == 10 || player.StateMachine.State == 11) && !badelineBoosting;
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
            badelineBoosting = false;
        }
    }
}
