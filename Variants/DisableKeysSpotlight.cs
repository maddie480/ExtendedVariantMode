using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class DisableKeysSpotlight : AbstractExtendedVariant {
        public DisableKeysSpotlight() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Key.ctor_Vector2_EntityID_Vector2Array += removeKeyLight;
        }

        public override void Unload() {
            On.Celeste.Key.ctor_Vector2_EntityID_Vector2Array -= removeKeyLight;
        }

        public override void VariantValueChanged() {
            if (!(Engine.Scene is Level)) return;

            if (GetVariantValue<bool>(Variant.DisableKeysSpotlight)) {
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
                        key.Add(key.light);
                    }
                }
            }
        }

        private static void removeKeyLight(On.Celeste.Key.orig_ctor_Vector2_EntityID_Vector2Array orig, Key self, Vector2 position, EntityID id, Vector2[] nodes) {
            orig(self, position, id, nodes);

            if (GetVariantValue<bool>(Variant.DisableKeysSpotlight)) {
                self.Get<VertexLight>().RemoveSelf();
            }
        }
    }
}
