
using System;
using Celeste;
using Celeste.Mod;
using MonoMod.Cil;

namespace ExtendedVariants.Variants {
    class GameSpeed : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.GameSpeed;
        }

        public override void SetValue(int value) {
            Settings.GameSpeed = value;
        }

        public override void Load() {
            IL.Celeste.Level.Update += modLevelUpdate;
        }

        public override void Unload() {
            IL.Celeste.Level.Update -= modLevelUpdate;
        }

        private void modLevelUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Assists>("GameSpeed"))) {
                Logger.Log("ExtendedVariantMode", $"Injecting own game speed at {cursor.Index} in IL code for Level.Update");
                cursor.EmitDelegate<Func<int, int>>(modGameSpeed);

                if(cursor.TryGotoNext(instr => instr.MatchStsfld<Level>("AssistSpeedSnapshotValue"))) {
                    Logger.Log("ExtendedVariantMode", $"Modding speed sound snapshot at {cursor.Index} in IL code for Level.Update");
                    cursor.EmitDelegate<Func<int, int>>(modSpeedSoundSnapshot);
                }
            }
        }

        private int modGameSpeed(int gameSpeed) {
            return gameSpeed * Settings.GameSpeed / 10;
        }

        private int modSpeedSoundSnapshot(int originalSnapshot) {
            // vanilla values are 5, 6, 7, 8, 9, 10, 12, 14 and 16
            // mod the snapshot to a "close enough" value
            if (originalSnapshot <= 5) return 5; // 5 or lower => 5
            else if (originalSnapshot <= 10) return originalSnapshot; // 6~10 => 6~10
            else if (originalSnapshot <= 13) return 12; // 11~13 => 12
            else if (originalSnapshot <= 15) return 14; // 14 or 15 => 14
            else return 16; // 16 or higher => 16
        }
    }
}
