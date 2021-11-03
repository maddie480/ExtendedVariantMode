using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;

namespace ExtendedVariants.Entities {
    [CustomEntity("ExtendedVariantMode/SetJumpCountTrigger")]
    public class SetJumpCountTrigger : Trigger {
        private enum Mode { Cap, Set }

        private readonly int jumpCount;
        private readonly Mode mode;


        public SetJumpCountTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            jumpCount = data.Int("jumpCount");
            mode = data.Enum("mode", Mode.Set);
        }

        public override void OnEnter(Player player) {
            JumpCount.SetJumpCount(jumpCount, mode == Mode.Cap);
        }
    }
}
