
using System;
using Celeste;
using Celeste.Mod;
using Monocle;
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
            IL.Monocle.VirtualButton.Update += modVirtualButtonUpdate;
        }

        public override void Unload() {
            IL.Celeste.Level.Update -= modLevelUpdate;
            IL.Monocle.VirtualButton.Update -= modVirtualButtonUpdate;
        }

        private void modLevelUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Assists>("GameSpeed"))) {
                Logger.Log("ExtendedVariantMode/GameSpeed", $"Injecting own game speed at {cursor.Index} in IL code for Level.Update");
                cursor.EmitDelegate<Func<int, int>>(modGameSpeed);

                if(cursor.TryGotoNext(instr => instr.MatchStsfld<Level>("AssistSpeedSnapshotValue"))) {
                    Logger.Log("ExtendedVariantMode/GameSpeed", $"Modding speed sound snapshot at {cursor.Index} in IL code for Level.Update");
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

        private void modVirtualButtonUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // the goal is to jump at this.repeatCounter -= Engine.DeltaTime;
            // we want to mod the repeat timer to make menus more usable at very high or low speeds.
            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdfld<VirtualButton>("repeatCounter"),
                instr => instr.MatchCall<Engine>("get_DeltaTime"))) {

                cursor.Index--;

                Logger.Log("ExtendedVariantMode/GameSpeed", $"Modding DeltaTime at {cursor.Index} in IL code for VirtualButton.Update");

                cursor.Remove();
                cursor.EmitDelegate<Func<float>>(getRepeatTimerDeltaTime);

                // what we have now is this.repeatCounter -= getRepeatTimerDeltaTime();
            }
        }

        private float getRepeatTimerDeltaTime() {
            // the delta time is Engine.RawDeltaTime * TimeRate * TimeRateB
            // we want to bound TimeRate * TimeRateB between 0.5 and 1.6
            // (this is the span of the vanilla Game Speed variant)
            float timeRate = Engine.TimeRate * Engine.TimeRateB;

            if(timeRate < 0.5f) {
                return Engine.RawDeltaTime * 0.5f;
            } else if(timeRate > 1.6f) {
                return Engine.RawDeltaTime * 1.6f;
            }
            // return the original value
            return Engine.DeltaTime;
        }
    }
}
