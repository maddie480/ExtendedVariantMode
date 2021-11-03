using Celeste;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities {
    public class AutoDestroyingBadelineOldsite : BadelineOldsite {
        private bool waitingForWatchtower = false;

        public AutoDestroyingBadelineOldsite(EntityData data, Vector2 position, int index) : base(data, position, index) { }

        public override void Update() {
            // if Badeline is waiting for a watchtower, stop updating it so that she doesn't go forward.
            if (!waitingForWatchtower) {
                base.Update();

                Level level = SceneAs<Level>();
                Player player = level.Tracker.GetEntity<Player>();

                if (ExtendedVariantsModule.ShouldEntitiesAutoDestroy(player)) {
                    // we are in a cutscene **but not the Badeline Intro one**
                    // so we should just make the chasers disappear to prevent them from killing the player mid-cutscene
                    level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f, null, null);
                    level.Particles.Emit(P_Vanish, 12, Center, Vector2.One * 6f);

                    if (!BadelineChasersEverywhere.UsingWatchtower) {
                        // cutscene!
                        RemoveSelf();
                    } else {
                        // waiting for watchtower: just become invisible instead.
                        waitingForWatchtower = true;
                        Visible = false;
                    }
                }
            } else if (!BadelineChasersEverywhere.UsingWatchtower) {
                // using the watchtower is over, make Badeline appear again!
                waitingForWatchtower = false;
                Visible = true;

                SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f, null, null);
                SceneAs<Level>().Particles.Emit(P_Vanish, 12, Center, Vector2.One * 6f);
            }
        }
    }
}