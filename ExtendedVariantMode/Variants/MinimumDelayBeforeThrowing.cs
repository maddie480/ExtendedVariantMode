using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class MinimumDelayBeforeThrowing : AbstractExtendedVariant {
        private static ILHook hookPickup = null;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetVariantValue() {
            return Settings.MinimumDelayBeforeThrowing;
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.MinimumDelayBeforeThrowing = value / 10f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.MinimumDelayBeforeThrowing = (float) value;
        }

        public override void Load() {
            hookPickup = new ILHook(typeof(Player).GetMethod("orig_Pickup", BindingFlags.NonPublic | BindingFlags.Instance), hookOrigPickup);
        }

        public override void Unload() {
            hookPickup?.Dispose();
            hookPickup = null;
        }

        private void hookOrigPickup(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.35f))) {
                Logger.Log("ExtendedVariantMode/MinimumDelayBeforeThrowing", $"Modding minimum delay before throwing at {cursor.Index} in IL for Player.orig_Pickup");
                cursor.EmitDelegate<Func<float, float>>(orig => orig * Settings.MinimumDelayBeforeThrowing);
            }
        }
    }
}
