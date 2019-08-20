using Microsoft.Xna.Framework;

namespace Celeste.Mod.ExtendedVariants {
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
