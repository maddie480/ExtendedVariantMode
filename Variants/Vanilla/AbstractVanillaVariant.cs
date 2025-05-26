using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExtendedVariants.Variants.Vanilla {
    public abstract class AbstractVanillaVariant : AbstractExtendedVariant {
        private static bool vanillaVariantsHooked = false;
        private static ILHook isaGrabBagHook = null;

        private static Assists vanillaAssists = Assists.Default;

        protected AbstractVanillaVariant(Type variantType, object defaultVariantValue) : base(variantType, defaultVariantValue) { }

        public override void Load() {
            if (!vanillaVariantsHooked) {
                On.Celeste.Level.Update += onLevelUpdate;
                On.Celeste.Level.Render += onLevelRender;
                Everest.Events.Player.OnSpawn += onPlayerSpawn;

                vanillaVariantsHooked = true;
            }
        }

        public static void Initialize() {
            if (isaGrabBagHook == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "IsaGrabBag", Version = new Version(1, 6, 10) })) {
                MethodInfo variantMenuMethod = Everest.Modules.Where(m => m.Metadata?.Name == "IsaGrabBag").First().GetType().Assembly
                    .GetType("Celeste.Mod.IsaGrabBag.ForceVariants")
                    .GetMethod("OnVariantMenu", BindingFlags.NonPublic | BindingFlags.Static);

                isaGrabBagHook = new ILHook(variantMenuMethod, modIsaGrabBag);
            }
        }

        private static void modIsaGrabBag(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(instr => instr.MatchIsinst<TextMenu.OnOff>())) {
                Logger.Log("ExtendedVariantMode/AbstractVanillaVariant", $"Adding extended variant mod option menu support at {cursor.Index} in IL for ForceVariants.OnVariantMenu");

                cursor.Emit(OpCodes.Dup);
                cursor.Index++;

                // if (item is TextMenu.OnOff) => if (item is TextMenu.OnOff || item is TextMenuExt.OnOff)
                // TextMenuExt.OnOff doesn't extend TextMenu.OnOff but does extend TextMenu.Option, so Isa's Grab Bag should just work (tm) with them.
                cursor.EmitDelegate<Func<TextMenu.Item, TextMenu.OnOff, TextMenu.Item>>((item, itemCast) => {
                    if (itemCast != null) return itemCast;
                    if (item is ExtendedVariants.UI.TextMenuExt.OnOff onOff) return onOff;
                    return null;
                });
            }
        }

        public override void Unload() {
            if (vanillaVariantsHooked) {
                On.Celeste.Level.Update -= onLevelUpdate;
                On.Celeste.Level.Render -= onLevelRender;
                Everest.Events.Player.OnSpawn -= onPlayerSpawn;

                vanillaVariantsHooked = false;
            }

            isaGrabBagHook?.Dispose();
            isaGrabBagHook = null;
        }

        private void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            swapOutForDurationOfOrigCall(() => orig(self));
        }

        private void onLevelRender(On.Celeste.Level.orig_Render orig, Level self) {
            swapOutForDurationOfOrigCall(() => orig(self));
        }

        private void onPlayerSpawn(Player player) {
            Assists.DashModes mapDefinedValue = getActiveAssistValues().DashMode;
            if (mapDefinedValue != SaveData.Instance.Assists.DashMode) {
                // make sure the dash count is applied right away when the player died and Air Dashes was a revert on death variant.
                SaveData.Instance.Assists.DashMode = mapDefinedValue;
                player.Dashes = player.MaxDashes;
            }
        }

        private void swapOutForDurationOfOrigCall(Action orig) {
            vanillaAssists = SaveData.Instance.Assists;
            Assists newAssists = applyAssists(vanillaAssists, out bool hasMapDefinedVariants);

            if (hasMapDefinedVariants) {
                // Apply the map-defined variants for the duration of the update/render,
                // so that they "naturally" get reverted when leaving the map.
                SaveData.Instance.Assists = newAssists;
                orig();
                SaveData.Instance.Assists = vanillaAssists;
            } else {
                // We have no reason to mess with SaveData.Instance.Assists!
                orig();
            }
        }

        public void VariantValueChangedByPlayer(object newValue) {
            // Apply to both vanilla assists and overridden assists.
            vanillaAssists = applyVariantValue(vanillaAssists, newValue);
            SaveData.Instance.Assists = applyVariantValue(SaveData.Instance.Assists, newValue);
        }

        public bool IsSetToDefaultByPlayer() {
            return applyVariantValue(vanillaAssists, GetDefaultVariantValue()).Equals(vanillaAssists);
        }

        protected abstract Assists applyVariantValue(Assists target, object value);

        private Assists applyAssists(Assists target, out bool updated) {
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

        protected Assists getActiveAssistValues() {
            return applyAssists(vanillaAssists, out _);
        }
    }
}
