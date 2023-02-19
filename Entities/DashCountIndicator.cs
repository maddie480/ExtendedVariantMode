using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedVariants.Entities {
    /// <summary>
    /// An indicator for the dash count (numbers above Madeline's head).
    /// </summary>
    public class DashCountIndicator : Entity {
        private const int normalDepth = (Depths.FGTerrain + Depths.FGDecals) / 2; // between fg tiles and fg decals
        private const int depthInFrontOfSolids = Depths.FakeWalls - 1; // in front of fake walls

        // dash count is hidden during intros and cutscenes, when player doesn't have control.
        private static readonly HashSet<int> hiddenPlayerStates = new HashSet<int> { Player.StIntroJump, Player.StIntroMoonJump, Player.StIntroRespawn,
            Player.StIntroThinkForABit, Player.StIntroWakeUp, Player.StIntroWalk, Player.StReflectionFall, Player.StDummy };

        private static MTexture[] numbers;
        protected ExtendedVariantsSettings settings;

        private static Type strawberrySeedIndicator = null;

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

            // extract a reference to the Max Helping Hand strawberry seed indicator.
            EverestModule mod = Everest.Modules.FirstOrDefault(m => m.Metadata?.Name == "MaxHelpingHand");
            if (mod != null) {
                strawberrySeedIndicator = mod.GetType().Assembly.GetType("Celeste.Mod.MaxHelpingHand.Entities.MultiRoomStrawberryCounter");
            }
        }

        public DashCountIndicator() {
            AddTag(Tags.Persistent); // this entity isn't bound to the screen it was spawned in, keep it when transitioning to another level.

            settings = ExtendedVariantsModule.Settings;
        }

        public override void Update() {
            base.Update();

            if (!settings.MasterSwitch) {
                // extended variants were turned off, the dash count indicator should kick itself out.
                RemoveSelf();
            }

            // if the indicator overlaps with a solid that's in front of everything (exit blocks etc), send it in front of them.
            // otherwise, have it between fg decals and fg tiles
            if (Collider != null && CollideAll<Solid>().Any(solid => solid.Depth < normalDepth)) {
                Depth = depthInFrontOfSolids;
            } else {
                Depth = normalDepth;
            }
        }

        public override void Render() {
            if (!shouldShowCounter()) {
                // hide the dash count.
                return;
            }

            base.Render();

            float minX = float.MaxValue, maxX = float.MaxValue, minY = float.MaxValue, maxY = float.MaxValue;

            // if the strawberry seed indicator is present, we should shift ourselves up.
            float offsetY = 0f;
            if (strawberrySeedIndicator != null && Scene.Tracker.Entities[strawberrySeedIndicator].Count > 0) {
                offsetY = 8f;
            }
            offsetY += getExtraOffset();

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                if (hiddenPlayerStates.Contains(player.StateMachine.State)) {
                    // the player is in one of the states that should have the dash count hidden.
                    return;
                }

                // compute the jump count so that we can put the dash count above it.
                int jumpIndicatorsToDraw = (int) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.JumpCount)
                    == int.MaxValue ? 0 : JumpCount.GetJumpBuffer();
                int jumpCountLines = jumpIndicatorsToDraw == 0 ? 0 : 1 + (jumpIndicatorsToDraw - 1) / 5;

                // draw Madeline's dash count, digit by digit.
                string number = getNumberToDisplay(player);
                int totalWidth = number.Length * 4 - 1;
                for (int i = 0; i < number.Length; i++) {
                    Vector2 position = player.Center + new Vector2(-totalWidth / 2 + i * 4, -18f - jumpCountLines * 6f - offsetY);
                    numbers[number.ToCharArray()[i] - '0'].DrawOutline(position, new Vector2(0f, 0.5f));

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
            }

            if (minX != float.MaxValue) {
                Collider = new Hitbox(maxX - minX + 5, maxY - minY + 6, minX - 1, minY - 1);
            } else {
                Collider = null;
            }
        }

        protected virtual bool shouldShowCounter() {
            return (bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.DisplayDashCount);
        }

        protected virtual float getExtraOffset() {
            return 0f;
        }

        protected virtual string getNumberToDisplay(Player player) {
            return player.Dashes.ToString();
        }
    }
}
