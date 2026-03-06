using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    class HarmlessSeekers : AbstractExtendedVariant {

        public HarmlessSeekers() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Seeker.OnAttackPlayer += onSeekerAttackPlayer;
        }

        public override void Unload() {
            On.Celeste.Seeker.OnAttackPlayer -= onSeekerAttackPlayer;
        }

        private static void onSeekerAttackPlayer(On.Celeste.Seeker.orig_OnAttackPlayer orig, Seeker self, Player player) {
            if (GetVariantValue<bool>(Variant.HarmlessSeekers)) {
                Logger.Log(LogLevel.Debug, "ExtendedVariantMode/HarmlessSeekers", "Seeker attack prevented - variant enabled");
                return;
            }

            orig(self, player);
        }
    }
}