using System;
using Celeste;
using ExtendedVariants.Module;
using Monocle;

namespace ExtendedVariants.Variants {
    public class BufferableGrab : AbstractExtendedVariant {
        public override void Load() => On.Celeste.Player.Added += Player_Added;

        public override void Unload() => On.Celeste.Player.Added -= Player_Added;

        public BufferableGrab() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) => value != 0;

        public override void VariantValueChanged() => UpdateBuffer();

        private static void UpdateBuffer() => Input.Grab.BufferTime = GetVariantValue<bool>(ExtendedVariantsModule.Variant.BufferableGrab) ? 0.08f : 0f;

        private static void Player_Added(On.Celeste.Player.orig_Added added, Player player, Scene scene) {
            added(player, scene);
            UpdateBuffer();
        }
    }
}