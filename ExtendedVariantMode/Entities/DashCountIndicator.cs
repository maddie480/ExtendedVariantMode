using Celeste;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;

namespace ExtendedVariants.Entities {
    /// <summary>
    /// An indicator for the dash count (numbers above Madeline's head).
    /// </summary>
    class DashCountIndicator : Entity {
        private static MTexture[] numbers;
        private ExtendedVariantsSettings settings;

        public static void Initialize() {
            // extract numbers from the PICO-8 font that ships with the game.
            MTexture source = GFX.Game["pico8/font"];
            numbers = new MTexture[10];
            int index = 0;
            for (int i = 104; index < 4; i += 4) {
                numbers[index++] = source.GetSubtexture(i, 0, 3, 5);
            }
            for (int i = 0; index < 10; i += 4) {
                numbers[index++] = source.GetSubtexture(i, 6, 3, 5);
            }
        }

        public DashCountIndicator() {
            Depth = (Depths.FGTerrain + Depths.FGDecals) / 2; // between fg tiles and fg decals
            AddTag(Tags.Persistent); // this entity isn't bound to the screen it was spawned in, keep it when transitioning to another level.

            settings = ExtendedVariantsModule.Settings;
        }

        public override void Update() {
            base.Update();

            if (!settings.MasterSwitch) {
                // extended variants were turned off, the dash count indicator should kick itself out.
                RemoveSelf();
            }
        }

        public override void Render() {
            if (!settings.DisplayDashCount) {
                // hide the dash count.
                return;
            }

            base.Render();

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                // compute the jump count so that we can put the dash count above it.
                int jumpIndicatorsToDraw = settings.JumpCount == 6 ? 0 : JumpCount.GetJumpBuffer();
                int jumpCountLines = jumpIndicatorsToDraw == 0 ? 0 : 1 + (jumpIndicatorsToDraw - 1) / 5;

                // draw Madeline's dash count, digit by digit.
                string dashCount = player.Dashes.ToString();
                int totalWidth = dashCount.Length * 4 - 1;
                for (int i = 0; i < dashCount.Length; i++) {
                    numbers[dashCount.ToCharArray()[i] - '0'].DrawOutline(player.Center + new Vector2(-totalWidth / 2 + i * 4, -18f - jumpCountLines * 6f), new Vector2(0f, 0.5f));
                }
            }
        }
    }
}
