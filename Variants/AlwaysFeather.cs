using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class AlwaysFeather : AbstractExtendedVariant {

        private const string FeatherSFX = "event:/game/06_reflection/feather_get";
        private static bool IsFeatherForced, WasFeatherForced;
        private static bool WasAlreadyInFeather;

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override void Load() {
            On.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
            IL.Celeste.Player.StarFlyUpdate += modPlayerStarFlyUpdate;

            On.Celeste.Player.Update += modPlayerUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.NormalUpdate -= modPlayerNormalUpdate;
            IL.Celeste.Player.StarFlyUpdate -= modPlayerStarFlyUpdate;

            On.Celeste.Player.Update -= modPlayerUpdate;
        }

        private static int modPlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self) {
            if (!IsFeatherForced) {
                // just call orig if variant is disabled
                return orig(self);
            }

            // else force them into feather
            self.StartStarFly();
            Audio.Play(FeatherSFX, self.Position);
            return Player.StStarFly;
        }

        private static void modPlayerStarFlyUpdate(ILContext il) {
            // skip all the parts that can make the player exit StStarFly

            //+if (IsFeatherForced)
            //+    return Player.StStarFly;
            //+else if (WasFeatherForced && !WasAlreadyInFeather)
            //+    return Player.StNormal;
            //+
            // if (Input.Jump.Pressed)

            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.AfterLabel,
                // can't use generic overload as Celeste.Input is static
                instr => instr.MatchLdsfld("Celeste.Input", "Jump"),
                instr => instr.MatchCallvirt<VirtualButton>("get_Pressed")
            )) {
                Logger.Log("ExtendedVariantMode/AlwaysFeather", $"Modding {il.Method.Name} to skip StStarFly exit actions at {cursor.Index}");

                ILLabel @continue = cursor.DefineLabel();
                cursor.Emit<AlwaysFeather>(OpCodes.Ldsfld, nameof(IsFeatherForced));
                cursor.Emit(OpCodes.Brfalse, @continue);
                cursor.Emit(OpCodes.Ldc_I4_S, (sbyte) Player.StStarFly);
                cursor.Emit(OpCodes.Ret);
                cursor.MarkLabel(@continue);

                @continue = cursor.DefineLabel();
                cursor.Emit<AlwaysFeather>(OpCodes.Ldsfld, nameof(WasFeatherForced));
                cursor.Emit<AlwaysFeather>(OpCodes.Ldsfld, nameof(WasAlreadyInFeather));
                cursor.Emit(OpCodes.Not);
                cursor.Emit(OpCodes.And);
                cursor.Emit(OpCodes.Brfalse, @continue);
                cursor.Emit(OpCodes.Ldc_I4_0); // Player.StNormal
                cursor.Emit(OpCodes.Ret);
                cursor.MarkLabel(@continue);
            }
        }


        private void modPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            WasFeatherForced = IsFeatherForced;
            IsFeatherForced = GetVariantValue<bool>(Variant.AlwaysFeather);

            orig(self);

            if (!IsFeatherForced) {
                WasAlreadyInFeather = self.StateMachine.State == Player.StStarFly;
            }
        }
    }
}
