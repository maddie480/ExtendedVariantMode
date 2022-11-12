using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class PickupDuration : AbstractExtendedVariant {
        private static ILHook pickupCoroutineHook = null;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetVariantValue() {
            return Settings.PickupDuration;
        }

        public override object GetDefaultVariantValue() {
            return 1f;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.PickupDuration = value / 10f;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.PickupDuration = (float) value;
        }

        public override void Load() {
            pickupCoroutineHook = new ILHook(typeof(Player).GetMethod("PickupCoroutine", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetStateMachineTarget(), hookPickupCoroutine);

            On.Celeste.Player.UpdateSprite += onUpdateSprite;
        }

        public override void Unload() {
            pickupCoroutineHook?.Dispose();
            pickupCoroutineHook = null;

            On.Celeste.Player.UpdateSprite -= onUpdateSprite;
        }

        private void hookPickupCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchLdcR4(0.16f), instr => instr.MatchLdcI4(1))) {
                cursor.Index++;
                Logger.Log("ExtendedVariantMode/PickupDuration", $"Editing grab delay at {cursor.Index} in IL for Player.PickupCoroutine");
                cursor.EmitDelegate<Func<float, float>>(orig => Settings.PickupDuration * orig);
            }
        }

        private void onUpdateSprite(On.Celeste.Player.orig_UpdateSprite orig, Player self) {
            orig(self);

            if (Settings.PickupDuration != 1f && self.StateMachine.State == Player.StPickup) {
                // adapt the animation speed to the pickup speed.
                self.Sprite.Rate = Settings.PickupDuration == 0 ? 1000 : 1 / Settings.PickupDuration;
            }
        }
    }
}
