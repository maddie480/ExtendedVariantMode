using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Entities.ForMappers;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace ExtendedVariants.Entities.Legacy {
    // obsoleted by "flag" and "flagInverted" options on the new Extended Variant Triggers.
    [CustomEntity("ExtendedVariantMode/FlagToggledExtendedVariantTrigger")]
    public static class FlagToggledExtendedVariantTrigger {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            Trigger trigger = new ExtendedVariantTrigger(entityData, offset);
            trigger.Add(new AbstractExtendedVariantTrigger<int>.FlagToggleComponent(entityData.Attr("flag"), entityData.Bool("inverted")));
            return trigger;
        }
    }
}
