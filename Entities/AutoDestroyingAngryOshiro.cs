using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;

namespace ExtendedVariants.Entities {
    [TrackedAs(typeof(AngryOshiro))]
    public class AutoDestroyingAngryOshiro : AngryOshiro {
        // cached accessor for AngryOshiro's "state" private field.
        private static FieldInfo stateMachine = typeof(AngryOshiro).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic);

        private const int StWaitingOffset = 9;

        private float waitTimer;
        private bool playerMoved = false;

        public AutoDestroyingAngryOshiro(Vector2 position, bool fromCutscene, float offsetTime) : base(position, fromCutscene) {
            // bump Oshiro up so that he goes over FakeWalls
            Depth = -13500;

            waitTimer = offsetTime;

            state.SetCallbacks(StWaitingOffset, WaitingOffsetUpdate);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // if the state is Waiting and Oshiro has an offset, hijack the state to take our own instead.
            if (state.State == 4 && waitTimer > 0f)
                state.State = StWaitingOffset;
        }

        private int WaitingOffsetUpdate() {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && player.Speed != Vector2.Zero) playerMoved = true;

            if (player != null && playerMoved && player.X > level.Bounds.Left + 48) {
                // vanilla Oshiro would charge. we want to wait for waitTimer to deplete first.
                waitTimer -= Engine.DeltaTime;
                if (waitTimer <= 0f) {
                    // timer depleted, proceed to Chase state
                    return 0;
                }
            }
            return StWaitingOffset;
        }

        public override void Update() {
            base.Update();

            Player player = level.Tracker.GetEntity<Player>();

            if (ExtendedVariantsModule.ShouldEntitiesAutoDestroy(player)) {
                // during cutscenes, tell Oshiro to get outta here the same way as Badeline
                // (we can't use the "official" way of making him leave because that doesn't cancel his attack.
                // A Badeline vanish animation looks weird but nicer than a flat out disappearance imo)
                level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f, null, null);
                level.Particles.Emit(BadelineOldsite.P_Vanish, 12, Center, Vector2.One * 6f);
                RemoveSelf();

                // make sure that the anxiety set by Oshiro went away (why doesn't that work in real life tho)
                Distort.Anxiety = 0;
                Distort.GameRate = 1;
            }
        }
    }
}
