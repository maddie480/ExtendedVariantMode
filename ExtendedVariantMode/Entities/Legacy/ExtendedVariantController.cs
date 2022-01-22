using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace ExtendedVariants.Entities.Legacy {
    // An Extended Variant Controller is a controller that behaves like an Extended Variant Trigger that would cover the entire screen--
    // actually no, it _is_ an Extended Variant Trigger that covers the entire screen.
    [CustomEntity("ExtendedVariantMode/ExtendedVariantController")]
    static class ExtendedVariantController {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) =>
            new ExtendedVariantTrigger(new EntityData {
                Position = new Vector2(0, -24f), // the trigger should stick out on the top because the player can go offscreen by up to 24px when there is no screen above.
                Width = levelData.Bounds.Width,
                Height = levelData.Bounds.Height + 24,
                Values = entityData.Values
            }, offset);
    }
}
