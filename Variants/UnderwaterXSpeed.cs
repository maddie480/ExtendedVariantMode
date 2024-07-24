using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class UnderwaterSpeedX : AbstractExtendedVariant {
        public const float VanillaSpeed = 60f;
        public const float SlowerDecelerationSpeedThreshold = 80f;

        private const string LogID = "ExtendedVariants/NewSwimmingSpeed";

        public override object ConvertLegacyVariantValue(int value) {
            return (float)value;
        }

        public override object GetDefaultVariantValue() {
            return VanillaSpeed;
        }

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override void Load() {
            IL.Celeste.Player.SwimUpdate += Player_SwimUpdate;
        }

        public override void Unload() {
            IL.Celeste.Player.SwimUpdate -= Player_SwimUpdate;
        }

        private void Player_SwimUpdate(ILContext il) {
            ILCursor xSpeedVariable = new ILCursor(il);
            if (!xSpeedVariable.TryGotoNext(MoveType.AfterLabel, static instr => instr.MatchStloc(2))) {
                Logger.Log(LogLevel.Error, LogID,
                    $"Could not find X water speed variable [stloc.2] in {il.Method.FullName}!");
                return;
            }

            ILCursor ySpeedVariable = xSpeedVariable.Clone();
            if (!ySpeedVariable.TryGotoNext(MoveType.Before, static instr => instr.MatchStloc(3))) {
                Logger.Log(LogLevel.Error, LogID,
                    $"Could not find Y water speed variable [stloc.3] in {il.Method.FullName}!");
                return;
            }

            ILCursor xIfCheck = ySpeedVariable.Clone();
            if (!TryGotoNextBestFit(xIfCheck, MoveType.Before,
                static instr => instr.MatchLdcR4(SlowerDecelerationSpeedThreshold),
                static instr => instr.MatchBleUn(out _)))
            {
                Logger.Log(LogLevel.Error, LogID,
                    $"Could not find X deceleration check [ldc.r4 80, ble.un.s IL_0165] in {il.Method.FullName}!");
                return;
            }
            xIfCheck.Index++;

            ILCursor yIfCheck = xIfCheck.Clone();
            if (!TryGotoNextBestFit(yIfCheck, MoveType.Before,
                static instr => instr.MatchLdcR4(SlowerDecelerationSpeedThreshold),
                static instr => instr.MatchBleUn(out _)))
            {
                Logger.Log(LogLevel.Error, LogID,
                    $"Could not find Y deceleration check [ldc.r4 80, ble.un.s IL_0165] in {il.Method.FullName}!");
                return;
            }
            yIfCheck.Index++;

            Logger.Log(LogLevel.Verbose, LogID,
                $"Overriding water X speed at {ToString(xSpeedVariable.Next)}");
            xSpeedVariable.EmitDelegate<Func<float, float>>(OverrideWaterXSpeed);

            Logger.Log(LogLevel.Verbose, LogID,
                $"Overriding water Y speed at {ToString(ySpeedVariable.Next)}");
            ySpeedVariable.Emit(OpCodes.Ldloc_0);
            ySpeedVariable.EmitDelegate<Func<float, bool, float>>(OverrideWaterYSpeed);

            Logger.Log(LogLevel.Verbose, LogID,
                $"Replacing 80f constant in X deceleration check at {ToString(xIfCheck.Next)}");
            xIfCheck.Emit(OpCodes.Ldloc_2);
            xIfCheck.Emit(OpCodes.Ldloc_0);
            xIfCheck.EmitDelegate<Func<float, float, bool, float>>(OverrideXSwimSpeedThreshold);

            Logger.Log(LogLevel.Verbose, LogID,
                $"Replacing 80f constant in Y deceleration check at {ToString(yIfCheck.Next)}");
            yIfCheck.Emit(OpCodes.Ldloc_3);
            yIfCheck.Emit(OpCodes.Ldloc_0);
            yIfCheck.EmitDelegate<Func<float, float, bool, float>>(OverrideYSwimSpeedThreshold);
        }

        private float OverrideWaterXSpeed(float previousSpeedX) {
            return previousSpeedX switch {
                UnderwaterSpeedX.VanillaSpeed => GetVariantValue<float>(Variant.UnderwaterSpeedX),
                WaterSurfaceSpeedX.VanillaSpeed => GetVariantValue<float>(Variant.WaterSurfaceSpeedX),

                // this should never happen, do nothing
                _ => previousSpeedX
            };
        }

        private float OverrideWaterYSpeed(float previousSpeedY, bool isUnderwater) {
            // this should never happen, do nothing
            if (previousSpeedY != UnderwaterSpeedY.VanillaSpeed)
                return previousSpeedY;

            return isUnderwater
                ? GetVariantValue<float>(Variant.UnderwaterSpeedY)
                : GetVariantValue<float>(Variant.WaterSurfaceSpeedY);
        }

        private float OverrideXSwimSpeedThreshold(float previousThreshold, float swimSpeedX, bool isUnderwater) {
            if (isUnderwater)
                return GetVariantValue<float>(Variant.UnderwaterSpeedX) == UnderwaterSpeedX.VanillaSpeed
                    ? previousThreshold
                    : swimSpeedX;
            else
                return GetVariantValue<float>(Variant.WaterSurfaceSpeedX) == WaterSurfaceSpeedX.VanillaSpeed
                    ? previousThreshold
                    : swimSpeedX;
        }

        private float OverrideYSwimSpeedThreshold(float previousThreshold, float swimSpeedY, bool isUnderwater) {
            if (isUnderwater)
                return GetVariantValue<float>(Variant.UnderwaterSpeedY) == UnderwaterSpeedY.VanillaSpeed
                    ? previousThreshold
                    : swimSpeedY;
            else
                return GetVariantValue<float>(Variant.WaterSurfaceSpeedY) == WaterSurfaceSpeedY.VanillaSpeed
                    ? previousThreshold
                    : swimSpeedY;
        }

        private static string ToString(Instruction instr) {
            if (instr.Operand is not ILLabel label)
                return instr.ToString();

            // MonoMod's ILLabels mess up Instruction.ToString() and crash when ToString()ing a branch instruction
            // thanks, MonoMod - very cool
            Instruction @fixed = Instruction.Create(instr.OpCode, label.Target);
            @fixed.Offset = instr.Offset;
            return @fixed.ToString();
        }
    }
}
