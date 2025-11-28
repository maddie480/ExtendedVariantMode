using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class AddSeekers : AbstractExtendedVariant {

        private static Random randomGenerator = new Random();

        private static bool killSeekerSlowdownToFixHeart = false;
        private static string calcLogPrefix = null;
        private static bool pathfindingForVariant => calcLogPrefix != null;

        private ILHook pathfinderFindHook = null;

        public AddSeekers() : base(variantType: typeof(int), defaultVariantValue: 0) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value;
        }

        public override void SetRandomSeed(int seed) {
            randomGenerator = new Random(seed);
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            IL.Celeste.SeekerEffectsController.Update += onSeekerEffectsControllerUpdate;
            On.Celeste.HeartGem.Collect += onHeartGemCollect;
            On.Monocle.Calc.Log_ObjectArray += onCalcLog;

            pathfinderFindHook = new ILHook(typeof(Pathfinder).GetMethod("orig_Find"), hookPathfinderFind);
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            IL.Celeste.SeekerEffectsController.Update -= onSeekerEffectsControllerUpdate;
            On.Celeste.HeartGem.Collect -= onHeartGemCollect;
            On.Monocle.Calc.Log_ObjectArray -= onCalcLog;

            pathfinderFindHook?.Dispose();
            pathfinderFindHook = null;

            killSeekerSlowdownToFixHeart = false;
        }

        private static void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            // if we killed the slowdown earlier, stop now!
            killSeekerSlowdownToFixHeart = false;

            Level level = self;
            Player player = level.Tracker.GetEntity<Player>();

            if (player != null && GetVariantValue<int>(Variant.AddSeekers) != 0) {
                Rectangle maxBounds = new Rectangle(
                    (int) Math.Floor(level.Bounds.X / 8f) * 8,
                    (int) Math.Floor(level.Bounds.Y / 8f) * 8,
                    (int) Math.Floor(level.Bounds.Width / 8f) * 8,
                    (int) Math.Floor(level.Bounds.Height / 8f) * 8
                );

                Vector2 clampedPlayerPosition = new Vector2(
                    Calc.Clamp(player.Center.X, maxBounds.Left, maxBounds.Right - 1),
                    Calc.Clamp(player.Center.Y, maxBounds.Top, maxBounds.Bottom - 1)
                );

                Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/AddSeekers", $"Level boundaries floored to multiples of 8 are: {maxBounds}");
                Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/AddSeekers", $"Player position was clamped from {player.Center} to {clampedPlayerPosition}");

                List<Vector2> nullref = null;

                // make the seeker barriers temporarily collidable so that they are taken in account in Solid collide checks
                // and seekers can't spawn in them
                // (... yes, this is also what vanilla does in the seekers' Update method.)
                foreach (Entity entity in self.Tracker.GetEntities<SeekerBarrier>()) entity.Collidable = true;

                for (int seekerCount = 0; seekerCount < GetVariantValue<int>(Variant.AddSeekers); seekerCount++) {
                    bool found = false;

                    for (int i = 0; i < 100; i++) {
                        // roll a seeker position in the room
                        int x = randomGenerator.Next(level.Bounds.Width) + level.Bounds.X;
                        int y = randomGenerator.Next(level.Bounds.Height) + level.Bounds.Y;

                        // should be at least 100 pixels from the player
                        double playerDistance = Math.Sqrt(Math.Pow(MathHelper.Distance(x, player.X), 2) + Math.Pow(MathHelper.Distance(y, player.Y), 2));
                        if (playerDistance <= 100) {
                            Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/AddSeekers", $"Rerolling seeker: position ({x}, {y}) too close from player ({playerDistance})");
                            continue;
                        }

                        // should not be spawning in a wall, that would be a shame
                        Solid solid;
                        if ((solid = level.CollideFirst<Solid>(new Rectangle(x - 8, y - 8, 16, 16))) != null) {
                            Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/AddSeekers", $"Rerolling seeker: position ({x}, {y}) is inside {solid.GetType().FullName}");
                            continue;
                        }

                        // make sure we spawn the seeker in a zone accessible to the player (not "out of bounds")
                        calcLogPrefix = $"Rerolling seeker: pathfinding to position ({x}, {y}) ";
                        bool pathFound = level.Pathfinder.Find(ref nullref, from: clampedPlayerPosition, to: new Vector2(x, y), fewerTurns: false, logging: true);
                        calcLogPrefix = null;
                        if (!pathFound) continue;

                        Logger.Log(LogLevel.Debug, "ExtendedVariantMode/AddSeekers", $"Spawning seeker: position ({x}, {y})");

                        // build a Seeker with a proper EntityID to make Speedrun Tool happy (this is useless in vanilla Celeste but the constructor call is intercepted by Speedrun Tool)
                        EntityData seekerData = ExtendedVariantsModule.GenerateBasicEntityData(level, 10 + seekerCount);
                        seekerData.Position = new Vector2(x, y);
                        Seeker seeker = new AutoDestroyingSeeker(seekerData, Vector2.Zero);
                        level.Add(seeker);
                        found = true;
                        break;
                    }

                    if (!found) Logger.Log(LogLevel.Warn, "ExtendedVariantMode/AddSeekers", "Bailed out after 100 rerolls without spawning a seeker!");
                }

                foreach (Entity entity in self.Tracker.GetEntities<SeekerBarrier>()) entity.Collidable = false;

                if (playerIntro != Player.IntroTypes.Transition) {
                    level.Entities.UpdateLists();
                }
            }
        }

        private static void onSeekerEffectsControllerUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // let's jump to Engine.TimeRate = Calc.Approach(Engine.TimeRate, target, 4f * Engine.DeltaTime);
            if (cursor.TryGotoNext(instr => instr.MatchLdcR4(4f))) {
                Logger.Log("ExtendedVariantMode/AddSeekers", $"Adding condition for time control at {cursor.Index} in CIL code for SeekerEffectsController.Update");

                // by placing ourselves just in front of the 4f, we can turn this into
                // Engine.TimeRate = Calc.Approach(Engine.TimeRate, transformTimeRate(target), 4f * Engine.DeltaTime);
                // by injecting a single delegate call
                cursor.EmitDelegate<Func<float, float>>(transformTimeRate);
            }
        }

        private static float transformTimeRate(float vanillaTimeRate) {
            return GetVariantValue<bool>(Variant.DisableSeekerSlowdown) || killSeekerSlowdownToFixHeart ? Engine.TimeRate : vanillaTimeRate;
        }

        private static void onHeartGemCollect(On.Celeste.HeartGem.orig_Collect orig, HeartGem self, Player player) {
            orig(self, player);

            // prevent seekers from slowing down time!
            if (self.Scene.Entities.OfType<AutoDestroyingSeeker>().Count() != 0) {
                killSeekerSlowdownToFixHeart = true;
            }
        }

        private static void onCalcLog(On.Monocle.Calc.orig_Log_ObjectArray orig, object[] obj) {
            orig(obj);

            if (calcLogPrefix != null) {
                foreach (object o in obj) {
                    // all logs are prefixed with "PF: " which we can replace with our own prefix
                    Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/AddSeekers", calcLogPrefix + o.ToString().Substring(4));
                }
            }
        }

        private static void hookPathfinderFind(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(instr => instr.MatchLdfld<Entity>("Collidable"))) {
                Logger.Log("ExtendedVariantMode/AddSeekers", $"Marking dream blocks as traversable at {cursor.Index} in CIL code for Pathfinder.orig_Find");
                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.EmitDelegate<Func<Entity, bool, bool>>(modIsCollidable);
            }

            if (cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchLdarg(1))) {
                Logger.Log("ExtendedVariantMode/AddSeekers", $"Adding early return at {cursor.Index} in CIL code for Pathfinder.orig_Find");

                // if we are currently pathfinding for the variant, skip the "rebuild the found path" part of the pathfinder,
                // since we only care about the path existing in the first place.
                cursor.EmitDelegate<Func<bool>>(cutPathfindingShort);
                cursor.Emit(OpCodes.Brfalse, cursor.Next);
                cursor.Emit(OpCodes.Ldc_I4_1);
                cursor.Emit(OpCodes.Ret);
            }
        }

        private static bool cutPathfindingShort() {
            if (pathfindingForVariant) Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/AddSeekers", "Pathfinding is a SUCCESS");
            return pathfindingForVariant;
        }

        private static bool modIsCollidable(Entity entity, bool orig) {
            if (!pathfindingForVariant || (!(entity is DreamBlock) && !(entity is SeekerBarrier))) {
                return orig;
            }
            Level level = (Engine.Scene as Level) ?? (Engine.Scene as LevelLoader)?.Level;
            if (level == null) {
                Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/AddSeekers", $"Not ignoring {entity.GetType().FullName} because there is no level to be found...?");
                return orig;
            }
            if (entity is DreamBlock && !level.Session.Inventory.DreamDash) {
                Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/AddSeekers", $"Not ignoring {entity.GetType().FullName} because the player doesn't have the dream dash");
                return orig;
            }
            Logger.Log(LogLevel.Verbose, "ExtendedVariantMode/AddSeekers", $"Ignoring {entity.GetType().FullName} for pathfinding!");
            return false;
        }
    }
}
