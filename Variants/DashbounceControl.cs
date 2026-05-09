using System.Collections;
using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;

namespace ExtendedVariants.Variants {
    public class DashbounceControl : AbstractExtendedVariant {
        public enum DashbounceControlMode {
            Off,
            Never,
            Hold
        }

        public DashbounceControl() : base(variantType: typeof(DashbounceControlMode), defaultVariantValue: DashbounceControlMode.Off) { }

        public override void Load() => On.Celeste.Player.DashCoroutine += Player_DashCoroutine;

        public override void Unload() => On.Celeste.Player.DashCoroutine -= Player_DashCoroutine;

        public override object ConvertLegacyVariantValue(int value) => GetDefaultVariantValue();

        private static IEnumerator Player_DashCoroutine(On.Celeste.Player.orig_DashCoroutine dashCoroutine, Player player) {
            yield return new SwapImmediately(dashCoroutine(player));

            switch (GetVariantValue<DashbounceControlMode>(ExtendedVariantsModule.Variant.DashbounceControl)) {
                case DashbounceControlMode.Never:
                case DashbounceControlMode.Hold when !Input.Jump.Check:
                    player.varJumpTimer = 0f;

                    break;
            }
        }
    }
}