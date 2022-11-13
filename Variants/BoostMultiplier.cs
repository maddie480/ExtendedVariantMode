using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class BoostMultiplier : AbstractExtendedVariant {
        private static Hook liftBoostHook;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override object GetVariantValue() {
            return Settings.BoostMultiplier;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.BoostMultiplier = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.BoostMultiplier = (value / 10f);
        }

        public override void Load() {
            liftBoostHook = new Hook(
                typeof(Player).GetMethod("get_LiftBoost", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(BoostMultiplier).GetMethod("multiplyLiftBoost", BindingFlags.NonPublic | BindingFlags.Instance), this);
        }

        public override void Unload() {
            liftBoostHook?.Dispose();
            liftBoostHook = null;
        }

        private Vector2 multiplyLiftBoost(Func<Player, Vector2> orig, Player self) {
            Vector2 result = orig(self);

            if (Settings.BoostMultiplier < 0f) {
                // capping has to be flipped around too!
                result.Y = Calc.Clamp(self.LiftSpeed.Y, 0f, 130f);
            }
            return result * Settings.BoostMultiplier;
        }
    }
}
