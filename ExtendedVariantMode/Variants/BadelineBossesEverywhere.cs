using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ExtendedVariants.Variants {
    class BadelineBossesEverywhere : AbstractExtendedVariant {

        private Random positionRandomizer = new Random();
        private Random patternRandomizer = new Random();

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.BadelineBossesEverywhere ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.BadelineBossesEverywhere = (value != 0);
        }

        public override void Load() {
            IL.Celeste.FinalBoss.CanChangeMusic += modCanChangeMusic;
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;
            IL.Celeste.FinalBoss.ctor_Vector2_Vector2Array_int_float_bool_bool_bool += modBadelineBossConstructor;
        }

        public override void Unload() {
            IL.Celeste.FinalBoss.CanChangeMusic -= modCanChangeMusic;
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
            IL.Celeste.FinalBoss.ctor_Vector2_Vector2Array_int_float_bool_bool_bool -= modBadelineBossConstructor;
        }

        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            // failsafe: kill the glitch effect.
            Glitch.Value = 0;

            if (Settings.BadelineBossesEverywhere && playerIntro != Player.IntroTypes.Transition) {
                injectBadelineBosses(self);
            }
        }

        private IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            // just make sure the whole transition routine is over
            IEnumerator origEnum = orig(self, next, direction);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

            if (Settings.BadelineBossesEverywhere) {
                injectBadelineBosses(self);
            }
        }

        private void injectBadelineBosses(Level level) {
            Player player = level.Tracker.GetEntity<Player>();

            if (player != null) {
                for (int id = level.Tracker.CountEntities<FinalBoss>(); id < Settings.BadelineBossCount; id++) {
                    // let's add a boss

                    Vector2 bossPosition;
                    if (id == 0 && !Settings.FirstBadelineSpawnRandom) {
                        // the first Badeline should spawn at the opposite of the room
                        bossPosition = computeBossPositionAtOppositeOfPlayer(level, player);
                    } else {
                        // all the others should spawn at random points
                        bossPosition = computeBossPositionAtRandom(level, player);
                    }

                    // position not found, abort!
                    if (bossPosition == Vector2.Zero) break;

                    Vector2 penultimateNode = player.Position;
                    Vector2 lastNode = bossPosition;

                    Vector2[] nodes = new Vector2[Settings.BadelineBossNodeCount];

                    for (int i = 0; i < Settings.BadelineBossNodeCount - 1; i++) {
                        // randomize all nodes, except the last one.
                        nodes[i] = computeBossPositionAtRandom(level, player);

                        penultimateNode = lastNode;
                        lastNode = nodes[i];

                        // position not found, don't process other nodes!
                        if (lastNode == Vector2.Zero) break;
                    }

                    // position not found, stop adding bosses!
                    if (lastNode == Vector2.Zero) break;

                    if (bossPosition != Vector2.Zero) {
                        Vector2 bossPositionOffscreen = penultimateNode;

                        Vector2 lastMoveDirection = (lastNode - penultimateNode);

                        // extremely unlikely, but better safe than sorry
                        if (lastMoveDirection == Vector2.Zero) lastMoveDirection = Vector2.One;

                        while (bossPositionOffscreen.X + 30 > level.Bounds.Left - 50 && bossPositionOffscreen.X < level.Bounds.Right + 50
                             && bossPositionOffscreen.Y + 30 > level.Bounds.Top - 50 && bossPositionOffscreen.Y < level.Bounds.Bottom + 50) {

                            // push the position until it is offscreen, using the same direction as the last move (or the player => boss direction if there is no node).
                            bossPositionOffscreen += lastMoveDirection;
                        }

                        // the offscreen position has to be the last node.
                        nodes[nodes.Length - 1] = bossPositionOffscreen;

                        // build the boss
                        EntityData bossData = ExtendedVariantsModule.GenerateBasicEntityData(level, 15 + id); // 0 to 9 are Badeline chasers, 10 to 14 are seekers.
                        bossData.Position = bossPosition;
                        bossData.Values["canChangeMusic"] = false;
                        bossData.Values["cameraLockY"] = false;
                        bossData.Values["patternIndex"] = Settings.BadelineAttackPattern == 0 ? patternRandomizer.Next(1, 16) : Settings.BadelineAttackPattern;
                        bossData.Nodes = nodes;

                        // add it to the level!
                        level.Add(new AutoDestroyingBadelineBoss(bossData, Vector2.Zero));
                    }
                }

                level.Entities.UpdateLists();
            }
        }

        private Vector2 computeBossPositionAtOppositeOfPlayer(Level level, Player player) {
            // we want the position to be on the opposite side of the room compared to the player (...?)
            Vector2 playerToCenter = new Vector2(level.Bounds.Center.X - player.Position.X, level.Bounds.Center.Y - player.Position.Y);
            Vector2 bossPosition = player.Position + playerToCenter * 2; // this is the opposite of the room relative to the center.

            // failsafe: if the player happens to be in the middle of the screen, just ragequit.
            if (playerToCenter == Vector2.Zero) return Vector2.Zero;

            // we are going to try 20 positions on the line between the player and the "opposite of the room".
            for (int i = 0; i < 20; i++) {
                // the Badeline boss hitbox is a circle of a 14 pixel radius, shifted 6 pixels to the top.
                // we want to take a 5 pixel security margin because I encountered some issues with dream blocks while testing.
                Rectangle collisionBox = new Rectangle((int) (bossPosition.X - 19), (int) (bossPosition.Y - 25), 38, 38);

                if (!level.CollideCheck<Solid>(collisionBox) && !level.CollideCheck<JumpThru>(collisionBox)) {
                    return bossPosition;
                }

                // if the boss ends up in a wall, draw it towards the player a bit.
                bossPosition -= playerToCenter / 10f;
            }

            Logger.Log(LogLevel.Warn, "ExtendedVariantMode/BadelineBossesEverywhere", "Could not find boss position at opposite of room! Aborting.");
            return Vector2.Zero;
        }

        private Vector2 computeBossPositionAtRandom(Level level, Player player) {
            for (int i = 0; i < 100; i++) {
                // roll a boss position in the room
                int x = positionRandomizer.Next(level.Bounds.Width) + level.Bounds.X;
                int y = positionRandomizer.Next(level.Bounds.Height) + level.Bounds.Y;

                // should be at least 100 pixels from the player
                double playerDistance = Math.Sqrt(Math.Pow(MathHelper.Distance(x, player.X), 2) + Math.Pow(MathHelper.Distance(y, player.Y), 2));

                // check if it collides with a solid
                Rectangle collideRectangle = new Rectangle(x - 19, y - 25, 38, 38);
                if (playerDistance > 100 && !level.CollideCheck<Solid>(collideRectangle) && !level.CollideCheck<JumpThru>(collideRectangle)) {
                    // found it!
                    return new Vector2(x, y);
                }
            }

            Logger.Log(LogLevel.Warn, "ExtendedVariantMode/BadelineBossesEverywhere", "Could not find boss position! Aborting.");
            return Vector2.Zero;
        }

        private void modCanChangeMusic(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go right after the equality check that compares the level set name with "Celeste"
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<string>("op_Equality"))) {
                Logger.Log("ExtendedVariantMode/BadelineBossesEverywhere", $"Modding vanilla level check at index {cursor.Index} in the CanChangeMusic method from FinalBoss");

                // mod the result of that check to always use modded value, even in vanilla levels, on A-sides
                cursor.EmitDelegate<Func<bool, bool>>(modVanillaBehaviorCheckForMusic);
            }
        }

        private bool modVanillaBehaviorCheckForMusic(bool shouldUseVanilla) {
            // we can use the "flag-based behavior" on all A-sides
            if (Engine.Scene.GetType() == typeof(Level) && (Engine.Scene as Level).Session.Area.Mode == AreaMode.Normal) {
                return false;
            }
            // fall back to standard Everest behavior everywhere else: vanilla will not trigger chase music, and Everest will be flag-based
            return shouldUseVanilla;
        }

        private void modBadelineBossConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(instr => instr.MatchStfld<FinalBoss>("patternIndex"))) {
                Logger.Log("ExtendedVariantMode/BadelineBossesEverywhere", $"Modding Badeline Boss patterns at {cursor.Index} in IL code for the FinalBoss constructor");

                cursor.EmitDelegate<Func<int, int>>(modAttackPattern);
            }
        }

        private int modAttackPattern(int vanillaPattern) {
            if (Settings.ChangePatternsOfExistingBosses) {
                return Settings.BadelineAttackPattern == 0 ? patternRandomizer.Next(1, 16) : Settings.BadelineAttackPattern;
            }
            return vanillaPattern;
        }
    }
}
