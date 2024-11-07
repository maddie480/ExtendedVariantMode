using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class ZoomLevel : AbstractExtendedVariant {
        private Vector2 previousDiff;
        private float transitionPercent = 1f;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            On.Celeste.Player.ctor += onPlayerConstructor;
            IL.Celeste.Level.Render += modLevelRender;
        }

        public override void Unload() {
            On.Celeste.Player.ctor -= onPlayerConstructor;
            IL.Celeste.Level.Render -= modLevelRender;
        }

        private void onPlayerConstructor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);

            // make the player spy on transitions
            self.Add(new TransitionListener {
                OnOutBegin = () => transitionPercent = 0f,
                OnOut = percent => transitionPercent = percent
            });
        }

        private void modLevelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Level>("Zoom"))) {
                Logger.Log("ExtendedVariantMode/ZoomLevel", $"Modding zoom at {cursor.Index} in IL code for Level.Render");
                cursor.EmitDelegate<Func<float, float>>(modZoom);
            }
        }

        private float modZoom(float zoom) {
            return zoom * GetVariantValue<float>(Variant.ZoomLevel);
        }

        public Vector2 getScreenPosition(Vector2 originalPosition) {
            if (GetVariantValue<float>(Variant.ZoomLevel) == 1f) {
                // nothing to do, spare us some processing.
                return originalPosition;
            }

            // compute the size difference between regular screen and zoomed in screen
            Vector2 screenSize = new Vector2(GameplayWidth, GameplayHeight) * GetVariantValue<float>(Variant.ZoomLevel);
            Vector2 diff = screenSize - new Vector2(GameplayWidth, GameplayHeight);

            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (GetVariantValue<float>(Variant.ZoomLevel) > 1f && player != null) {
                // if the player is on the left of the screen, we shouldn't move the screen (left is aligned with left side of the screen).
                // if they are on the right, we should move it left by the difference (right is aligned with right side of the screen).
                // in between, just lerp
                diff *= new Vector2(
                    Calc.ClampedMap(player.CenterX, (Engine.Scene as Level).Bounds.Left, (Engine.Scene as Level).Bounds.Right),
                    Calc.ClampedMap(player.CenterY, (Engine.Scene as Level).Bounds.Top, (Engine.Scene as Level).Bounds.Bottom));
            } else {
                // no player, or < 1x zoom: center the screen.
                diff *= 0.5f;
            }

            if (player == null || player.Dead) {
                // no player: no target, don't move
                diff = previousDiff;
            } else if (transitionPercent == 1) {
                // save the position in case we're transitioning later
                previousDiff = diff;
            } else {
                // lerp in the same way transitions do, synchronized with the transition: this allows for a seemless realignment.
                diff = Vector2.Lerp(previousDiff, diff, Ease.CubeOut(transitionPercent));
            }

            return originalPosition - diff;
        }
    }
}
