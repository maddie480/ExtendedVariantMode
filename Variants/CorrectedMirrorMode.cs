using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class CorrectedMirrorMode : AbstractExtendedVariant {
        static private ILHook patchVirtualIntegerAxisUpdateHook;

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public CorrectedMirrorMode() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override void Load() {
            patchVirtualIntegerAxisUpdateHook = new ILHook(typeof(VirtualIntegerAxis).GetMethod("orig_Update"), PatchVirtualIntegerAxisUpdate);
        }

        public override void Unload() {
            patchVirtualIntegerAxisUpdateHook?.Dispose();
        }


        private static void PatchVirtualIntegerAxisUpdate(ILContext il) {
            FieldInfo f_turned = typeof(VirtualIntegerAxis).GetField("turned", BindingFlags.NonPublic | BindingFlags.Instance);

            ILCursor cursor = new ILCursor(il);

            ILLabel afterTurnedCheckLabel = cursor.DefineLabel();
            ILLabel afterInvertedLabel = null;

            cursor.GotoNext(inter => inter.MatchLdfld("Monocle.VirtualIntegerAxis", "Inverted"));
            cursor.GotoNext(MoveType.After, inter => inter.MatchBrfalse(out afterInvertedLabel));

            // test if the variant is active
            cursor.EmitDelegate<Func<bool>>(() => {
                return (bool) Instance.TriggerManager.GetCurrentVariantValue(Variant.CorrectedMirrorMode);
            });

            // if it isn't, skip over the correction check
            cursor.Emit(OpCodes.Brfalse, afterTurnedCheckLabel);

            // if it is, we need to not invert the direction of the player if turned is set to true
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, f_turned);
            cursor.Emit(OpCodes.Brtrue, afterInvertedLabel);
            cursor.MarkLabel(afterTurnedCheckLabel);
        }
    }
}
