using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace ExtendedVariants.Variants;

public class ConsistentThrowing : AbstractExtendedVariant {
    public ConsistentThrowing() : base(variantType: typeof(bool), defaultVariantValue: false) { }

    public override object ConvertLegacyVariantValue(int value) {
        return value != 0;
    }

    public override void Load() {
        On.Celeste.Holdable.Release += MakeHoldableOffsetConsistent;
    }

    public override void Unload() {
        On.Celeste.Holdable.Release -= MakeHoldableOffsetConsistent;
    }

    private void MakeHoldableOffsetConsistent(On.Celeste.Holdable.orig_Release orig, Holdable self, Vector2 force) {
        if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.ConsistentThrowing))
        {
            Player holder = self.Holder;

            Vector2 holdablePosition = holder.Position
                + DynamicData.For(holder).Get<Vector2>("carryOffset")
                + Vector2.UnitY * (holder.Sprite.CarryYOffset / holder.Sprite.Scale.Y);
            self.Carry(holdablePosition.Floor());

            if (self.Entity is Actor actor)
            {
                actor.ZeroRemainderX();
                actor.ZeroRemainderY();
            }
        }
        orig(self, force);
    }
}
