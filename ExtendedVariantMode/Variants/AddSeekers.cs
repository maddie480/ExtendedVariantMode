using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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
            IL.Celeste.Pathfinder.ctor += modPathfinderConstructor;
            Everest.Events.Level.OnExit += onLevelExit;
            IL.Celeste.SeekerEffectsController.Update += onSeekerEffectsControllerUpdate;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            IL.Celeste.Pathfinder.ctor -= modPathfinderConstructor;
            Everest.Events.Level.OnExit -= onLevelExit;
            IL.Celeste.SeekerEffectsController.Update -= onSeekerEffectsControllerUpdate;
        }
        
        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            Level level = self;
            Player player = level.Tracker.GetEntity<Player>();
                
            if(player != null && Settings.AddSeekers != 0) {
                // first of all, ensure that the size is within the limits of the pathfinder
                if (level.Bounds.Width / 8 > 768 || level.Bounds.Height / 8 > 578) {
                    Logger.Log(LogLevel.Warn, "ExtendedVariantMode", $"Not spawning seekers since room exceeds max size of 659x407 tiles. ({level.Bounds.Width / 8}x{level.Bounds.Height / 8})");
                    return;
                }

                // ensure the pathfinder is the right one (i.e. the extended one)
                if (!extendedPathfinder) {
                    extendedPathfinder = true;
                    level.Pathfinder = new Pathfinder(level);
                }

                for(int seekerCount = 0; seekerCount < Settings.AddSeekers; seekerCount++) {
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

                Logger.Log("ExtendedVariantsModule", $"Modding size of pathfinder array at {cursor.Index} in CIL code for the Pathfinder constructor");

                // we will resize the pathfinder (provided that the seekers everywhere variant is enabled) to fit all rooms in vanilla Celeste
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Pathfinder, int>>(determinePathfinderWidth);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Pathfinder, int>>(determinePathfinderHeight);
            }
        }

        private int determinePathfinderWidth(Pathfinder self) {
            if(extendedPathfinder) {
                return 768;
            }
            return 200;
        }

        private int determinePathfinderHeight(Pathfinder self) {
            if (extendedPathfinder) {
                return 578;
            }
            return 200;
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            extendedPathfinder = false;
        }

        private void onSeekerEffectsControllerUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // let's jump to Engine.TimeRate = Calc.Approach(Engine.TimeRate, target, 4f * Engine.DeltaTime);
            if (cursor.TryGotoNext(instr => instr.MatchLdcR4(4f))) {
                Logger.Log("ExtendedVariantsModule", $"Adding condition for time control at {cursor.Index} in CIL code for SeekerEffectsController.Update");

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
