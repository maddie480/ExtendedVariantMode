using Celeste;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace ExtendedVariants.Variants {
    public class DashRestriction : AbstractExtendedVariant {
        private Hook canDashHook;

        public override object ConvertLegacyVariantValue(int value) {
            return GetDefaultVariantValue();
        }

        public override object GetDefaultVariantValue() {
            return DashRestrictionType.None;
        }

        public override Type GetVariantType() {
            return typeof(DashRestrictionType);
        }

        public override void Load() {
            canDashHook = new Hook(
                typeof(Player).GetProperty("CanDash").GetGetMethod(),
                modCanDash
            );
        }

        public override void Unload() {
            canDashHook?.Dispose(); // shouldn't be null if we're here, but just in case
        }

        private static bool modCanDash(Func<Player, bool> orig, Player self) {
            DashRestrictionType dashRestrictionType = (DashRestrictionType)ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(
                ExtendedVariantsModule.Variant.DashRestriction
            );

            if (dashRestrictionType == DashRestrictionType.None)
                return orig(self);

            bool isGrounded = self.OnGround() || DynamicData.For(self).Get<float>("jumpGraceTimer") > 0f;

            if ((dashRestrictionType == DashRestrictionType.GroundedOnly && !isGrounded) 
                || (dashRestrictionType == DashRestrictionType.AirborneOnly && isGrounded))
                return false;
            return orig(self);
        }

        public enum DashRestrictionType {
            None,
            GroundedOnly,
            AirborneOnly
        }
    }
}
