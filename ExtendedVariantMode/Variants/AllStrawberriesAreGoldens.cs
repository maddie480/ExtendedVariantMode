using System;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
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
            IL.Celeste.Strawberry.Added += onStrawberryAdded;
        }

        public override void Unload() {
            On.Celeste.Player.Die -= onPlayerDie;
            IL.Celeste.Strawberry.Update -= onStrawberryUpdate;
            IL.Celeste.Strawberry.Added -= onStrawberryAdded;
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
                Logger.Log("ExtendedVariantMode", $"Patching strawberry collecting behavior at {cursor.Index} in IL code for Strawberry.Update");

                cursor.Remove();
                cursor.EmitDelegate<Func<Strawberry, bool>>(strawberryHasGoldenCollectBehavior);
            }
        }

        private bool strawberryHasGoldenCollectBehavior(Strawberry berry) {
            return berry.Golden || Settings.AllStrawberriesAreGoldens;
        }
        

        private void onStrawberryAdded(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(instr => instr.MatchStfld<Strawberry>("light"))
                && cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Entity>("Add"))) {

                Logger.Log("ExtendedVariantMode", $"Injecting strawberry light killing code at {cursor.Index} in IL code for Strawberry.Added");

                // just after adding the light entity, inject a call removing it if AllStrawberriesAreGoldens is enabled.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(Strawberry).GetField("light", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                cursor.EmitDelegate<Action<VertexLight>>(killStrawberryLight);
            }
        }

        private void killStrawberryLight(VertexLight berryLight) {
            // we want to kill the berries' light sources, because it's known to cause crashes if there are too many on screen.
            if(Settings.AllStrawberriesAreGoldens) {
                Logger.Log("EVM", "Killing light");
                berryLight.RemoveSelf();
            }
        }
    }
}
