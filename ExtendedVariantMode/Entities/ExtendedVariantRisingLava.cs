using Celeste;
using ExtendedVariants.Module;
using Monocle;
using System.Reflection;

namespace ExtendedVariants.Entities {
    class ExtendedVariantRisingLava : RisingLava {
        private static FieldInfo iceMode = typeof(RisingLava).GetField("iceMode", BindingFlags.NonPublic | BindingFlags.Instance);

        public ExtendedVariantRisingLava() : base(false) {
            foreach(Component component in Components) {
                // toss the CoreModeListener so that the lava/ice doesn't depend on the core mode.
                if (component.GetType() == typeof(CoreModeListener)) {
                    Remove(component);
                    break;
                }
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // initialize the iceMode variable to false. Nothing else will change it afterwards.
            iceMode.SetValue(this, false);
        }
        
        public override void Update() {
            base.Update();

            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (ExtendedVariantsModule.ShouldEntitiesAutoDestroy(player)) {
                // we should destroy lava/ice
                RemoveSelf();
            }
        }
    }
}
