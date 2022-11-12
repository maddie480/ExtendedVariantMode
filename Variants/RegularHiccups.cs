using System;
using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace ExtendedVariants.Variants {
    public class RegularHiccups : AbstractExtendedVariant {

        private float regularHiccupTimer = 0f;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return 0f;
        }

        public override object GetVariantValue() {
            return Settings.RegularHiccups;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.RegularHiccups = (float) value;

            (ExtendedVariantsModule.Instance.VariantHandlers[ExtendedVariantsModule.Variant.RegularHiccups] as RegularHiccups).UpdateTimerFromSettings();
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.RegularHiccups = (value / 10f);
        }

        public override void Load() {
            On.Celeste.Player.Added += onPlayerAdded;
            On.Celeste.Player.Update += modUpdate;
            IL.Celeste.Player.HiccupJump += modHiccupJump;
        }

        public override void Unload() {
            On.Celeste.Player.Added -= onPlayerAdded;
            On.Celeste.Player.Update -= modUpdate;
            IL.Celeste.Player.HiccupJump -= modHiccupJump;
        }

        public void UpdateTimerFromSettings() {
            regularHiccupTimer = Settings.RegularHiccups;
        }

        private void onPlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig(self, scene);

            // reset the hiccup timer when the player respawns, for more consistency.
            regularHiccupTimer = Settings.RegularHiccups;
        }

        private void modUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            if (Settings.RegularHiccups != 0f) {
                regularHiccupTimer -= Engine.DeltaTime;

                if (regularHiccupTimer > Settings.RegularHiccups) {
                    regularHiccupTimer = Settings.RegularHiccups;
                }
                if (regularHiccupTimer <= 0) {
                    regularHiccupTimer = Settings.RegularHiccups;
                    self.HiccupJump();
                }
            }
        }

        private void modHiccupJump(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-60f))) {
                Logger.Log("ExtendedVariantMode/RegularHiccups", $"Modding hiccup size at {cursor.Index} in CIL code for HiccupJump");

                cursor.EmitDelegate<Func<float>>(determineHiccupStrengthFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private float determineHiccupStrengthFactor() {
            return Settings.HiccupStrength;
        }
    }
}
