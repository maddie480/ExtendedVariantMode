using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace ExtendedVariants.Entities.ForMappers {
    public class AbstractExtendedVariantTriggerTeleportHandler {
        // === hook on teleport to sync up a variant change with a teleport
        // since all teleport triggers call UnloadLevel(), we can hook that to detect the instant the teleport happens at.
        // this is in a separate class because we don't want it for each generic type.

        internal static event Action onTeleport;

        public static void Load() {
            On.Celeste.Level.UnloadLevel += onUnloadLevel;
        }

        public static void Unload() {
            On.Celeste.Level.UnloadLevel -= onUnloadLevel;
            onTeleport = null;
        }

        private static void onUnloadLevel(On.Celeste.Level.orig_UnloadLevel orig, Level self) {
            if (onTeleport != null) {
                onTeleport();
                onTeleport = null;
            }

            orig(self);
        }
    }

    public abstract class AbstractExtendedVariantTrigger<T> : Trigger {
        private ExtendedVariantsModule.Variant variantChange;
        private T newValue;
        private bool revertOnLeave;
        private bool revertOnDeath;
        private bool delayRevertOnDeath;
        private bool withTeleport;
        private bool coversScreen;
        private bool onlyOnce;

        public AbstractExtendedVariantTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            // parse the trigger parameters
            variantChange = getVariant(data);
            newValue = getNewValue(data);
            revertOnLeave = data.Bool("revertOnLeave", false);
            revertOnDeath = data.Bool("revertOnDeath", true);
            delayRevertOnDeath = data.Bool("delayRevertOnDeath", false);
            withTeleport = data.Bool("withTeleport", false);
            coversScreen = data.Bool("coversScreen", false);
            onlyOnce = data.Bool("onlyOnce", false);

            if (!data.Bool("enable", true)) {
                // "disabling" a variant is actually just resetting its value to default
                newValue = (T) ExtendedVariantTriggerManager.GetDefaultValueForVariant(variantChange);
            }

            // this is a replacement for the Flag-Toggled Extended Variant Trigger.
            if (!string.IsNullOrEmpty(data.Attr("flag"))) {
                Add(new FlagToggleComponent(data.Attr("flag"), data.Bool("flagInverted")));
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            Rectangle bounds = (scene as Level).Bounds;

            // this is a replacement for the Extended Variant Controller.
            if (coversScreen) {
                // the trigger should stick out on the top because the player can go offscreen by up to 24px when there is no screen above.
                Position = new Vector2(bounds.X, bounds.Y - 24f);
                Collider.Width = bounds.Width;
                Collider.Height = bounds.Height + 32f;
            }
        }

        protected virtual ExtendedVariantsModule.Variant getVariant(EntityData data) {
            return data.Enum("variantChange", ExtendedVariantsModule.Variant.Gravity);
        }

        protected abstract T getNewValue(EntityData data);

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            Action applyVariant = () =>
                ExtendedVariantsModule.Instance.TriggerManager.OnEnteredInTrigger(variantChange, newValue, revertOnLeave, isFade: false, revertOnDeath, legacy: false);

            if (withTeleport) {
                AbstractExtendedVariantTriggerTeleportHandler.onTeleport += applyVariant;
            } else {
                applyVariant();
            }

            if (onlyOnce) {
                RemoveSelf();
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if (revertOnLeave && (!delayRevertOnDeath || !player.Dead)) {
                ExtendedVariantsModule.Instance.TriggerManager.OnExitedRevertOnLeaveTrigger(variantChange, newValue, legacy: false);
            }
        }

        // This comes from the flag-toggled camera triggers in max480's Helping Hand, which come from the Spring Collab.
        // Recycling is good.
        internal class FlagToggleComponent : Component {
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
