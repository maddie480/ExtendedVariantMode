using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;

namespace ExtendedVariants.Variants {
    public class AddSeekers : AbstractExtendedVariant {

        private Random randomGenerator = new Random();

        private bool extendedPathfinder = false;

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.AddSeekers;
        }

        public override void SetValue(int value) {
            Settings.AddSeekers = value;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            Everest.Events.Level.OnExit += onLevelExit;
            IL.Celeste.SeekerEffectsController.Update += onSeekerEffectsControllerUpdate;

            // by ensuring our IL hook comes first, we ensure that the patch will work even with Crow Control,
            // which uses (almost) the exact same patch. Crow Control will inject itself before us, and we will either
            // let its modded values pass through, or make them bigger.
            using (new DetourContext {
                Before = { "*" }
            }) {
                IL.Celeste.Pathfinder.ctor += modPathfinderConstructor;
            }
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            IL.Celeste.Pathfinder.ctor -= modPathfinderConstructor;
            Everest.Events.Level.OnExit -= onLevelExit;
            IL.Celeste.SeekerEffectsController.Update -= onSeekerEffectsControllerUpdate;

            // if disabled during a level, we have to plug out the extended pathfinder right away.
            if(extendedPathfinder && Engine.Scene.GetType() == typeof(Level)) {
                Level level = Engine.Scene as Level;

                extendedPathfinder = false;
                level.Pathfinder = new Pathfinder(level);
            }
        }
        
        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            Level level = self;
            Player player = level.Tracker.GetEntity<Player>();
                
            if(player != null && Settings.AddSeekers != 0) {
                // first of all, ensure that the size is within the limits of the pathfinder
                if (level.Bounds.Width / 8 > 768 || level.Bounds.Height / 8 > 578) {
                    Logger.Log(LogLevel.Warn, "ExtendedVariantMode/AddSeekers", $"Not spawning seekers since room exceeds max size of 659x407 tiles. ({level.Bounds.Width / 8}x{level.Bounds.Height / 8})");
                    return;
                }

                // ensure the pathfinder is the right one (i.e. the extended one)
                if (!extendedPathfinder) {
                    extendedPathfinder = true;
                    level.Pathfinder = new Pathfinder(level);
                }

                // make the seeker barriers temporarily collidable so that they are taken in account in Solid collide checks
                // and seekers can't spawn in them
                // (... yes, this is also what vanilla does in the seekers' Update method.)
                foreach (Entity entity in self.Tracker.GetEntities<SeekerBarrier>()) entity.Collidable = true;

                for (int seekerCount = 0; seekerCount < Settings.AddSeekers; seekerCount++) {
                    for (int i = 0; i < 100; i++) {
                        // roll a seeker position in the room
                        int x = randomGenerator.Next(level.Bounds.Width) + level.Bounds.X;
                        int y = randomGenerator.Next(level.Bounds.Height) + level.Bounds.Y;

                        // should be at least 100 pixels from the player
                        double playerDistance = Math.Sqrt(Math.Pow(MathHelper.Distance(x, player.X), 2) + Math.Pow(MathHelper.Distance(y, player.Y), 2));

                        // also check if we are not spawning in a wall, that would be a shame
                        Rectangle collideRectangle = new Rectangle(x - 8, y - 8, 16, 16);
                        if (playerDistance > 100 && !level.CollideCheck<Solid>(collideRectangle) && !level.CollideCheck<Seeker>(collideRectangle)) {
                            // build a Seeker with a proper EntityID to make Speedrun Tool happy (this is useless in vanilla Celeste but the constructor call is intercepted by Speedrun Tool)
                            EntityData seekerData = ExtendedVariantsModule.GenerateBasicEntityData(level, 10 + seekerCount);
                            seekerData.Position = new Vector2(x, y);
                            Seeker seeker = new AutoDestroyingSeeker(seekerData, Vector2.Zero);
                            level.Add(seeker);
                            break;
                        }
                    }
                }

                foreach (Entity entity in self.Tracker.GetEntities<SeekerBarrier>()) entity.Collidable = false;

                level.Entities.UpdateLists();
            } else if(Settings.AddSeekers == 0 && extendedPathfinder) {
                extendedPathfinder = false;
                level.Pathfinder = new Pathfinder(level);
            }
        }

        private void modPathfinderConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go everywhere where the 0.8 second delay is defined
            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdcI4(200),
                instr => instr.MatchLdcI4(200))) {

                Logger.Log("ExtendedVariantMode/AddSeekers", $"Modding size of pathfinder array at {cursor.Index} in CIL code for the Pathfinder constructor");

                // we will resize the pathfinder (provided that the seekers everywhere variant is enabled) to fit all rooms in vanilla Celeste
                cursor.EmitDelegate<Action<int>>(storeLocal);
                cursor.EmitDelegate<Func<int, int>>(determinePathfinderWidth);
                cursor.EmitDelegate<Func<int>>(restoreLocal);
                cursor.EmitDelegate<Func<int, int>>(determinePathfinderHeight);
            }
        }

        // begin utility method to temporarily hold int in local variable
        private int local;

        private void storeLocal(int variable) {
            local = variable;
        }

        private int restoreLocal() {
            return local;
        }
        // end utility method

        private int determinePathfinderWidth(int vanilla) {
            if (extendedPathfinder) {
                return Math.Max(768, vanilla);
            }
            return vanilla; // vanilla is either 200 or whatever Crow Control decided.
        }

        private int determinePathfinderHeight(int vanilla) {
            if (extendedPathfinder) {
                return Math.Max(578, vanilla);
            }
            return vanilla; // vanilla is either 200 or whatever Crow Control decided.
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            extendedPathfinder = false;
        }

        private void onSeekerEffectsControllerUpdate(ILContext il) {
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

        private float transformTimeRate(float vanillaTimeRate) {
            return Settings.DisableSeekerSlowdown ? 1f : vanillaTimeRate;
        }
    }
}
