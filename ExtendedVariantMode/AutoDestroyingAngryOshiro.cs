using Microsoft.Xna.Framework;

namespace Celeste.Mod.ExtendedVariants {
    class AutoDestroyingAngryOshiro : AngryOshiro {
        public AutoDestroyingAngryOshiro(Vector2 position, bool fromCutscene): base(position, fromCutscene) { }

        public override void Update() {
            base.Update();

            Level level = SceneAs<Level>();
            Player player = level.Tracker.GetEntity<Player>();

            if (player != null && player.StateMachine.State == 11) {
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
