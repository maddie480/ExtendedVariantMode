using Celeste;
using Monocle;

namespace ExtendedVariants.Entities {

    class ExtendedVariantWindSnowFG : WindSnowFG {

        public override void Update(Scene scene) {
            // update the alpha with the same fade as vanilla (except we can't just override IsVisible)
            Alpha = Calc.Approach(Alpha, isVisible(scene as Level) ? 1 : 0, Engine.DeltaTime * 2f);

            base.Update(scene);
        }

        // This backdrop should be visible whenever no WindSnowFG or StardustFG is visible.
        private bool isVisible(Level level) {
            return level.Foreground.Backdrops.TrueForAll(backdrop => {
                if (backdrop.GetType() == typeof(WindSnowFG) || backdrop.GetType() == typeof(StardustFG)) {
                    return !backdrop.IsVisible(level);
                }
                return true;
            });
        }
    }
}
