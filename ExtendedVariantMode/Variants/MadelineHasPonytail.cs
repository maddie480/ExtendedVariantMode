using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExtendedVariants.Variants {
    class MadelineHasPonytail : AbstractExtendedVariant {
        private static List<ILHook> doneILHooks = new List<ILHook>();

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.MadelineHasPonytail ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.MadelineHasPonytail = (value != 0);
        }

        public override void Load() {
            // hook Max Helping Hand with sick reflection
            Assembly assembly = Everest.Modules.Where(m => m.Metadata?.Name == "MaxHelpingHand").First().GetType().Assembly;
            Type madelinePonytailTrigger = assembly.GetType("Celeste.Mod.MaxHelpingHand.Triggers.MadelinePonytailTrigger");
            doneILHooks.Add(new ILHook(madelinePonytailTrigger.GetMethod("hookHairColor", BindingFlags.NonPublic | BindingFlags.Static), hookMadelineHasPonytail));
            doneILHooks.Add(new ILHook(madelinePonytailTrigger.GetMethod("hookParticleColor", BindingFlags.NonPublic | BindingFlags.Static), hookMadelineHasPonytail));
            doneILHooks.Add(new ILHook(madelinePonytailTrigger.GetNestedType("<>c", BindingFlags.NonPublic)
                .GetMethod("<hookHairCount>b__8_0", BindingFlags.NonPublic | BindingFlags.Instance), hookMadelineHasPonytail));
            doneILHooks.Add(new ILHook(madelinePonytailTrigger.GetNestedType("<>c__DisplayClass6_0", BindingFlags.NonPublic)
                .GetMethod("<hookHairScaleAndHairCount>b__1", BindingFlags.NonPublic | BindingFlags.Instance), hookMadelineHasPonytail));
        }

        public override void Unload() {
            foreach (ILHook h in doneILHooks) {
                h.Dispose();
            }
            doneILHooks.Clear();
        }

        private void hookMadelineHasPonytail(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Callvirt && (instr.Operand as MethodReference).Name == "get_MadelineHasPonytail")) {
                Logger.Log("ExtendedVariantMode/MadelineHasPonytail", $"Hooking MadelineHasPonytail at {cursor.Index} in IL for {cursor.Method.FullName}");
                cursor.EmitDelegate<Func<bool, bool>>(orig => {
                    if (Settings.MadelineHasPonytail) {
                        // before returning true, ensure hair has enough nodes to handle the ponytail (6).
                        // this prevents crashes when enabling the variant in the middle of a level.
                        Player player;
                        if ((player = Engine.Scene?.Tracker?.GetEntity<Player>()) != null && player.Hair != null) {
                            while (player.Hair.Nodes.Count < 6) {
                                player.Hair.Nodes.Add(Vector2.Zero);
                            }
                        }
                        return true;
                    }
                    return orig;
                });
            }
        }
    }
}
