using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class WindCrouchMove : AbstractExtendedVariant {
        private ILHook hookOnOrigWindMove;
        private static readonly MethodInfo m_get_Ducking = typeof(Player).GetMethod("get_Ducking", BindingFlags.Public | BindingFlags.Instance);

        public WindCrouchMove() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load()
        {
            hookOnOrigWindMove = new ILHook(typeof(Player).GetMethod("orig_WindMove", BindingFlags.NonPublic | BindingFlags.Instance), modWindMove);
        }

        public override void Unload() {
            hookOnOrigWindMove?.Dispose();
            hookOnOrigWindMove = null;
        }

        private void modWindMove (ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt(m_get_Ducking))) {
                Logger.Log("ExtendedVariantMode/WindCrouchMove", $"Enabling wind moving the player while crouched at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<bool, bool>>(modDuckCheck);
            }
        }

        private bool modDuckCheck(bool orig) {
            if (GetVariantValue<bool>(Variant.WindCrouchMove)) {
                return false;
            }

            return orig;
        }
    }
}
