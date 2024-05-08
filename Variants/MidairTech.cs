using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class MidairTech : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            IL.Celeste.Player.DashUpdate += modMidairTech;
            IL.Celeste.Player.RedDashUpdate += modMidairTech;
        }

        public override void Unload() {
            IL.Celeste.Player.DashUpdate -= modMidairTech;
            IL.Celeste.Player.RedDashUpdate -= modMidairTech;
        }

        private void modMidairTech(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // Remove the grounded check
            // We have to be careful here because XaphanHelper also hooks here

            ILLabel labelPastCheck = cursor.DefineLabel();

            if (!cursor.TryGotoNext(
                MoveType.Before,
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdfld<Player>("jumpGraceTimer")
            )) { return; }
               
            int before = cursor.Index;

            if (!cursor.TryGotoNext(
                MoveType.After,
                instr => instr.MatchBleUn(out _)
            )) { return; }

            Logger.Log("ExtendedVariantMode/MidairTech", $"Modding midair check in CIL code for {cursor.Method.Name}");
                
            cursor.MarkLabel(labelPastCheck);

            cursor.Index = before;
            cursor.MoveAfterLabels();
                
            cursor.EmitDelegate<Func<bool>>(GetMidairTechAllowed);
            cursor.Emit(OpCodes.Brtrue, labelPastCheck);
        }

        private bool GetMidairTechAllowed() {
            return GetVariantValue<bool>(Variant.MidairTech);
        }
    }
}
