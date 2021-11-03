using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace ExtendedVariants {
    [CustomEntity("ExtendedVariantMode/FlagToggledExtendedVariantTrigger")]
    public static class FlagToggledExtendedVariantTrigger {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            Trigger trigger = new ExtendedVariantTrigger(entityData, offset);
            trigger.Add(new FlagToggleComponent(entityData.Attr("flag"), entityData.Bool("inverted")));
            return trigger;
        }

        // This comes from the flag-toggled camera triggers in max480's Helping Hand, which come from the Spring Collab.
        // Recycling is good.
        private class FlagToggleComponent : Component {
            public bool Enabled = true;
            private string flag;
            private Action onDisable;
            private Action onEnable;
            private bool inverted;

            public FlagToggleComponent(string flag, bool inverted, Action onDisable = null, Action onEnable = null) : base(true, false) {
                this.flag = flag;
                this.inverted = inverted;
                this.onDisable = onDisable;
                this.onEnable = onEnable;
            }

            public override void EntityAdded(Scene scene) {
                base.EntityAdded(scene);
                UpdateFlag();
            }

            public override void Update() {
                base.Update();
                UpdateFlag();
            }

            public void UpdateFlag() {
                if ((!inverted && SceneAs<Level>().Session.GetFlag(flag) != Enabled)
                    || (inverted && SceneAs<Level>().Session.GetFlag(flag) == Enabled)) {

                    if (Enabled) {
                        // disable the entity.
                        Entity.Visible = Entity.Collidable = false;
                        onDisable?.Invoke();
                        Enabled = false;
                    } else {
                        // enable the entity.
                        Entity.Visible = Entity.Collidable = true;
                        onEnable?.Invoke();
                        Enabled = true;
                    }
                }
            }
        }
    }
}
