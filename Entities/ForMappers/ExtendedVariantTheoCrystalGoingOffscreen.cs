using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace ExtendedVariants.Entities.ForMappers {
    [TrackedAs(typeof(TheoCrystal))]
    public class ExtendedVariantTheoCrystalGoingOffscreen : ExtendedVariantTheoCrystal {
        public ExtendedVariantTheoCrystalGoingOffscreen(Vector2 position) : base(position) { }

        public override void Update() {
            Level level = SceneAs<Level>();

            // temporarily pretend the level's boundaries are infinite. This way, vanilla code won't ever reach them.
            Rectangle originalDimensions = level.Session.LevelData.Bounds;
            level.Session.LevelData.Bounds = new Rectangle(int.MinValue / 2, int.MinValue / 2, int.MaxValue, int.MaxValue);
            base.Update();
            level.Session.LevelData.Bounds = originalDimensions;

            // then handle transitioning ourselves!
            if (!Hold.IsHeld) {
                // first, check if we should bounce Theo off the level boundaries.
                // like vanilla, except we want to do that only if the screen edge leads nowhere.
                if (Center.X > level.Bounds.Right && !level.Session.MapData.CanTransitionTo(level, BottomRight)) {
                    Right = level.Bounds.Right;
                    Speed.X *= -0.4f;
                } else if (Left < level.Bounds.Left && !level.Session.MapData.CanTransitionTo(level, BottomLeft)) {
                    Left = level.Bounds.Left;
                    Speed.X *= -0.4f;
                } else if (Top < (level.Bounds.Top - 4) && !level.Session.MapData.CanTransitionTo(level, TopCenter)) {
                    Top = level.Bounds.Top + 4;
                    Speed.Y = 0f;
                } else if (Top > level.Bounds.Bottom && (level.Session.LevelData.DisableDownTransition || !level.Session.MapData.CanTransitionTo(level, BottomCenter))) {
                    if (SaveData.Instance.Assists.Invincible) {
                        Bottom = level.Bounds.Bottom;
                        Speed.Y = -300f;
                        Audio.Play("event:/game/general/assist_screenbottom", Position);
                    } else {
                        Die();
                    }
                }

                // then, check if Theo is in another room now. if it is, make it commit remove self.
                // (yes I know all branches of that "if" are the same)
                if (Left > level.Bounds.Right && checkTransitionAndWallUpOtherTransitions(CenterLeft)) {
                    RemoveSelf();
                } else if (Right < level.Bounds.Left && checkTransitionAndWallUpOtherTransitions(CenterRight)) {
                    RemoveSelf();
                } else if (Bottom < level.Bounds.Top && checkTransitionAndWallUpOtherTransitions(BottomCenter)) {
                    RemoveSelf();
                } else if (Top > level.Bounds.Bottom && !level.Session.LevelData.DisableDownTransition && checkTransitionAndWallUpOtherTransitions(TopCenter)) {
                    RemoveSelf();
                }
            }
        }

        private bool checkTransitionAndWallUpOtherTransitions(Vector2 position) {
            Level level = SceneAs<Level>();
            if ((bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.AllowLeavingTheoBehind)) {
                // don't wall up other transitions.
                return level.Session.MapData.CanTransitionTo(level, position);
            }

            if (level.Session.MapData.CanTransitionTo(level, position)) {
                Rectangle currentBounds = level.Session.LevelData.Bounds;
                LevelData target = level.Session.MapData.GetTransitionTarget(level, position);

                // left side
                int start = int.MinValue;
                for (int i = currentBounds.Top; i <= currentBounds.Bottom + 1; i++) {
                    if (i != currentBounds.Bottom + 1 && !target.Check(new Vector2(currentBounds.Left - 8, i))) {
                        if (start == int.MinValue) {
                            start = i;
                        }
                    } else if (start != int.MinValue) {
                        level.Add(new InvisibleBarrier(new Vector2(currentBounds.Left - 8, start), 9f, i - 1 - start));
                        start = int.MinValue;
                    }
                }

                // right side
                start = int.MinValue;
                for (int i = currentBounds.Top; i <= currentBounds.Bottom + 1; i++) {
                    if (i != currentBounds.Bottom + 1 && !target.Check(new Vector2(currentBounds.Right + 8, i))) {
                        if (start == int.MinValue) {
                            start = i;
                        }
                    } else if (start != int.MinValue) {
                        level.Add(new InvisibleBarrier(new Vector2(currentBounds.Right - 1f, start), 9f, i - 1 - start));
                        start = int.MinValue;
                    }
                }

                // up side
                start = int.MinValue;
                for (int i = currentBounds.Left; i <= currentBounds.Right + 1; i++) {
                    if (i != currentBounds.Right + 1 && !target.Check(new Vector2(i, currentBounds.Top - 8))) {
                        if (start == int.MinValue) {
                            start = i;
                        }
                    } else if (start != int.MinValue) {
                        level.Add(new InvisibleBarrier(new Vector2(start, currentBounds.Top - 8), i - 1 - start, 9f));
                        start = int.MinValue;
                    }
                }

                // down side
                start = int.MinValue;
                for (int i = currentBounds.Left; i <= currentBounds.Right + 1; i++) {
                    if (i != currentBounds.Right + 1 && !target.Check(new Vector2(i, currentBounds.Bottom + 8))) {
                        if (start == int.MinValue) {
                            start = i;
                        }
                    } else if (start != int.MinValue) {
                        level.Add(new BottomDeathTrigger(new Vector2(start, currentBounds.Bottom - 1), i - 1 - start, 9));
                        start = int.MinValue;
                    }
                }

                return true;
            }

            return false;
        }

        // a trigger simulating a bottomless pit death (either killing the player, or bouncing them up).
        private class BottomDeathTrigger : Trigger {
            public BottomDeathTrigger(Vector2 position, int width, int height) : base(new EntityData { Position = position, Width = width, Height = height }, Vector2.Zero) { }

            public override void OnEnter(Player player) {
                base.OnEnter(player);

                if (SaveData.Instance.Assists.Invincible) {
                    player.Play("event:/game/general/assist_screenbottom");
                    player.Bounce(Top);
                } else {
                    player.Die(Vector2.Zero);
                }
            }
        }
    }
}
