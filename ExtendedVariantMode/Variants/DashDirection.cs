using Celeste;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Collections;
using MonoMod.RuntimeDetour;

namespace ExtendedVariants.Variants {
    public class DashDirection : AbstractExtendedVariant {
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
            return orig(self) &&
                (SaveData.Instance.Assists.DashAssist ||
                (Settings.DashDirection == 0) ||
                (Settings.DashDirection == 1 && (aim.X != 0) != (aim.Y != 0)) ||
                (Settings.DashDirection == 2 && aim.X != 0 && aim.Y != 0));
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

            if ((Settings.DashDirection == 1 && direction.X != 0 && direction.Y != 0)
                || (Settings.DashDirection == 2 && (direction.X == 0 || direction.Y == 0))) {

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
    }
}
