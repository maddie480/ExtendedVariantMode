using System;
using System.Collections;
using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace ExtendedVariants.Variants {
    class BadelineBossesEverywhere : AbstractExtendedVariant {

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
            On.Celeste.Player.WindMove += modPlayerWindMove;
        }

        public override void Unload() {
            IL.Celeste.FinalBoss.CanChangeMusic -= modCanChangeMusic;
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
            IL.Celeste.FinalBoss.ctor_Vector2_Vector2Array_int_float_bool_bool_bool -= modBadelineBossConstructor;
            On.Celeste.Player.WindMove -= modPlayerWindMove;
        }

        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            // failsafe: kill the glitch effect.
            Glitch.Value = 0;

            if (Settings.BadelineBossesEverywhere && playerIntro != Player.IntroTypes.Transition) {
                injectBadelineBoss(self);
            }
        }

        private IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            // just make sure the whole transition routine is over
            IEnumerator origEnum = orig(self, next, direction);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

            if (Settings.BadelineBossesEverywhere) {
                injectBadelineBoss(self);
            }
        }

        private void injectBadelineBoss(Level level) {
            Player player = level.Tracker.GetEntity<Player>();

            if (player != null && level.Tracker.CountEntities<FinalBoss>() == 0) {
                // there is no boss in the base level ; proceed adding our own boss

                // let's do the funniest part: the boss position
                // we want it to be on the opposite side of the room compared to the player (...?)
                Vector2 playerToCenter = new Vector2(level.Bounds.Center.X - player.Position.X, level.Bounds.Center.Y - player.Position.Y);
                Vector2 bossPosition = player.Position + playerToCenter * 2; // this is the opposite of the room relative to the center.

                // failsafe: if the player happens to be in the middle of the screen, just ragequit.
                if (playerToCenter == Vector2.Zero) return;

                // we are going to try 20 positions on the line between the player and the "opposite of the room".
                bool positionFound = false;
                for (int i = 0; i < 20; i++) {
                    // the Badeline boss hitbox is a circle of a 14 pixel radius, shifted 6 pixels to the top.
                    // we want to take a 5 pixel security margin because I encountered some issues with dream blocks while testing.
                    Rectangle collisionBox = new Rectangle((int)(bossPosition.X - 19), (int)(bossPosition.Y - 25), 38, 38);
                    if (!level.CollideCheck<Solid>(collisionBox) && !level.CollideCheck<JumpThru>(collisionBox)) {
                        positionFound = true;
                        break;
                    }

                    // if the boss ends up in a wall, draw it towards the player a bit.
                    bossPosition -= playerToCenter / 10f;
                }

                if (positionFound) {
                    Vector2 bossPositionOffscreen = player.Position;
                    while(bossPositionOffscreen.X + 30 > level.Bounds.Left - 50 && bossPositionOffscreen.X < level.Bounds.Right + 50
                         && bossPositionOffscreen.Y + 30 > level.Bounds.Top - 50 && bossPositionOffscreen.Y < level.Bounds.Bottom + 50) {

                        // push the position until it is offscreen.
                        bossPositionOffscreen += playerToCenter;
                    }

                    EntityData bossData = ExtendedVariantsModule.GenerateBasicEntityData(level, 15); // 0 to 9 are Badeline chasers, 10 to 14 are seekers.
                    bossData.Position = bossPosition;
                    bossData.Values["canChangeMusic"] = false;
                    bossData.Values["cameraLockY"] = false;
                    bossData.Values["patternIndex"] = Settings.BadelineAttackPattern == 0 ? patternRandomizer.Next(1, 16) : Settings.BadelineAttackPattern;
                    bossData.Nodes = new Vector2[] { bossPositionOffscreen }; // if / when hit, the boss will fly offscreen.

                    level.Add(new AutoDestroyingBadelineBoss(bossData, Vector2.Zero));
                    level.Entities.UpdateLists();
                }
            }
        }
        
        private void modCanChangeMusic(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go right after the equality check that compares the level set name with "Celeste"
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<string>("op_Equality"))) {
                Logger.Log("ExtendedVariantsModule", $"Modding vanilla level check at index {cursor.Index} in the CanChangeMusic method from FinalBoss");

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

            if(cursor.TryGotoNext(instr => instr.MatchStfld<FinalBoss>("patternIndex"))) {
                Logger.Log("ExtendedVariantsModule", $"Modding Badeline Boss patterns at {cursor.Index} in IL code for the FinalBoss constructor");

                cursor.EmitDelegate<Func<int, int>>(modAttackPattern);
            }
        }

        private int modAttackPattern(int vanillaPattern) {
            if(Settings.ChangePatternsOfExistingBosses) {
                return Settings.BadelineAttackPattern == 0 ? patternRandomizer.Next(1, 16) : Settings.BadelineAttackPattern;
            }
            return vanillaPattern;
        }

        private void modPlayerWindMove(On.Celeste.Player.orig_WindMove orig, Player self, Vector2 move) {
            // the Attract state doesn't cope well with wind **at all**.
            // that creates a softlock when the player gets pushed away from the attract target indefinitely.
            // so, just don't apply wind when in the Attract state.
            if (self.StateMachine.State != 22) {
                orig(self, move);
            }
        }
    }
}
