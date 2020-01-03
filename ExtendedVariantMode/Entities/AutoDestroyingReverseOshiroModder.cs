using Celeste;
using Celeste.Mod.DJMapHelper.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;

namespace ExtendedVariants.Entities {
    // we can't extend AngryOshiroRight for a thousand reasons. So, we'll make a component that will mess around with the AngryOshiroRight, and attach that to the ones we spawn.
    class AutoDestroyingReverseOshiroModder : Component {
        // cached accessor for AngryOshiroRight's "state" private field.
        private static FieldInfo stateMachine = typeof(AngryOshiroRight).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic);

        private const int StWaitingOffset = 9;

        private float offsetTime;

        private Level level;
        private StateMachine state;
        private float waitTimer;
        private bool playerMoved = false;

        public AutoDestroyingReverseOshiroModder(float offsetTime): base(true, false) {
            this.offsetTime = offsetTime;
        }

        public override void Added(Entity entity) {
            base.Added(entity);

            // bump Oshiro up so that he goes over FakeWalls
            entity.Depth = -13500;

            state = (StateMachine)stateMachine.GetValue(entity);
            waitTimer = offsetTime;

            state.SetCallbacks(StWaitingOffset, WaitingOffsetUpdate);
        }

        public override void EntityAdded(Scene scene) {
            base.EntityAdded(scene);

            level = (Level) scene;
        }

        private int WaitingOffsetUpdate() {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && player.Speed != Vector2.Zero) playerMoved = true;

            if (player != null && playerMoved && player.X > level.Bounds.Left + 48) {
                // vanilla Oshiro would charge. we want to wait for waitTimer to deplete first.
                waitTimer -= Engine.DeltaTime;
                if(waitTimer <= 0f) {
                    // timer depleted, proceed to Chase state
                    return 0;
                }
            }
            return StWaitingOffset;
        }

        public override void Update() {
            base.Update();

            // if the state is Waiting and Oshiro has an offset, hijack the state to take our own instead.
            if (state.State == 4 && waitTimer > 0f)
                state.State = StWaitingOffset;

            Level level = SceneAs<Level>();
            Player player = level.Tracker.GetEntity<Player>();

            if (ExtendedVariantsModule.ShouldEntitiesAutoDestroy(player)) {
                // during cutscenes, tell Oshiro to get outta here the same way as Badeline
                // (we can't use the "official" way of making him leave because that doesn't cancel his attack.
                // A Badeline vanish animation looks weird but nicer than a flat out disappearance imo)
                level.Displacement.AddBurst(Entity.Center, 0.5f, 24f, 96f, 0.4f, null, null);
                level.Particles.Emit(BadelineOldsite.P_Vanish, 12, Entity.Center, Vector2.One * 6f);
                Entity.RemoveSelf();

                // make sure that the anxiety set by Oshiro went away (why doesn't that work in real life tho)
                Distort.Anxiety = 0;
                Distort.GameRate = 1;
            }
        }
    }
}
