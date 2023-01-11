using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;

namespace ExtendedVariants.Entities {
    /// <summary>
    /// An indicator for extra jumps (dots above Madeline's head).
    /// </summary>
    [Tracked]
    public class JumpIndicator : Entity {
        private const int normalDepth = (Depths.FGTerrain + Depths.FGDecals) / 2; // between fg tiles and fg decals
        private const int depthInFrontOfSolids = Depths.FakeWalls - 1; // in front of fake walls

        private static Type strawberrySeedIndicator = null;

        public static void Initialize() {
            // extract a reference to the Max Helping Hand strawberry seed indicator.
            EverestModule mod = Everest.Modules.FirstOrDefault(m => m.Metadata?.Name == "MaxHelpingHand");
            if (mod != null) {
                strawberrySeedIndicator = mod.GetType().Assembly.GetType("Celeste.Mod.MaxHelpingHand.Entities.MultiRoomStrawberryCounter");
            }
        }

        private ExtendedVariantsSettings settings;
        private float invisibleJumpCountTimer = 0f;

        public JumpIndicator() {
            Depth = (Depths.FGTerrain + Depths.FGDecals) / 2; // between fg tiles and fg decals
            AddTag(Tags.Persistent); // this entity isn't bound to the screen it was spawned in, keep it when transitioning to another level.

            settings = ExtendedVariantsModule.Settings;
        }

        public override void Update() {
            base.Update();

            if (!settings.MasterSwitch) {
                // extended variants were turned off, the jump indicator should kick itself out.
                RemoveSelf();
            }

            // if the indicator overlaps with a solid that's in front of everything (exit blocks etc), send it in front of them.
            // otherwise, have it between fg decals and fg tiles
            if (Collider != null && CollideAll<Solid>().Any(solid => solid.Depth < normalDepth)) {
                Depth = depthInFrontOfSolids;
            } else {
                Depth = normalDepth;
            }

            // hide jump count when infinite jumps are active, and show them again 0.1 second after turning infinite jumps off.
            // this avoids showing the jump count briefly on a transition between two rooms with infinite jumps and "revert on leave" enabled.
            if (settings.JumpCount == int.MaxValue) {
                invisibleJumpCountTimer = 0.1f;
            } else if (invisibleJumpCountTimer > 0f) {
                invisibleJumpCountTimer -= Engine.DeltaTime;
            }
        }

        public override void Render() {
            base.Render();

            float minX = float.MaxValue, maxX = float.MaxValue, minY = float.MaxValue, maxY = float.MaxValue;

            // if the strawberry seed indicator is present, we should shift ourselves up.
            float offsetY = 0f;
            if (strawberrySeedIndicator != null && Scene.Tracker.Entities[strawberrySeedIndicator].Count > 0) {
                offsetY = 8f;
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                MTexture jumpIndicator = GFX.Game["ExtendedVariantMode/jumpindicator"];

                // draw no indicator in the case of infinite jumps.
                int jumpIndicatorsToDraw = Math.Min(200, invisibleJumpCountTimer > 0f ? 0 : JumpCount.GetJumpBuffer());

                int lines = 1 + (jumpIndicatorsToDraw - 1) / 5;

                for (int line = 0; line < lines; line++) {
                    int jumpIndicatorsToDrawOnLine = Math.Min(jumpIndicatorsToDraw, 5);
                    int totalWidth = jumpIndicatorsToDrawOnLine * 6 - 2;
                    for (int i = 0; i < jumpIndicatorsToDrawOnLine; i++) {
                        Vector2 position = player.Center + new Vector2(-totalWidth / 2 + i * 6, -15f - line * 6 - offsetY);
                        jumpIndicator.DrawJustified(new Vector2((float) Math.Round(position.X), (float) Math.Round(position.Y)), new Vector2(0f, 0.5f));

                        if (minX == float.MaxValue) {
                            minX = maxX = position.X;
                            minY = maxY = position.Y;
                        } else {
                            minX = Math.Min(minX, position.X);
                            maxX = Math.Max(maxX, position.X);
                            minY = Math.Min(minY, position.Y);
                            maxY = Math.Max(maxY, position.Y);
                        }
                    }
                    jumpIndicatorsToDraw -= jumpIndicatorsToDrawOnLine;
                }
            }

            if (minX != float.MaxValue) {
                Collider = new Hitbox(maxX - minX + 4, maxY - minY + 3, minX, minY - 2);
            } else {
                Collider = null;
            }
        }
    }
}
