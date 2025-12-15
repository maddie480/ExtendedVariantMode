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
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class MadelineHasPonytail : AbstractExtendedVariant {
        private static List<ILHook> doneILHooks = new List<ILHook>();

        public MadelineHasPonytail() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            // hook Max Helping Hand with sick reflection
            Assembly assembly = Everest.Modules.Where(m => m.Metadata?.Name == "MaxHelpingHand").First().GetType().Assembly;
            Type madelinePonytailTrigger = assembly.GetType("Celeste.Mod.MaxHelpingHand.Triggers.MadelinePonytailTrigger");
            hooklining(madelinePonytailTrigger.GetMethod("modHairScaleAndCount", BindingFlags.NonPublic | BindingFlags.Static));
            hooklining(madelinePonytailTrigger.GetMethod("makeHairLonger", BindingFlags.NonPublic | BindingFlags.Static));
            hooklining(madelinePonytailTrigger.GetMethod("hookHairColor", BindingFlags.NonPublic | BindingFlags.Static));
            hooklining(madelinePonytailTrigger.GetMethod("hookParticleColor", BindingFlags.NonPublic | BindingFlags.Static));
            hooklining(madelinePonytailTrigger.GetMethod("skipMadelineInWonderlandHook", BindingFlags.NonPublic | BindingFlags.Static));
        }

        private static void hooklining(MethodInfo method) {
            TryDisableInlining(method);
            doneILHooks.Add(new ILHook(method, hookMadelineHasPonytail));
        }


        public override void Unload() {
            foreach (ILHook h in doneILHooks) {
                h.Dispose();
            }
            doneILHooks.Clear();
        }

        private static void hookMadelineHasPonytail(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Callvirt && (instr.Operand as MethodReference).Name == "get_MadelineHasPonytail")) {
                Logger.Log("ExtendedVariantMode/MadelineHasPonytail", $"Hooking MadelineHasPonytail at {cursor.Index} in IL for {cursor.Method.FullName}");
                cursor.EmitDelegate<Func<bool, bool>>(applyMadelineHasPonytail);
            }
        }
        private static bool applyMadelineHasPonytail(bool orig) {
            if (GetVariantValue<bool>(Variant.MadelineHasPonytail)) {
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
        }
    }
}
