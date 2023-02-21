using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using System;
using System.Collections.Generic;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants.Vanilla {
    public abstract class AbstractVanillaVariant : AbstractExtendedVariant {
        private static bool vanillaVariantsHooked = false;

        protected static Assists vanillaAssists;
        protected static bool overriding;

        public override void Load() {
            if (!vanillaVariantsHooked) {
                On.Celeste.Level.Update += onLevelUpdate;
                On.Celeste.Level.Render += onLevelRender;
                Everest.Events.Player.OnSpawn += onPlayerSpawn;

                vanillaVariantsHooked = true;
            }
        }

        public override void Unload() {
            if (vanillaVariantsHooked) {
                On.Celeste.Level.Update -= onLevelUpdate;
                On.Celeste.Level.Render -= onLevelRender;
                Everest.Events.Player.OnSpawn -= onPlayerSpawn;

                vanillaVariantsHooked = false;
            }
        }

        private void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            swapOutForDurationOfOrigCall(() => orig(self));
        }

        private void onLevelRender(On.Celeste.Level.orig_Render orig, Level self) {
            swapOutForDurationOfOrigCall(() => orig(self));
        }

        private void onPlayerSpawn(Player player) {
            Assists.DashModes mapDefinedValue = (Assists.DashModes) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(Variant.AirDashes);
            if (mapDefinedValue != SaveData.Instance.Assists.DashMode) {
                // make sure the dash count is applied right away when the player died and Air Dashes was a revert on death variant.
                SaveData.Instance.Assists.DashMode = mapDefinedValue;
                player.Dashes = player.MaxDashes;
            }
        }

        private void swapOutForDurationOfOrigCall(Action orig) {
            vanillaAssists = SaveData.Instance.Assists;
            Assists newAssists = applyAssists(vanillaAssists, out overriding);

            if (overriding) {
                SaveData.Instance.Assists = newAssists;
                orig();
                SaveData.Instance.Assists = vanillaAssists;
            } else {
                orig();
            }
        }

        public void VariantValueChangedByPlayer(object newValue) {
            if (overriding) {
                vanillaAssists = applyVariantValue(vanillaAssists, newValue);
            } else {
                SaveData.Instance.Assists = applyVariantValue(SaveData.Instance.Assists, newValue);
                vanillaAssists = SaveData.Instance.Assists;
            }
        }

        protected abstract Assists applyVariantValue(Assists target, object value);

        protected Assists applyAssists(Assists target, out bool updated) {
            updated = false;

            foreach (KeyValuePair<ExtendedVariantsModule.Variant, AbstractExtendedVariant> variant in ExtendedVariantsModule.Instance.VariantHandlers) {
                if (!(variant.Value is AbstractVanillaVariant vanillaVariant)) continue;

                object value = ExtendedVariantsModule.Instance.TriggerManager.GetCurrentMapDefinedVariantValue(variant.Key);
                if (!ExtendedVariantsModule.Session.VariantsOverridenByUser.Contains(variant.Key)
                    && !ExtendedVariantTriggerManager.AreValuesIdentical(value, vanillaVariant.GetDefaultVariantValue())) {

                    target = vanillaVariant.applyVariantValue(target, value);
                    updated = true;
                }
            }
            return target;
        }
    }
}
