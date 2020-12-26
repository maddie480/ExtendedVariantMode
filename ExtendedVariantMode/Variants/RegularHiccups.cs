using System;
using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace ExtendedVariants.Variants {
    public class RegularHiccups : AbstractExtendedVariant {

        private float regularHiccupTimer = 0f;

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.RegularHiccups;
        }

        public override void SetValue(int value) {
            Settings.RegularHiccups = value;
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
            regularHiccupTimer = Settings.RegularHiccups / 10f;
        }

        private void onPlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig(self, scene);

            // reset the hiccup timer when the player respawns, for more consistency.
            regularHiccupTimer = Settings.RegularHiccups / 10f;
        }

        private void modUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            if (Settings.RegularHiccups != 0) {
                regularHiccupTimer -= Engine.DeltaTime;

                if (regularHiccupTimer > Settings.RegularHiccups / 10f) {
                    regularHiccupTimer = Settings.RegularHiccups / 10f;
                }
                if (regularHiccupTimer <= 0) {
                    regularHiccupTimer = Settings.RegularHiccups / 10f;
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
            return Settings.HiccupStrength / 10f;
        }
    }
}
