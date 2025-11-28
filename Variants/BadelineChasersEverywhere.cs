using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class BadelineChasersEverywhere : AbstractExtendedVariant {

        public static bool UsingWatchtower { get; private set; } = false;

        private static float lastChaserLag = 0f;

        public BadelineChasersEverywhere() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            IL.Celeste.BadelineOldsite.ctor_Vector2_int += modBadelineOldsiteConstructor;
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;
            IL.Celeste.BadelineOldsite.Added += modBadelineOldsiteAdded;
            IL.Celeste.BadelineOldsite.CanChangeMusic += modBadelineOldsiteCanChangeMusic;
            On.Celeste.BadelineOldsite.IsChaseEnd += modBadelineOldsiteIsChaseEnd;
            IL.Celeste.Player.UpdateChaserStates += modUpdateChaserStates;

            On.Celeste.LevelLoader.ctor += onLevelStart;
            On.Celeste.Lookout.Interact += onWatchtowerInteract;
            On.Celeste.Lookout.LookRoutine += onWatchtowerUse;
            On.Celeste.Player.UpdateChaserStates += onUpdateChaserStates;
        }

        public override void Unload() {
            IL.Celeste.BadelineOldsite.ctor_Vector2_int -= modBadelineOldsiteConstructor;
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
            IL.Celeste.BadelineOldsite.Added -= modBadelineOldsiteAdded;
            IL.Celeste.BadelineOldsite.CanChangeMusic -= modBadelineOldsiteCanChangeMusic;
            On.Celeste.BadelineOldsite.IsChaseEnd -= modBadelineOldsiteIsChaseEnd;
            IL.Celeste.Player.UpdateChaserStates -= modUpdateChaserStates;

            On.Celeste.LevelLoader.ctor -= onLevelStart;
            On.Celeste.Lookout.Interact -= onWatchtowerInteract;
            On.Celeste.Lookout.LookRoutine -= onWatchtowerUse;
            On.Celeste.Player.UpdateChaserStates -= onUpdateChaserStates;

            // this is reset on every room enter. reset that in case we enable variants again mid-level
            lastChaserLag = 0f;

            // force every Badeline to unfreeze and vanish.
            UsingWatchtower = false;
        }

        private static void modBadelineOldsiteConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go everywhere where the 1.55 second delay is defined
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1.55f))) {
                Logger.Log("ExtendedVariantMode/BadelineChasersEverywhere", $"Modding Badeline lag at {cursor.Index} in CIL code for BadelineOldsite constructor");

                // and substitute it with our own value
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate<Func<float>>(determineBadelineLag);
            }

            cursor.Index = 0;

            // go everywhere where the 0.4 second delay between Badelines is defined
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.4f))) {
                Logger.Log("ExtendedVariantMode/BadelineChasersEverywhere", $"Modding delay between Badelines at {cursor.Index} in CIL code for BadelineOldsite constructor");

                // and substitute it with our own value
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate<Func<float>>(determineDelayBetweenBadelines);
            }
        }

        private static float determineBadelineLag() {
            return ExtendedVariantsModule.ShouldIgnoreCustomDelaySettings() ? 1.55f : GetVariantValue<float>(Variant.BadelineLag);
        }

        private static float determineDelayBetweenBadelines() {
            return GetVariantValue<float>(Variant.DelayBetweenBadelines);
        }

        /// <summary>
        /// Wraps the LoadLevel method in order to add Badeline chasers when needed.
        /// </summary>
        /// <param name="orig">The base method</param>
        /// <param name="self">The level entity</param>
        /// <param name="playerIntro">The type of player intro</param>
        /// <param name="isFromLoader">unused</param>
        private static void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            // this method takes care of every situation except transitions, we let this one to TransitionRoutine
            if (GetVariantValue<bool>(Variant.BadelineChasersEverywhere) && playerIntro != Player.IntroTypes.Transition) {
                // set this to avoid the player being instakilled during the level intro animation
                Player player = self.Tracker.GetEntity<Player>();
                if (player != null) player.JustRespawned = true;
            }

            if (playerIntro != Player.IntroTypes.Transition) {
                if ((GetVariantValue<bool>(Variant.BadelineChasersEverywhere) || GetVariantValue<bool>(Variant.AffectExistingChasers))) {
                    injectBadelineChasers(self);
                }
                updateLastChaserLag(self);
            }
        }

        /// <summary>
        /// Wraps the TransitionRoutine in Level, in order to add Badeline chasers when needed.
        /// This is not done in LoadLevel, since this one will wait for the transition to be done, so that the entities from the previous screen are unloaded.
        /// </summary>
        /// <param name="orig">The base method</param>
        /// <param name="self">The level entity</param>
        /// <param name="next">unused</param>
        /// <param name="direction">unused</param>
        /// <returns></returns>
        private static IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            yield return new SwapImmediately(orig(self, next, direction));

            // decide whether to add Badeline or not
            injectBadelineChasers(self);
            updateLastChaserLag(self);
        }

        /// <summary>
        /// Mods the Added method in BadelineOldsite, to make it not kill chasers on screens they are not supposed to be.
        /// </summary>
        /// <param name="il">Object allowing IL modding</param>
        private static void modBadelineOldsiteAdded(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go right after the equality check that compares the level set name with "Celeste"
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Call && ((MethodReference) instr.Operand).Name.Contains("op_Equality"))) {
                Logger.Log("ExtendedVariantMode/BadelineChasersEverywhere", $"Modding vanilla level check at index {cursor.Index} in the Added method from BadelineOldsite");

                // mod the result of that check to prevent the chasers we will spawn from... committing suicide
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<bool, Scene, bool>>(modVanillaBehaviorCheckForChasers);
            }
        }

        /// <summary>
        /// Mods the CanChangeMusic method in BadelineOldsite, so that forcibly added chasers do not change the level music.
        /// </summary>
        /// <param name="il">Object allowing IL modding</param>
        private static void modBadelineOldsiteCanChangeMusic(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go right after the equality check that compares the level set name with "Celeste"
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Call && ((MethodReference) instr.Operand).Name.Contains("op_Equality"))) {
                Logger.Log("ExtendedVariantMode/BadelineChasersEverywhere", $"Modding vanilla level check at index {cursor.Index} in the CanChangeMusic method from BadelineOldsite");

                // mod the result of that check to always use modded value, even in vanilla levels
                cursor.EmitDelegate<Func<bool, bool>>(modVanillaBehaviorCheckForMusic);
            }
        }

        private static void injectBadelineChasers(Level level) {
            bool hasChasersInBaseLevel = level.Tracker.CountEntities<BadelineOldsite>() != 0;

            if (GetVariantValue<bool>(Variant.BadelineChasersEverywhere)) {
                Player player = level.Tracker.GetEntity<Player>();

                // check if the base level already has chasers
                if (player != null && !hasChasersInBaseLevel) {
                    // add a Badeline chaser where the player is, and tell it not to change the music to the chase music
                    for (int i = 0; i < GetVariantValue<int>(Variant.ChaserCount); i++) {
                        level.Add(new AutoDestroyingBadelineOldsite(generateBadelineEntityData(level, i), player.Position, i));
                    }

                    level.Entities.UpdateLists();
                }
            }

            // plz disregard the settings and don't touch the chasers if in Badeline Intro cutscene
            // because the chaser triggers the cutscene, so having 10 chasers triggers 10 instances of the cutscene at the same time (a)
            if (GetVariantValue<bool>(Variant.AffectExistingChasers) && hasChasersInBaseLevel && notInBadelineIntroCutscene(level)) {
                List<Entity> chasers = level.Tracker.GetEntities<BadelineOldsite>();
                if (chasers.Count > GetVariantValue<int>(Variant.ChaserCount)) {
                    // for example, if there are 6 chasers and we want 3, we will ask chasers 4-6 to commit suicide
                    for (int i = chasers.Count - 1; i >= GetVariantValue<int>(Variant.ChaserCount); i--) {
                        chasers[i].RemoveSelf();
                    }
                } else if (chasers.Count < GetVariantValue<int>(Variant.ChaserCount)) {
                    // for example, if we have 2 chasers and we want 6, we will duplicate both chasers twice
                    for (int i = chasers.Count; i < GetVariantValue<int>(Variant.ChaserCount); i++) {
                        int baseChaser = i % chasers.Count;
                        level.Add(new AutoDestroyingBadelineOldsite(generateBadelineEntityData(level, i), chasers[baseChaser].Position, i));
                    }
                }

                level.Entities.UpdateLists();
            }
        }

        private static bool notInBadelineIntroCutscene(Level level) {
            return (level.Session.Area.GetSID() != "Celeste/2-OldSite" || level.Session.Level != "3" || level.Session.Area.Mode != AreaMode.Normal);
        }

        private static EntityData generateBadelineEntityData(Level level, int badelineNumber) {
            EntityData entityData = ExtendedVariantsModule.GenerateBasicEntityData(level, badelineNumber);
            entityData.Values["canChangeMusic"] = false;
            return entityData;
        }

        private static bool modVanillaBehaviorCheckForMusic(bool shouldUseVanilla) {
            // we can use the "flag-based behavior" on all A-sides
            if (Engine.Scene.GetType() == typeof(Level) && (Engine.Scene as Level).Session.Area.Mode == AreaMode.Normal) {
                return false;
            }
            // fall back to standard Everest behavior everywhere else: vanilla will not trigger chase music, and Everest will be flag-based
            return shouldUseVanilla;
        }

        private static bool modVanillaBehaviorCheckForChasers(bool shouldUseVanilla, Scene scene) {
            Session session = (scene as Level).Session;

            if (GetVariantValue<bool>(Variant.BadelineChasersEverywhere) &&
                // don't use vanilla behaviour when that would lead the chasers to commit suicide
                (!session.GetLevelFlag("3") || session.GetLevelFlag("11") ||
                // don't use vanilla behaviour when that would trigger the Badeline intro cutscene, except (of course) on Old Site
                (session.Area.GetSID() != "Celeste/2-OldSite" && session.Level == "3" && session.Area.Mode == AreaMode.Normal))) {
                return false;
            }
            return shouldUseVanilla;
        }

        private static bool modBadelineOldsiteIsChaseEnd(On.Celeste.BadelineOldsite.orig_IsChaseEnd orig, BadelineOldsite self, bool value) {
            Session session = self.SceneAs<Level>().Session;
            if (session.Area.GetLevelSet() == "Celeste" && session.Area.GetSID() != "Celeste/2-OldSite") {
                // there is no chase end outside Old Site in the vanilla game.
                return false;
            }
            return orig(self, value);
        }

        /// <summary>
        /// Mods the UpdateChaserStates to tell it to save a bit more history of chaser states, so that we can spawn more chasers.
        /// </summary>
        /// <param name="il">Object allowing IL modding</param>
        private static void modUpdateChaserStates(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go where the "4" is (this is the amount of player history vanilla is keeping)
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(4f))) {
                Logger.Log("ExtendedVariantMode/BadelineChasersEverywhere", $"Modding constant at {cursor.Index} in the UpdateChaserStates method to allow more chasers to spawn");

                // call a method that will compute the right delay instead
                cursor.EmitDelegate<Func<float, float>>(determineHistoryAmountToKeep);
            }
        }

        private static float determineHistoryAmountToKeep(float orig) {
            // return the amount of lag the farthest Badeline has (with a 0.1s margin), but make it 4 seconds minimum (= vanilla value).
            float moddedDelay = lastChaserLag + 0.1f;
            return moddedDelay < 4f ? orig : moddedDelay;
        }

        private static void updateLastChaserLag(Level self) {
            // called just after the extended Badelines are injected (if required).
            // we can count how many vanilla + extended Badelines we have on-screen.
            int badelineCount = self.Entities.AmountOf<BadelineOldsite>();
            lastChaserLag = determineBadelineLag() + (badelineCount - 1) * determineDelayBetweenBadelines();
        }

        // ========== Watchtower handling ==========

        private static void onLevelStart(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition) {
            orig(self, session, startPosition);
            UsingWatchtower = false;
        }

        private static void onWatchtowerInteract(On.Celeste.Lookout.orig_Interact orig, Lookout self, Player player) {
            orig(self, player);

            // Pandora's Box seems to throw the player in the Dummy state 1 frame too early, so we got to flip the flag earlier as well. :a:
            UsingWatchtower = true;
        }

        private static IEnumerator onWatchtowerUse(On.Celeste.Lookout.orig_LookRoutine orig, Lookout self, Player player) {
            UsingWatchtower = true;
            float timeStartedUsing = Engine.Scene.TimeActive;

            yield return new SwapImmediately(orig(self, player));

            UsingWatchtower = false;

            if (GetVariantValue<bool>(Variant.BadelineChasersEverywhere)) {
                // adjust chaser state timestamps so that they behave as if the player didn't stand by that watchtower.
                float timeDiff = Engine.Scene.TimeActive - timeStartedUsing;
                for (int i = 0; i < player.ChaserStates.Count; i++) {
                    // structs are nice. I have to copy it, modify it, then place it back.
                    Player.ChaserState state = player.ChaserStates[i];
                    state.TimeStamp += timeDiff;
                    player.ChaserStates[i] = state;
                }
            }
        }

        private static void onUpdateChaserStates(On.Celeste.Player.orig_UpdateChaserStates orig, Player self) {
            // we want to stop saving chaser states when Madeline is using a watchtower, because Extended Variants-brand Badeline chasers are going to pause.
            if (!GetVariantValue<bool>(Variant.BadelineChasersEverywhere) || !UsingWatchtower) {
                orig(self);
            }
        }
    }
}
