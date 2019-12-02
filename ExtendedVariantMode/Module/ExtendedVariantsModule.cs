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
using Mono.Cecil.Cil;
using System.Xml;
using MonoMod.RuntimeDetour;
using Mono.Cecil;
using MonoMod.Utils;
using System.Reflection;

namespace ExtendedVariants.Module {
    public class ExtendedVariantsModule : EverestModule {

        public static ExtendedVariantsModule Instance;

        private bool stuffIsHooked = false;
        private bool triggerIsHooked = false;
        private bool variantsWereForceEnabled = false;
        private Postcard forceEnabledVariantsPostcard;

        public override Type SettingsType => typeof(ExtendedVariantsSettings);
        public override Type SessionType => typeof(ExtendedVariantsSession);

        public static ExtendedVariantsSettings Settings => (ExtendedVariantsSettings)Instance._Settings;
        public static ExtendedVariantsSession Session => (ExtendedVariantsSession)Instance._Session;

        public VariantRandomizer Randomizer;

        public enum Variant {
            Gravity, FallSpeed, JumpHeight, WallBouncingSpeed, DisableWallJumping, JumpCount, RefillJumpsOnDashRefill, DashSpeed, DashLength,
            HyperdashSpeed, DashCount, HeldDash, DontRefillDashOnGround, SpeedX, Friction, AirFriction, BadelineChasersEverywhere, ChaserCount,
            AffectExistingChasers, BadelineBossesEverywhere, BadelineAttackPattern, ChangePatternsOfExistingBosses, BadelineLag, DelayBetweenBadelines,
            OshiroEverywhere, DisableOshiroSlowdown, WindEverywhere, SnowballsEverywhere, SnowballDelay, AddSeekers, DisableSeekerSlowdown, TheoCrystalsEverywhere, 
            Stamina, UpsideDown, DisableNeutralJumping, RegularHiccups, HiccupStrength, RoomLighting, RoomBloom, EverythingIsUnderwater, ForceDuckOnGround,
            InvertDashes, InvertGrab, AllStrawberriesAreGoldens, GameSpeed, ColorGrading
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
            VariantHandlers[Variant.InvertGrab] = new InvertGrab();
            VariantHandlers[Variant.DisableNeutralJumping] = new DisableNeutralJumping();
            VariantHandlers[Variant.BadelineChasersEverywhere] = new BadelineChasersEverywhere();
            // ChaserCount is not a variant
            // AffectExistingChasers is not a variant
            VariantHandlers[Variant.BadelineBossesEverywhere] = new BadelineBossesEverywhere();
            // BadelineAttackPattern is not a variant
            // ChangePatternsOfExistingBosses is not a variant
            VariantHandlers[Variant.RegularHiccups] = new RegularHiccups();
            // HiccupStrength is not a variant
            // RefillJumpsOnDashRefill is not a variant
            VariantHandlers[Variant.RoomLighting] = new RoomLighting();
            VariantHandlers[Variant.RoomBloom] = new RoomBloom();
            VariantHandlers[Variant.EverythingIsUnderwater] = new EverythingIsUnderwater();
            VariantHandlers[Variant.OshiroEverywhere] = new OshiroEverywhere();
            // DisableOshiroSlowdown is not a variant
            VariantHandlers[Variant.WindEverywhere] = new WindEverywhere();
            VariantHandlers[Variant.SnowballsEverywhere] = new SnowballsEverywhere();
            // SnowballDelay is not a variant
            VariantHandlers[Variant.AddSeekers] = new AddSeekers();
            // DisableSeekerSlowdown is not a variant
            VariantHandlers[Variant.TheoCrystalsEverywhere] = new TheoCrystalsEverywhere();
            // BadelineLag is not a variant
            // DelayBetweenBadelines is not a variant
            VariantHandlers[Variant.AllStrawberriesAreGoldens] = new AllStrawberriesAreGoldens();
            VariantHandlers[Variant.DontRefillDashOnGround] = new DontRefillDashOnGround();
            VariantHandlers[Variant.GameSpeed] = new GameSpeed();
            VariantHandlers[Variant.ColorGrading] = new ColorGrading();
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            base.CreateModMenuSection(menu, inGame, snapshot);

