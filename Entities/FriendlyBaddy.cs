using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace ExtendedVariants.Entities {
    [Tracked]
    public class FriendlyBaddy : BadelineDummy {
        public FriendlyBaddy(Vector2 position) : base(position) {
            Depth = -20000;
            Floatness = 4f;
            AddTag(Tags.Persistent);
        }

        public override void Update() {
            base.Update();

            Player player = Scene.Tracker.GetEntity<Player>();

            if (player != null) {
                Vector2 position = Position;
                Vector2 playerPosition = player.Position + new Vector2(-16f * (int) player.Facing, -16f);
                Position = position + (playerPosition - position) * (1f - (float) Math.Pow(0.0099999997764825821, Engine.DeltaTime));

                int facing = Math.Sign(player.Position.X - position.X);
                Sprite.Scale.X = facing == 0 ? 1 : facing;
            }

            if (!((bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.FriendlyBadelineFollower))) {
                Vanish();
            }
        }
    }
}
