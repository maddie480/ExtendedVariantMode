using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedVariants.Variants {
    class MadelineIsSilhouette : AbstractExtendedVariant {
        private static List<ILHook> doneILHooks = new List<ILHook>();
        private static MethodInfo refreshPlayerSpriteMode = null;
        private static bool previouslyEnabled = false;

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.MadelineIsSilhouette ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.MadelineIsSilhouette = (value != 0);
        }

        public override void Load() {
            if (ExtendedVariantsModule.Instance.MaxHelpingHandInstalled) {
                // hook Max Helping Hand with sick reflection
                Assembly assembly = Everest.Modules.Where(m => m.Metadata?.Name == "MaxHelpingHand").First().GetType().Assembly;
                Type madelineSilhouetteTrigger = assembly.GetType("Celeste.Mod.MaxHelpingHand.Triggers.MadelineSilhouetteTrigger");
                doneILHooks.Add(new ILHook(madelineSilhouetteTrigger.GetMethod("onPlayerSpriteConstructor", BindingFlags.NonPublic | BindingFlags.Static), hookMadelineIsSilhouette));
                doneILHooks.Add(new ILHook(madelineSilhouetteTrigger.GetNestedType("<>c", BindingFlags.NonPublic)
                    .GetMethod("<patchPlayerRender>b__4_1", BindingFlags.NonPublic | BindingFlags.Instance), hookMadelineIsSilhouette));
                doneILHooks.Add(new ILHook(madelineSilhouetteTrigger.GetNestedType("<>c", BindingFlags.NonPublic)
                    .GetMethod("<patchPlayerRender>b__4_3", BindingFlags.NonPublic | BindingFlags.Instance), hookMadelineIsSilhouette));
                refreshPlayerSpriteMode = madelineSilhouetteTrigger.GetMethod("refreshPlayerSpriteMode", BindingFlags.NonPublic | BindingFlags.Static);
            } else if (ExtendedVariantsModule.Instance.SpringCollab2020Installed) {
                // hook Spring Collab 2020 with equally sick reflection but a bit less <>c
                Assembly assembly = Everest.Modules.Where(m => m.Metadata?.Name == "SpringCollab2020").First().GetType().Assembly;
                Type madelineSilhouetteTrigger = assembly.GetType("Celeste.Mod.SpringCollab2020.Triggers.MadelineSilhouetteTrigger");
                doneILHooks.Add(new ILHook(madelineSilhouetteTrigger.GetMethod("onPlayerAdded", BindingFlags.NonPublic | BindingFlags.Static), hookMadelineIsSilhouette));
                doneILHooks.Add(new ILHook(madelineSilhouetteTrigger.GetNestedType("<>c", BindingFlags.NonPublic)
                    .GetMethod("<patchPlayerRender>b__4_0", BindingFlags.NonPublic | BindingFlags.Instance), hookMadelineIsSilhouette));
                doneILHooks.Add(new ILHook(madelineSilhouetteTrigger.GetMethod("GetMadelineColor", BindingFlags.Public | BindingFlags.Static), hookMadelineIsSilhouette));
                refreshPlayerSpriteMode = madelineSilhouetteTrigger.GetMethod("refreshPlayerSpriteMode", BindingFlags.NonPublic | BindingFlags.Static);
            }
        }

        public override void Unload() {
            foreach (ILHook h in doneILHooks) {
                h.Dispose();
            }
            doneILHooks.Clear();
            refreshPlayerSpriteMode = null;
        }

        private void hookMadelineIsSilhouette(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Callvirt && (instr.Operand as MethodReference).Name == "get_MadelineIsSilhouette")) {
                Logger.Log("ExtendedVariantMode/MadelineIsSilhouette", $"Hooking MadelineIsSilhouette at {cursor.Index} in IL for {cursor.Method.FullName}");
                cursor.EmitDelegate<Func<bool, bool>>(orig => {
                    if (previouslyEnabled != Settings.MadelineIsSilhouette) {
                        previouslyEnabled = Settings.MadelineIsSilhouette;
                        runRefreshPlayerSpriteMode(orig || Settings.MadelineIsSilhouette);
                    }
                    if (Settings.MadelineIsSilhouette) {
                        return true;
                    }
                    return orig;
                });
            }
        }

        private void runRefreshPlayerSpriteMode(bool enable) {
            Player player = Engine.Scene?.Tracker?.GetEntity<Player>();
            if (refreshPlayerSpriteMode != null && player != null) {
                Logger.Log(LogLevel.Debug, "ExtendedVariantMode/MadelineIsSilhouette", "Running refreshPlayerSpriteMode()");
                refreshPlayerSpriteMode.Invoke(null, new object[] { player, enable });
            }
        }
    }
}
