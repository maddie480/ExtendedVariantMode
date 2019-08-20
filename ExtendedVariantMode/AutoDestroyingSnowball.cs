
namespace Celeste.Mod.ExtendedVariants {
    class AutoDestroyingSnowball : Snowball {
        public AutoDestroyingSnowball() : base() { }

        public override void Update() {
            base.Update();

            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (ExtendedVariantsModule.ShouldEntitiesAutoDestroy(player)) {
                // kill the snowball no matter what
                RemoveSelf();
            }
        }
    }
}
