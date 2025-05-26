using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class BoostMultiplier : AbstractExtendedVariant {
        private static Hook liftBoostHook;
        private static bool isBoostMultiplierApplied = true;

        public BoostMultiplier() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            liftBoostHook = new Hook(
                typeof(Player).GetMethod("get_LiftBoost", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(BoostMultiplier).GetMethod("multiplyLiftBoost", BindingFlags.NonPublic | BindingFlags.Instance), this);

            IL.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
        }

        public override void Unload() {
            liftBoostHook?.Dispose();
            liftBoostHook = null;

            IL.Celeste.Player.NormalUpdate -= modPlayerNormalUpdate;
        }

        private Vector2 multiplyLiftBoost(Func<Player, Vector2> orig, Player self) {
            Vector2 result = orig(self);

            float capX = GetVariantValue<float>(Variant.LiftboostCapX);
            float capUp = GetVariantValue<float>(Variant.LiftboostCapUp);
            float capDown = GetVariantValue<float>(Variant.LiftboostCapDown);

            if (capX < 0.0f) capX = float.MaxValue;
            if (capUp > 0.0f) capUp = -float.MaxValue;
            if (capDown < 0.0f) capDown = float.MaxValue;

            if (capX != LiftboostCapX.Default)
                result.X = Calc.Clamp(self.LiftSpeed.X, -capX, capX);
            if (capUp != LiftboostCapUp.Default || capDown != LiftboostCapDown.Default)
                result.Y = Calc.Clamp(self.LiftSpeed.Y, Math.Min(capUp, capDown), Math.Max(capUp, capDown));

            if (!isBoostMultiplierApplied) return result;

            if (GetVariantValue<float>(Variant.BoostMultiplier) < 0f) {
                // capping has to be flipped around too!
                result.Y = Calc.Clamp(self.LiftSpeed.Y, 0f, 130f);
            }
            return result * GetVariantValue<float>(Variant.BoostMultiplier);
        }

        private void modPlayerNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // disable the boost multiplier for the first part of the method: it is intended to be able to walk off from a block seamlessly...
            // and multiplying the boost makes it *a bit* less seamless.
            cursor.EmitDelegate<Action>(() => isBoostMultiplierApplied = false);
            cursor.GotoNext(instr => instr.MatchCallvirt<Player>("get_Holding"));
            cursor.EmitDelegate<Action>(() => isBoostMultiplierApplied = true);
        }
    }
}
