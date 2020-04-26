using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace ExtendedVariants.Entities {
    [TrackedAs(typeof(Seeker))]
    class AutoDestroyingSeeker : Seeker {
        public AutoDestroyingSeeker(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void Update() {
            base.Update();

            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (ExtendedVariantsModule.ShouldEntitiesAutoDestroy(player)) {
                RemoveSelf();
            }
        }
    }
}
