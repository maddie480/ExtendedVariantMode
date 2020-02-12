using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Reflection;

namespace ExtendedVariants.Entities {
    class ExtendedVariantSandwichLava : SandwichLava {
        private static FieldInfo iceMode = typeof(SandwichLava).GetField("iceMode", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo topRect = typeof(SandwichLava).GetField("topRect", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo bottomRect = typeof(SandwichLava).GetField("bottomRect", BindingFlags.NonPublic | BindingFlags.Instance);

        private bool isIceMode;
        private bool triggeredLeave = false;

        public ExtendedVariantSandwichLava(bool isIceMode, float startX) : base(startX) {
            this.isIceMode = isIceMode;

            foreach (Component component in Components) {
                // toss the CoreModeListener so that the lava/ice doesn't depend on the core mode.
                if (component.GetType() == typeof(CoreModeListener)) {
                    Remove(component);
                    break;
                }
            }
            Add(new DashListener() {
                OnDash = OnDash
            });
        }

        private void OnDash(Vector2 dashDir) {
            isIceMode = !isIceMode;
            iceMode.SetValue(this, isIceMode);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // initialize the iceMode variable to what we asked for. Nothing else will change it afterwards.
            iceMode.SetValue(this, isIceMode);

            // initialize the Y so that the player is in the middle of the sandwich lava
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null) {
                Y = player.Y + 80f;
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // waiting is broken, lava won't center at the right position.
            Waiting = false;

            // prevent the "ease in" effect that is more confusing than anything in our case.
            LavaRect topRectVal = (LavaRect) topRect.GetValue(this);
            LavaRect bottomRectVal = (LavaRect) bottomRect.GetValue(this);
            topRectVal.Position.Y = -360f;
            bottomRectVal.Position.Y = 0f;
        }

        public override void Update() {
            base.Update();

            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null && ExtendedVariantsModule.ShouldEntitiesAutoDestroy(player) && !triggeredLeave) {
                // we should destroy lava/ice
                Leave();
                triggeredLeave = true;
            }
        }
    }
}
