using System;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace ExtendedVariants.Variants {
    class AllStrawberriesAreGoldens : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.AllStrawberriesAreGoldens ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.AllStrawberriesAreGoldens = (value != 0);
        }

        public override void Load() {
            On.Celeste.Player.Die += onPlayerDie;
            IL.Celeste.Strawberry.Update += onStrawberryUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.Die -= onPlayerDie;
            IL.Celeste.Strawberry.Update -= onStrawberryUpdate;
        }

        private PlayerDeadBody onPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
            if(!Settings.AllStrawberriesAreGoldens) {
                return orig(self, direction, evenIfInvincible, registerDeathInStats);
            }

            // get the first following strawberry before it is detached by the orig method
            Strawberry firstStrawberry = null;
            foreach (Follower follower in self.Leader.Followers) {
                if (follower.Entity is Strawberry) {
                    firstStrawberry = (follower.Entity as Strawberry);
                    break;
                }
            }

            Level level = self.SceneAs<Level>();

            // call the orig method
            PlayerDeadBody deadBody = orig(self, direction, evenIfInvincible, registerDeathInStats);

            if(deadBody != null && !deadBody.HasGolden && firstStrawberry != null) {
                // the player is dead, doesn't have the actual golden but has a strawberry.
                // we have to do magic to make the game believe they had the golden.
                // (actually, we just do what vanilla does, but with a regular berry instead.)
                deadBody.HasGolden = true;
                deadBody.DeathAction = () => {
                    Engine.Scene = new LevelExit(LevelExit.Mode.GoldenBerryRestart, level.Session) {
                        GoldenStrawberryEntryLevel = firstStrawberry.ID.Level
                    };
                };
            }

            // proceed
            return deadBody;
        }

        private void onStrawberryUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to the first Golden flag usage: this is the one which decides how the berry gets collected
            if(cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt<Strawberry>("get_Golden"))) {
                // replace it with our own call: this will give regular berries the same collect behavior as golden ones
                Logger.Log("ExtendedVariantMode/AllStrawberriesAreGoldens", $"Patching strawberry collecting behavior at {cursor.Index} in IL code for Strawberry.Update");

                cursor.Remove();
                cursor.EmitDelegate<Func<Strawberry, bool>>(strawberryHasGoldenCollectBehavior);
            }
        }

        private bool strawberryHasGoldenCollectBehavior(Strawberry berry) {
            return berry.Golden || Settings.AllStrawberriesAreGoldens;
        }
    }
}
