using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities {
    public class AutoDestroyingBadelineBoss : FinalBoss {
        public AutoDestroyingBadelineBoss(EntityData e, Vector2 offset) : base(e, offset) {
            // ... I don't want to lock the camera though
            CameraLocker cameraLocker = Components.Get<CameraLocker>();
            Components.Remove(cameraLocker);
        }

        public override void Update() {
            base.Update();

            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (ExtendedVariantsModule.ShouldEntitiesAutoDestroy(player)) {
                // kill the boss no matter what
                RemoveSelf();

                // and kill all the beams and shots as well, they could still kill the player!
                foreach (FinalBossShot shot in SceneAs<Level>().Tracker.GetEntities<FinalBossShot>()) {
                    shot.Destroy();
                }
                foreach (FinalBossBeam beam in SceneAs<Level>().Tracker.GetEntities<FinalBossBeam>()) {
                    beam.Destroy();
                }
            }
        }
    }
}
