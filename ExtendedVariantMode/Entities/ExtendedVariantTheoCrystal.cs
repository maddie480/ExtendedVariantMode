using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace ExtendedVariants.Entities {
    [Tracked(false)]
    public class ExtendedVariantTheoCrystal : TheoCrystal {

        public ExtendedVariantTheoCrystal(Vector2 position) : base(position) { }

        public override void Update() {
            Level level = SceneAs<Level>();

            // prevent the crystal from going offscreen by the right as well
            // (that's the only specificity of Extended Variant Theo Crystal.)
            if (Right > level.Bounds.Right) {
                Right = level.Bounds.Right;
                Speed.X *= -0.4f;
            }

            base.Update();

            // commit remove self if the variant is disabled mid-screen
            if(!ExtendedVariantsModule.Settings.TheoCrystalsEverywhere) {
                RemoveSelf();
            }
        }
    }
}
