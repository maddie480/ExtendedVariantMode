using System;
using System.Collections.Generic;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;

namespace ExtendedVariants.Variants {
    public class MultiBuffering : AbstractExtendedVariant {
        public override void Load() => IL.Monocle.VirtualButton.Update += VirtualButton_Update_il;

        public override void Unload() => IL.Monocle.VirtualButton.Update -= VirtualButton_Update_il;

        public override Type GetVariantType() => typeof(int);

        public override object GetDefaultVariantValue() => 1;

        public override object ConvertLegacyVariantValue(int value) => value;

        private void VirtualButton_Update_il(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.GotoNext(instr => instr.MatchStfld<VirtualButton>("bufferCounter"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, VirtualButton, float>>((bufferCounter, button) => {
                int size = GetVariantValue<int>(ExtendedVariantsModule.Variant.MultiBuffering);

                if (size == 1)
                    return bufferCounter;

                var dynamicData = DynamicData.For(button);

                if (!dynamicData.TryGet("bufferQueue", out List<float> bufferQueue) || bufferQueue.Capacity != size - 1) {
                    bufferQueue = new List<float>(size - 1);
                    dynamicData.Set("bufferQueue", bufferQueue);
                }

                for (int i = 0; i < bufferQueue.Count; i++)
                    bufferQueue[i] -= Engine.DeltaTime;

                while (bufferQueue.Count > 0 && bufferCounter <= 0f) {
                    bufferCounter = bufferQueue[0];
                    bufferQueue.RemoveAt(0);
                }

                return bufferCounter;
            });

            cursor.GotoNext(
                instr => instr.MatchLdfld<VirtualButton>("BufferTime"),
                instr => instr.MatchStfld<VirtualButton>("bufferCounter"));
            cursor.Index++;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit<VirtualButton>(OpCodes.Ldfld, "bufferCounter");
            cursor.EmitDelegate<Func<float, VirtualButton, float, float>>((bufferTime, button, bufferCounter) => {
                if (bufferCounter <= 0f)
                    return bufferTime;

                int size = GetVariantValue<int>(ExtendedVariantsModule.Variant.MultiBuffering);

                if (size == 1)
                    return bufferTime;

                var bufferQueue = DynamicData.For(button).Get<List<float>>("bufferQueue");

                if (bufferQueue.Count == size - 1) {
                    bufferCounter = bufferQueue[0];
                    bufferQueue.RemoveAt(0);
                }

                bufferQueue.Add(bufferTime);

                return bufferCounter;
            });

            cursor.Index = -1;
            cursor.GotoPrev(MoveType.After, instr => instr.MatchStfld<VirtualButton>("bufferCounter"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<VirtualButton>>(button => {
                if (GetVariantValue<int>(ExtendedVariantsModule.Variant.MultiBuffering) > 1
                    && !GetVariantValue<bool>(ExtendedVariantsModule.Variant.AlternativeBuffering))
                    DynamicData.For(button).Get<List<float>>("bufferQueue").Clear();
            });
        }
    }
}