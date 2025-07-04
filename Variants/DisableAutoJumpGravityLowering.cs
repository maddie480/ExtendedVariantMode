using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class DisableAutoJumpGravityLowering : AbstractExtendedVariant {
        public DisableAutoJumpGravityLowering() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            IL.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
        }

        private void modPlayerNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchBrtrue(out ILLabel _),
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdfld<Player>("AutoJump")
            )) {
                Logger.Log("ExtendedVariantMode/DisableJumpGravityLowering", $"Disabling AutoJump gravity lowering at {cursor.Index} in IL for Player.NormalUpdate");

                cursor.EmitDelegate<Func<bool, bool>>(orig => orig && !GetVariantValue<bool>(ExtendedVariantsModule.Variant.DisableAutoJumpGravityLowering));
            }
        }
    }
}
