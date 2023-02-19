using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedVariants.Variants.Vanilla {
    public abstract class AbstractVanillaVariant : AbstractExtendedVariant {
        private static bool vanillaVariantsHooked = false;

        public override void Load() {
            if (!vanillaVariantsHooked) {
                On.Celeste.Player.Added += onPlayerAdded;
                On.Celeste.Level.Update += onLevelUpdate;
                On.Celeste.Level.Render += onLevelRender;

                vanillaVariantsHooked = true;
            }
        }

        public override void Unload() {
            if (vanillaVariantsHooked) {
                On.Celeste.Player.Added -= onPlayerAdded;
                On.Celeste.Level.Update -= onLevelUpdate;
                On.Celeste.Level.Render -= onLevelRender;

                vanillaVariantsHooked = false;
            }
        }

        private void onPlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            swapOutForDurationOfOrigCall(() => orig(self, scene));
        }

        private void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            swapOutForDurationOfOrigCall(() => orig(self));
        }

        private void onLevelRender(On.Celeste.Level.orig_Render orig, Level self) {
            swapOutForDurationOfOrigCall(() => orig(self));
        }

        private void swapOutForDurationOfOrigCall(Action orig) {
            Assists origAssists = SaveData.Instance.Assists;
            Assists newAssists = applyAssists(SaveData.Instance.Assists, out bool updated);

            if (updated) {
                SaveData.Instance.Assists = newAssists;
                orig();
                SaveData.Instance.Assists = origAssists;
            } else {
                orig();
            }
        }

        public override bool IsVanilla() {
            return true;
        }

        protected abstract Assists applyVariantValue(Assists target, object value);

        protected Assists applyAssists(Assists target, out bool updated) {
            updated = false;

            foreach (KeyValuePair<ExtendedVariantsModule.Variant, AbstractExtendedVariant> variant in ExtendedVariantsModule.Instance.VariantHandlers) {
                if (!(variant.Value is AbstractVanillaVariant vanillaVariant)) continue;

                object value = ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(variant.Key);
                if (value != vanillaVariant.GetDefaultVariantValue()) {
                    target = vanillaVariant.applyVariantValue(target, value);
                    updated = true;
                }
            }
            return target;
        }
    }
}
