using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class MinimumDelayBeforeThrowing : AbstractExtendedVariant {
        private static ILHook hookPickup = null;

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
                cursor.EmitDelegate<Func<float, float>>(orig => orig * GetVariantValue<float>(Variant.MinimumDelayBeforeThrowing));
            }
        }
    }
}
