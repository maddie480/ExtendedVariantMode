using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;

namespace ExtendedVariants.Variants {
    public class DisableDashCooldown : AbstractExtendedVariant {
        private ILHook hookOnCanDash;
        private ILHook hookOnOrigUpdate;

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetVariantValue() {
            return Settings.DisableDashCooldown;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DisableDashCooldown = (bool) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.DisableDashCooldown = (value != 0);
        }

        public override void Load() {
            hookOnCanDash = new ILHook(typeof(Player).GetMethod("get_CanDash"), modCanDash);
            hookOnOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update"), modCanDash);
        }

        public override void Unload() {
            hookOnCanDash?.Dispose();
            hookOnCanDash = null;

            hookOnOrigUpdate?.Dispose();
            hookOnOrigUpdate = null;
        }

        private void modCanDash(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Player>("dashCooldownTimer") || instr.MatchLdfld<Player>("dashRefillCooldownTimer"))) {
                Logger.Log("ExtendedVariantMode/DisableDashCooldown", $"Disabling dash cooldown at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<float, float>>(modDashCooldownTimer);
            }
        }

        private float modDashCooldownTimer(float orig) {
            if (Settings.DisableDashCooldown) {
                return 0f;
            }

            return orig;
        }
    }
}
