using Celeste;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Collections;
using MonoMod.RuntimeDetour;
using System;

namespace ExtendedVariants.Variants {
    public class DashDirection : AbstractExtendedVariant {
        // those are all the dash directions; to allow top and bottom-left, set Settings.DashDirection to TOP | BOTTOM_LEFT | BASE
        public const int TOP = 0b1000000000;
        public const int TOP_RIGHT = 0b0100000000;
        public const int RIGHT = 0b0010000000;
        public const int BOTTOM_RIGHT = 0b0001000000;
        public const int BOTTOM = 0b0000100000;
        public const int BOTTOM_LEFT = 0b0000010000;
        public const int LEFT = 0b0000001000;
        public const int TOP_LEFT = 0b0000000100;
        public const int BASE = 0b0000000011;

        private static FieldInfo playerLastAim = typeof(Player).GetField("lastAim", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo playerCalledDashEvents = typeof(Player).GetField("calledDashEvents", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo playerBeforeDashSpeed = typeof(Player).GetField("beforeDashSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo playerLastDashes = typeof(Player).GetField("lastDashes", BindingFlags.NonPublic | BindingFlags.Instance);

        private Hook canDashHook;
        private int dashCountBeforeDash;

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.DashDirection;
        }

        public override void SetValue(int value) {
            Settings.DashDirection = value;
        }

        public override void Load() {
            canDashHook = new Hook(typeof(Player).GetMethod("get_CanDash"), typeof(DashDirection).GetMethod("modCanDash", BindingFlags.NonPublic | BindingFlags.Instance), this);
            On.Celeste.Player.StartDash += onStartDash;
            On.Celeste.Player.DashCoroutine += onDashCoroutine;
            On.Celeste.Player.RedDashCoroutine += onRedDashCoroutine;
        }

        public override void Unload() {
            canDashHook?.Dispose();
            On.Celeste.Player.StartDash -= onStartDash;
            On.Celeste.Player.DashCoroutine -= onDashCoroutine;
            On.Celeste.Player.RedDashCoroutine -= onRedDashCoroutine;
        }

        private delegate bool orig_CanDash(Player self);

        private bool modCanDash(orig_CanDash orig, Player self) {
            Vector2 aim = Input.GetAimVector();

            // block the dash directly if the player is holding a forbidden direction, and does not have Dash Assist enabled.
            return orig(self) && (SaveData.Instance.Assists.DashAssist || isDashDirectionAllowed(aim));
        }

        private int onStartDash(On.Celeste.Player.orig_StartDash orig, Player self) {
            dashCountBeforeDash = self.Dashes;
            return orig(self);
        }

        private IEnumerator onDashCoroutine(On.Celeste.Player.orig_DashCoroutine orig, Player self) {
            if (Settings.DashDirection == 0) {
                return orig(self);
            }
            return modDashCoroutine(orig(self), self);
        }

        private IEnumerator onRedDashCoroutine(On.Celeste.Player.orig_RedDashCoroutine orig, Player self) {
            if (Settings.DashDirection == 0) {
                return orig(self);
            }
            return modDashCoroutine(orig(self), self);
        }

        private IEnumerator modDashCoroutine(IEnumerator vanillaCoroutine, Player self) {
            // make a step forward
            if (vanillaCoroutine.MoveNext()) {
                yield return vanillaCoroutine.Current;
            }

            // get the dash general direction
            Vector2 direction;
            if (self.OverrideDashDirection.HasValue) {
                direction = self.OverrideDashDirection.Value;
            } else {
                direction = (Vector2) playerLastAim.GetValue(self);
            }

            if (!isDashDirectionAllowed(direction)) {
                // forbidden direction! aaa

                // prevent DashEnd from triggering dash events (no dash sound)
                playerCalledDashEvents.SetValue(self, true);
                // restore pre-dash speed
                self.Speed = (Vector2) playerBeforeDashSpeed.GetValue(self);
                // restore pre-dash dash count
                self.Dashes = dashCountBeforeDash;
                // prevent the hair from flashing
                playerLastDashes.SetValue(self, self.Dashes);

                // if in a bubble, make the bubble explode
                if (self.CurrentBooster != null) {
                    self.CurrentBooster.PlayerReleased();
                    self.CurrentBooster = null;
                }

                // kick the player back to the normal state
                self.StateMachine.State = 0;
            } else {
                // continue with the dash like normal.
                while (vanillaCoroutine.MoveNext()) {
                    yield return vanillaCoroutine.Current;
                }
            }
        }

        private bool isDashDirectionAllowed(Vector2 direction) {
            int allowedDirections = Settings.DashDirection;
            if (allowedDirections == 0) {
                // all directions allowed! let's skip over everything.
                return true;
            } else if (allowedDirections == 1) {
                // straight only
                allowedDirections = 0b1010101000;
            } else if (allowedDirections == 2) {
                // diagonal only
                allowedDirections = 0b0101010100;
            }

            // if directions are not integers, make them integers.
            direction = new Vector2(Math.Sign(direction.X), Math.Sign(direction.Y));

            // apply a mask to the current direction to determine if it is allowed or not.
            if (direction.X == -1) {
                if (direction.Y == -1) {
                    return (allowedDirections & TOP_LEFT) != 0;
                } else if (direction.Y == 0) {
                    return (allowedDirections & LEFT) != 0;
                } else if (direction.Y == 1) {
                    return (allowedDirections & BOTTOM_LEFT) != 0;
                }
            } else if (direction.X == 0) {
                if (direction.Y == -1) {
                    return (allowedDirections & TOP) != 0;
                } else if (direction.Y == 1) {
                    return (allowedDirections & BOTTOM) != 0;
                }
            } else if (direction.X == 1) {
                if (direction.Y == -1) {
                    return (allowedDirections & TOP_RIGHT) != 0;
                } else if (direction.Y == 0) {
                    return (allowedDirections & RIGHT) != 0;
                } else if (direction.Y == 1) {
                    return (allowedDirections & BOTTOM_RIGHT) != 0;
                }
            }

            // what? I checked all directions though!
            return false;
        }
    }
}
