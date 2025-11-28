using System;
using System.Collections.Generic;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace ExtendedVariants.Variants {
    public class MultiBuffering : AbstractExtendedVariant {
        private static readonly Dictionary<VirtualButton, List<float>> bufferQueues = new Dictionary<VirtualButton, List<float>>();

        public override void Load() => IL.Monocle.VirtualButton.Update += VirtualButton_Update_il;

        public override void Unload() {
            IL.Monocle.VirtualButton.Update -= VirtualButton_Update_il;
            bufferQueues.Clear();
        }

        public MultiBuffering() : base(variantType: typeof(int), defaultVariantValue: 1) { }

        public override object ConvertLegacyVariantValue(int value) => value;

        private static void VirtualButton_Update_il(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.GotoNext(instr => instr.MatchStfld<VirtualButton>("bufferCounter"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, VirtualButton, float>>(computeBufferCounter);

            cursor.GotoNext(
                instr => instr.MatchLdfld<VirtualButton>("BufferTime"),
                instr => instr.MatchStfld<VirtualButton>("bufferCounter"));
            cursor.Index++;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit<VirtualButton>(OpCodes.Ldfld, "bufferCounter");
            cursor.EmitDelegate<Func<float, VirtualButton, float, float>>(computeBufferTime);

            cursor.Index = -1;
            cursor.GotoPrev(MoveType.After, instr => instr.MatchStfld<VirtualButton>("bufferCounter"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<VirtualButton>>(clearBufferQueue);
        }

        private static float computeBufferCounter(float bufferCounter, VirtualButton button) {
            int size = GetVariantValue<int>(ExtendedVariantsModule.Variant.MultiBuffering);

            if (size == 1)
                return bufferCounter;

            if (!bufferQueues.TryGetValue(button, out List<float> bufferQueue) || bufferQueue.Capacity != size - 1) {
                bufferQueue = new List<float>(size - 1);
                bufferQueues[button] = bufferQueue;
            }

            for (int i = 0; i < bufferQueue.Count; i++)
                bufferQueue[i] -= Engine.DeltaTime;

            while (bufferQueue.Count > 0 && bufferCounter <= 0f) {
                bufferCounter = bufferQueue[0];
                bufferQueue.RemoveAt(0);
            }

            return bufferCounter;
        }

        private static float computeBufferTime(float bufferTime, VirtualButton button, float bufferCounter) {
            if (bufferCounter <= 0f)
                return bufferTime;

            int size = GetVariantValue<int>(ExtendedVariantsModule.Variant.MultiBuffering);

            if (size == 1)
                return bufferTime;

            var bufferQueue = bufferQueues[button];

            if (bufferQueue.Count == size - 1) {
                bufferCounter = bufferQueue[0];
                bufferQueue.RemoveAt(0);
            }

            bufferQueue.Add(bufferTime);

            return bufferCounter;
        }

        private static void clearBufferQueue(VirtualButton button) {
            if (GetVariantValue<int>(ExtendedVariantsModule.Variant.MultiBuffering) > 1
                && !GetVariantValue<bool>(ExtendedVariantsModule.Variant.AlternativeBuffering))
                bufferQueues[button].Clear();
        }
    }
}
