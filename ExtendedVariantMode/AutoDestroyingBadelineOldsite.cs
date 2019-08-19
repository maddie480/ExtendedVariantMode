using Microsoft.Xna.Framework;

namespace Celeste.Mod.ExtendedVariants {
    class AutoDestroyingBadelineOldsite : BadelineOldsite {
        public AutoDestroyingBadelineOldsite(EntityData data, Vector2 position, int index) : base(data, position, index) { }

        public override void Update() {
            base.Update();

            Level level = SceneAs<Level>();
            Player player = level.Tracker.GetEntity<Player>();

            if (player != null && player.StateMachine.State == 11) {
                // we are in a cutscene **but not the Badeline Intro one**
                // so we should just make the chasers disappear to prevent them from killing the player mid-cutscene
                level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f, null, null);
                level.Particles.Emit(BadelineOldsite.P_Vanish, 12, Center, Vector2.One * 6f);
                RemoveSelf();
            }
        }
    }
}