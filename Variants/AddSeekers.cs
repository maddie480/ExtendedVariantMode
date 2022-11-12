using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using System.Linq;

namespace ExtendedVariants.Variants {
    public class AddSeekers : AbstractExtendedVariant {

        private Random randomGenerator = new Random();

        private bool killSeekerSlowdownToFixHeart = false;

        public override Type GetVariantType() {
            return typeof(int);
        }

        public override object GetDefaultVariantValue() {
            return 0;
        }

        public override object GetVariantValue() {
            return Settings.AddSeekers;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.AddSeekers = (int) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.AddSeekers = value;
        }

        public override void SetRandomSeed(int seed) {
            randomGenerator = new Random(seed);
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            IL.Celeste.SeekerEffectsController.Update += onSeekerEffectsControllerUpdate;
            On.Celeste.HeartGem.Collect += onHeartGemCollect;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            IL.Celeste.SeekerEffectsController.Update -= onSeekerEffectsControllerUpdate;
            On.Celeste.HeartGem.Collect -= onHeartGemCollect;

            killSeekerSlowdownToFixHeart = false;
        }

        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            // if we killed the slowdown earlier, stop now!
            killSeekerSlowdownToFixHeart = false;

            Level level = self;
            Player player = level.Tracker.GetEntity<Player>();

            if (player != null && Settings.AddSeekers != 0) {
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

                if (playerIntro != Player.IntroTypes.Transition) {
                    level.Entities.UpdateLists();
                }
            }
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
            return Settings.DisableSeekerSlowdown || killSeekerSlowdownToFixHeart ? Engine.TimeRate : vanillaTimeRate;
        }

        private void onHeartGemCollect(On.Celeste.HeartGem.orig_Collect orig, HeartGem self, Player player) {
            orig(self, player);

            // prevent seekers from slowing down time!
            if (self.Scene.Entities.OfType<AutoDestroyingSeeker>().Count() != 0) {
                killSeekerSlowdownToFixHeart = true;
            }
        }
    }
}
