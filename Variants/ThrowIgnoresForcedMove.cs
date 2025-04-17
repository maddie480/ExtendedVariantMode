using System;
using Celeste;
using ExtendedVariants.Module;
using MonoMod.Cil;

namespace ExtendedVariants.Variants {
    public class ThrowIgnoresForcedMove : AbstractExtendedVariant {
        public override void Load() => IL.Celeste.Player.Throw += Player_Throw_il;

        public override void Unload() => IL.Celeste.Player.Throw -= Player_Throw_il;

        public ThrowIgnoresForcedMove() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) => value != 0;

        private void Player_Throw_il(ILContext il) {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Player>("Facing"))) {
                cursor.EmitDelegate<Func<Facings, Facings>>(facing => {
                    if (!GetVariantValue<bool>(ExtendedVariantsModule.Variant.ThrowIgnoresForcedMove))
                        return facing;

                    int moveX = Input.MoveX.Value;

                    return moveX != 0 ? (Facings) moveX : facing;
                });
            }
        }
    }
}