            new ModOptionsEntries().CreateAllOptions(menu, inGame, triggerIsHooked);
        }

        // ================ Variants hooking / unhooking ================

        public override void Load() {
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "Initializing Extended Variant Mode");

            On.Celeste.LevelEnter.Go += checkForceEnableVariants;
            On.Celeste.LevelExit.ctor += checkForTriggerUnhooking;
            On.Celeste.TextMenu.GetYOffsetOf += fixYOffsetOfMenuOptions;
            IL.Celeste.Fonts.Prepare += registerExtendedKoreanFont;
            On.Monocle.PixelFont.AddFontSize_string_XmlElement_Atlas_bool += loadOrMergeExtendedFont;

            if (Settings.MasterSwitch) {
                // variants are enabled: we want to hook them on startup.
                HookStuff();
            }
        }

        public override void Unload() {
            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "Unloading Extended Variant Mode");

            On.Celeste.LevelEnter.Go -= checkForceEnableVariants;
            On.Celeste.LevelExit.ctor -= checkForTriggerUnhooking;
            On.Celeste.TextMenu.GetYOffsetOf -= fixYOffsetOfMenuOptions;
            IL.Celeste.Fonts.Prepare -= registerExtendedKoreanFont;
            On.Monocle.PixelFont.AddFontSize_string_XmlElement_Atlas_bool -= loadOrMergeExtendedFont;

