using System;
using System.Reflection;
using Celeste;
using ExtendedVariants.Module;
using MonoMod.RuntimeDetour;

namespace ExtendedVariants.Variants {
    public class TrueNoGrabbing : AbstractExtendedVariant {
        private Hook Celeste_Input_get_GrabCheck;

        public override void Load() => Celeste_Input_get_GrabCheck = new Hook(
            typeof(Input).GetProperty("GrabCheck", BindingFlags.Public | BindingFlags.Static).GetGetMethod(),
            typeof(TrueNoGrabbing).GetMethod("Input_get_GrabCheck", BindingFlags.NonPublic | BindingFlags.Instance), this);

        public override void Unload() => Celeste_Input_get_GrabCheck.Dispose();

        public override Type GetVariantType() => typeof(bool);

        public override object GetDefaultVariantValue() => false;

        public override object ConvertLegacyVariantValue(int value) => value != 0;

        private bool Input_get_GrabCheck(Func<bool> grabCheck) => !GetVariantValue<bool>(ExtendedVariantsModule.Variant.TrueNoGrabbing) && grabCheck();
    }
}