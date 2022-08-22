using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace ExtendedVariants.Variants {
    public class DisableKeysSpotlight : AbstractExtendedVariant {
        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object GetVariantValue() {
            return Settings.DisableKeysSpotlight;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.DisableKeysSpotlight = (bool) value;
            OnSettingChanged();
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.DisableKeysSpotlight = (value != 0);
            OnSettingChanged();
        }

        public override void Load() {
            On.Celeste.Key.ctor_Vector2_EntityID_Vector2Array += removeKeyLight;
        }

        public override void Unload() {
            On.Celeste.Key.ctor_Vector2_EntityID_Vector2Array -= removeKeyLight;
        }

        public void OnSettingChanged() {
            if (!(Engine.Scene is Level)) return;

            if (Settings.DisableKeysSpotlight) {
                // remove the light of all keys in the scene
                foreach (Key key in Engine.Scene.Entities.FindAll<Key>()) {
                    VertexLight light = key.Get<VertexLight>();
                    if (light != null) {
                        light.RemoveSelf();

                        // the game crashes if the light isn't attached to an entity (wtf)
                        new Entity().Add(light);
                    }
                }
            } else {
                // add back the light of all keys in the scene (it's still stored in a private field inside it)
                foreach (Key key in Engine.Scene.Entities.FindAll<Key>()) {
                    if (key.Get<VertexLight>() == null) {
                        key.Add(new DynData<Key>(key).Get<VertexLight>("light"));
                    }
                }
            }
        }

        private void removeKeyLight(On.Celeste.Key.orig_ctor_Vector2_EntityID_Vector2Array orig, Key self, Vector2 position, EntityID id, Vector2[] nodes) {
            orig(self, position, id, nodes);

            if (Settings.DisableKeysSpotlight) {
                self.Get<VertexLight>().RemoveSelf();
            }
        }
    }
}