            if (stuffIsHooked) {
                UnhookStuff();
            }
        }

        public void HookStuff() {
            if (stuffIsHooked) return;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Loading variant common methods...");
            On.Celeste.AreaComplete.VersionNumberAndVariants += modVersionNumberAndVariants;
            Everest.Events.Level.OnExit += onLevelExit;
            On.Celeste.BadelineBoost.BoostRoutine += modBadelineBoostRoutine;
            On.Celeste.CS00_Ending.OnBegin += onPrologueEndingCutsceneBegin;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Loading variant randomizer...");
            Randomizer.Load();

            foreach(Variant variant in VariantHandlers.Keys) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Loading variant {variant}...");
                VariantHandlers[variant].Load();
            }

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "Done hooking stuff.");

            stuffIsHooked = true;
        }

        public void UnhookStuff() {
            if (!stuffIsHooked) return;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant common methods...");
            On.Celeste.AreaComplete.VersionNumberAndVariants -= modVersionNumberAndVariants;
            Everest.Events.Level.OnExit -= onLevelExit;
            On.Celeste.BadelineBoost.BoostRoutine -= modBadelineBoostRoutine;
            On.Celeste.CS00_Ending.OnBegin -= onPrologueEndingCutsceneBegin;

            // unset flags
            onLevelExit();

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant randomizer...");
            Randomizer.Unload();
            
            foreach(Variant variant in VariantHandlers.Keys) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant {variant}...");
                VariantHandlers[variant].Unload();
            }

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", "Done unhooking stuff.");

            stuffIsHooked = false;
        }

        private void hookTrigger() {
            if (triggerIsHooked) return;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Loading variant trigger manager...");
            TriggerManager.Load();

            On.Celeste.LevelEnter.Routine += addForceEnabledVariantsPostcard;
            On.Celeste.LevelEnter.BeforeRender += addForceEnabledVariantsPostcardRendering;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Done loading variant trigger manager.");

            triggerIsHooked = true;
        }

        private void unhookTrigger() {
            if (!triggerIsHooked) return;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Unloading variant trigger manager...");
            TriggerManager.Unload();

            On.Celeste.LevelEnter.Routine -= addForceEnabledVariantsPostcard;
            On.Celeste.LevelEnter.BeforeRender -= addForceEnabledVariantsPostcardRendering;

            Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Done unloading variant trigger manager.");

            triggerIsHooked = false;
        }
        
        private void checkForceEnableVariants(On.Celeste.LevelEnter.orig_Go orig, Session session, bool fromSaveData) {
            if(session.MapData.Levels.Exists(levelData => levelData.Triggers.Exists(entityData => entityData.Name == "ExtendedVariantTrigger"))) {
                // the level we're entering has an Extended Variant Trigger: load the trigger on-demand.
                hookTrigger();

                // if variants are disabled, we want to enable them as well, with default values
                // (so that we don't get variants that were enabled long ago).
                if(!stuffIsHooked) {
                    variantsWereForceEnabled = true;
                    Settings.MasterSwitch = true;
                    ResetToDefaultSettings();
                    HookStuff();
                    SaveSettings();
                }
            }

            orig(session, fromSaveData);
        }

        private IEnumerator addForceEnabledVariantsPostcard(On.Celeste.LevelEnter.orig_Routine orig, LevelEnter self) {
            if(variantsWereForceEnabled) {
                variantsWereForceEnabled = false;

                // let's show a postcard to let the player know Extended Variants have been enabled.
                self.Add(forceEnabledVariantsPostcard = new Postcard(Dialog.Get("POSTCARD_EXTENDEDVARIANTS_FORCEENABLED"), "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out"));
                yield return forceEnabledVariantsPostcard.DisplayRoutine();
                forceEnabledVariantsPostcard = null;
            }

            // just go on with vanilla behavior (other postcards, B-side intro, etc)
            yield return orig(self);
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

        public void ResetToDefaultSettings() {
            if(Settings.RoomLighting != -1 && Engine.Scene.GetType() == typeof(Level)) {
                // currently in level, change lighting right away
                Level lvl = (Engine.Scene as Level);
                lvl.Lighting.Alpha = lvl.BaseLightingAlpha + lvl.Session.LightingAlphaAdd;
            }
            if(Settings.RoomBloom != -1 && Engine.Scene.GetType() == typeof(Level)) {
                // currently in level, change bloom right away
                Level lvl = (Engine.Scene as Level);
                lvl.Bloom.Base = AreaData.Get(lvl).BloomBase + lvl.Session.BloomBaseAdd;
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
            Settings.DelayBetweenBadelines = 4;
            Settings.ChangeVariantsRandomly = false;
            Settings.DisableOshiroSlowdown = false;
            Settings.DisableSeekerSlowdown = false;
            Settings.BadelineAttackPattern = 0;
            Settings.ChangePatternsOfExistingBosses = false;
        }

        // ================ Fix for TextMenu Y offset of options ================

        // TODO REMOVE ON NEXT EVEREST STABLE: ships with Everest 1148+
        private float fixYOffsetOfMenuOptions(On.Celeste.TextMenu.orig_GetYOffsetOf orig, TextMenu self, TextMenu.Item itemToGetOffsetFor) {
            if (itemToGetOffsetFor == null) return 0f;

            float offset = 0f;
            foreach (TextMenu.Item itemFromList in self.GetItems()) {
                if (itemFromList.Visible) // this is itemToGetOffsetFor in vanilla, which is plain broken
                    offset += itemFromList.Height() + self.ItemSpacing;
                if (itemFromList == itemToGetOffsetFor)
                    break;
            }

            return offset - itemToGetOffsetFor.Height() * 0.5f - self.ItemSpacing;
        }

        // ================ Font support for missing Korean characters ================

        private void registerExtendedKoreanFont(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go to the end of the method
            if (cursor.TryGotoNext(instr => instr.MatchRet())) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Injecting font loading code at {cursor.Index} in IL code for Fonts.Prepare");

                // we just want to inject ourselves into the "paths" variable, listing all available fonts.
                cursor.Emit(OpCodes.Ldsfld, typeof(Fonts).GetField("paths", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
                cursor.EmitDelegate<Action<Dictionary<string, List<string>>>>(injectExtendedFont);
            }
        }

        private void injectExtendedFont(Dictionary<string, List<string>> paths) {
            string fontName = "Noto Sans CJK KR Medium";

            // add the font if not loaded yet (it should be though)
            if (!paths.TryGetValue(fontName, out List<string> pathList)) {
                paths.Add(fontName, pathList = new List<string>());
            }

            // add our extended language xml afterwards, so the game will pick it up when loading the Korean font.
            // Everest manages the "loading from zip / mod directory" part already.
            pathList.Add("Dialog/Fonts/max480_extendedvariants_extendedkorean.xml");
        }

        private PixelFontSize loadOrMergeExtendedFont(On.Monocle.PixelFont.orig_AddFontSize_string_XmlElement_Atlas_bool orig, PixelFont self, 
            string path, XmlElement data, Atlas atlas, bool outline) {

            PixelFontSize loadedFontSize = orig(self, path, data, atlas, outline);

            if(path == "Dialog/Fonts/max480_extendedvariants_extendedkorean.xml") {
                // we just loaded our extended font, we shall merge it with the original font.
                // (a size of 63 has been set in purpose so that Celeste will load it as a different size.)
                foreach (PixelFontSize originalFontSize in self.Sizes) {
                    if (originalFontSize.Size == 64) {
                        Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Adding {loadedFontSize.Characters.Count} extra characters into the Korean font.");

                        foreach (int character in loadedFontSize.Characters.Keys) originalFontSize.Characters[character] = loadedFontSize.Characters[character];
                        originalFontSize.Textures.AddRange(loadedFontSize.Textures);

                        self.Sizes.Remove(loadedFontSize);

                        return originalFontSize;
                    }
                }
            }

            return loadedFontSize;
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
        private static bool prologueEndingCutscene = false;

        /// <summary>
        /// Utility method to patch "coroutine" kinds of methods with IL.
        /// Those methods' code reside in a compiler-generated method, and IL.Celeste.* do not allow manipulating them directly.
        /// </summary>
        /// <param name="manipulator">Method taking care of the patching</param>
        /// <returns>The IL hook if the actual code was found, null otherwise</returns>
        public static ILHook HookCoroutine(string typeName, string methodName, ILContext.Manipulator manipulator) {
            // get the Celeste.exe module definition Everest loaded for us
            ModuleDefinition celeste = Everest.Relinker.SharedRelinkModuleMap["Celeste.Mod.mm"];

            // get the type
            TypeDefinition type = celeste.GetType(typeName);
            if (type == null) return null;

            // the "coroutine" method is actually a nested type tracking the coroutine's state
            // (to make it restart from where it stopped when MoveNext() is called).
            // what we see in ILSpy and what we want to hook is actually the MoveNext() method in this nested type.
            foreach (TypeDefinition nest in type.NestedTypes) {
                if (nest.Name.StartsWith("<" + methodName + ">d__")) {
                    // check that this nested type contains a MoveNext() method
                    MethodDefinition method = nest.FindMethod("System.Boolean MoveNext()");
                    if (method == null) return null;

                    // we found it! let's convert it into basic System.Reflection stuff and hook it.
                    Logger.Log("ExtendedVariantMode/ExtendedVariantsModule", $"Building IL hook for method {method.FullName} in order to mod {typeName}.{methodName}()");
                    Type reflectionType = typeof(Player).Assembly.GetType(typeName);
                    Type reflectionNestedType = reflectionType.GetNestedType(nest.Name, BindingFlags.NonPublic);
                    MethodBase moveNextMethod = reflectionNestedType.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);
                    return new ILHook(moveNextMethod, manipulator);
                }
            }

            return null;
        }

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
