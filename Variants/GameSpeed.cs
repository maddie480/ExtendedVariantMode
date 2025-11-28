using System;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class GameSpeed : AbstractExtendedVariant {
        private static float previousGameSpeed = 1f;

        private static ILHook hookAntiSoftlock = null;

        public GameSpeed() : base(variantType: typeof(float), defaultVariantValue: 1f) {}

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            IL.Celeste.Level.Update += modLevelUpdate;
            IL.Monocle.VirtualButton.Update += modVirtualButtonUpdate;

            // turns out GetMethod("TransitionRoutine").GetStateMachineTarget() gives me <TransitionRoutine>d__126 when I want the other one.
            // The other one being <TransitionRoutine>d__29 on Core and <TransitionRoutine>d__26 on non-Core. :FRICK:
            // I harness the power of brute force
            Type nestedTransitionRoutineType = null;
            for (int i = 0; i < 126; i++) {
                nestedTransitionRoutineType = typeof(Level).Assembly.GetType($"Celeste.Level+<TransitionRoutine>d__{i}");
                if (nestedTransitionRoutineType != null) break;
            }

            MethodInfo moveNext = nestedTransitionRoutineType.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);
            TryDisableInlining(moveNext);
            hookAntiSoftlock = new ILHook(moveNext, fixupAntiSoftlockDelay);
        }

        public override void Unload() {
            IL.Celeste.Level.Update -= modLevelUpdate;
            IL.Monocle.VirtualButton.Update -= modVirtualButtonUpdate;

            hookAntiSoftlock?.Dispose();
            hookAntiSoftlock = null;

            previousGameSpeed = 10;
        }

        private static void modLevelUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Assists>("GameSpeed"))) {
                Logger.Log("ExtendedVariantMode/GameSpeed", $"Injecting own game speed at {cursor.Index} in IL code for Level.Update");
                cursor.EmitDelegate<Func<int, int>>(modGameSpeed);
            }

            // mod the snapshots that are applied to snap all extended variants values to their closest available vanilla values.
            while (cursor.TryGotoNext(
                instr => instr.MatchLdsfld<Level>("AssistSpeedSnapshotValue"),
                instr => instr.MatchLdcI4(10),
                instr => instr.MatchMul())) {

                cursor.Index++;

                Logger.Log("ExtendedVariantMode/GameSpeed", $"Modding speed sound snapshot at {cursor.Index} in IL code for Level.Update");
                cursor.EmitDelegate<Func<int, int>>(modSpeedSoundSnapshot);
            }

            cursor.Index = 0;

            // in vanilla, no snapshot is applied past 1.6x, change that to *infinite*.
            // we want to apply the 1.6x snapshot even on 100x speed.
            if (cursor.TryGotoNext(
                instr => instr.MatchLdsfld<Level>("AssistSpeedSnapshotValue"),
                instr => instr.MatchLdcI4(16),
                instr => instr.OpCode == OpCodes.Bgt_S)) {

                cursor.Index++;
                Logger.Log("ExtendedVariantMode/GameSpeed", $"Modding max speed sound snapshot at {cursor.Index} in IL code for Level.Update");

                cursor.Next.OpCode = OpCodes.Ldc_I4;
                cursor.Next.Operand = int.MaxValue;
            }
        }

        private static int modGameSpeed(int gameSpeed) {
            if (previousGameSpeed > GetVariantValue<float>(Variant.GameSpeed)) {
                Logger.Log("ExtendedVariantMode/GameSpeed", "Game speed slowed down, ensuring cassette blocks and sprites are sane...");
                if (Engine.Scene is Level level) {
                    // go across all entities in the screen
                    foreach (Entity entity in level.Entities) {
                        // ... and all sprites in them
                        foreach (Sprite sprite in entity.Components.GetAll<Sprite>()) {
                            if (sprite.Animating) {
                                // if the current animation is waaaay over, bring it to a more sane value.
                                if (Math.Abs(sprite.animationTimer) >= sprite.currentAnimation.Delay * 2) {
                                    sprite.animationTimer = sprite.currentAnimation.Delay;
                                    Logger.Log(LogLevel.Info, "ExtendedVariantMode/GameSpeed", $"Sanified animation for {sprite.Texture?.AtlasPath}");
                                }
                            }
                        }
                    }

                    CassetteBlockManager manager = level.Tracker.GetEntity<CassetteBlockManager>();
                    if (manager != null) {
                        // check if the beatTimer of the cassette block manager went overboard or not.
                        if (manager.beatTimer > 0.166666672f * 2) {
                            // this is madness!
                            manager.beatTimer = 0.166666672f;
                            Logger.Log(LogLevel.Info, "ExtendedVariantMode/GameSpeed", "Sanified cassette block beat timer");
                        }
                    }
                }
            }
            previousGameSpeed = GetVariantValue<float>(Variant.GameSpeed);

            return (int) (gameSpeed * GetVariantValue<float>(Variant.GameSpeed));
        }

        private static int modSpeedSoundSnapshot(int originalSnapshot) {
            // vanilla values are 5, 6, 7, 8, 9, 10, 12, 14 and 16
            // mod the snapshot to a "close enough" value
            if (originalSnapshot <= 5) return 5; // 5 or lower => 5
            else if (originalSnapshot <= 10) return originalSnapshot; // 6~10 => 6~10
            else if (originalSnapshot <= 13) return 12; // 11~13 => 12
            else if (originalSnapshot <= 15) return 14; // 14 or 15 => 14
            else return 16; // 16 or higher => 16
        }

        private static void modVirtualButtonUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // the goal is to jump at this.repeatCounter -= Engine.DeltaTime;
            // we want to mod the repeat timer to make menus more usable at very high or low speeds.
            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdfld<VirtualButton>("repeatCounter"),
                instr => instr.MatchCall<Engine>("get_DeltaTime"))) {

                Logger.Log("ExtendedVariantMode/GameSpeed", $"Modding DeltaTime at {cursor.Index} in IL code for VirtualButton.Update");
                cursor.EmitDelegate<Func<float, float>>(getRepeatTimerDeltaTime);

                // what we have now is this.repeatCounter -= getRepeatTimerDeltaTime();
            }
        }

        private static float getRepeatTimerDeltaTime(float orig) {
            // the delta time is Engine.RawDeltaTime * TimeRate * TimeRateB
            // we want to bound TimeRate * TimeRateB between 0.5 and 1.6
            // (this is the span of the vanilla Game Speed variant)
            float timeRate = Engine.TimeRate * Engine.TimeRateB;

            if (timeRate < 0.5f) {
                return Engine.RawDeltaTime * 0.5f;
            } else if (timeRate > 1.6f) {
                return Engine.RawDeltaTime * 1.6f;
            }
            // return the original value
            return orig;
        }

        private static void fixupAntiSoftlockDelay(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR8(5.0))) {
                Logger.Log("ExtendedVariantMode/GameSpeed", $"Fixing up anti-softlock delay at {cursor.Index} in IL for Level.TransitionRoutine ({il.Method.DeclaringType.Name})");
                cursor.EmitDelegate<Func<double, double>>(applyNewAntiSoftlockDelay);
            }
        }
        private static double applyNewAntiSoftlockDelay(double orig) {
            float gameSpeed = GetVariantValue<float>(Variant.GameSpeed);
            if (gameSpeed <= 0 || !(Engine.Scene is Level level)) {
                return orig;
            }

            // make sure the softlock timer gives at least the theoretical transition time + 2 seconds.
            float transitionDuration = (level.NextTransitionDuration / gameSpeed) + 2f;
            if (transitionDuration > orig) {
                Logger.Log("ExtendedVariantMode/GameSpeed", $"Extending allowed transition duration to {transitionDuration} seconds!");
                return transitionDuration;
            }

            return orig;
        }
    }
}
