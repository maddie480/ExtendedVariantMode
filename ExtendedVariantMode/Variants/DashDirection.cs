using Celeste;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class DashDirection : AbstractExtendedVariant {
        private Hook canDashHook;

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
        }

        public override void Unload() {
            canDashHook?.Dispose();
        }

        private delegate bool orig_CanDash(Player self);

        private bool modCanDash(orig_CanDash orig, Player self) {
            Vector2 aim = Input.GetAimVector();

            return orig(self) &&
                (Settings.DashDirection == 0 ||
                (Settings.DashDirection == 1 && (aim.X != 0) != (aim.Y != 0)) ||
                (Settings.DashDirection == 2 && aim.X != 0 && aim.Y != 0));
        }
    }
}
