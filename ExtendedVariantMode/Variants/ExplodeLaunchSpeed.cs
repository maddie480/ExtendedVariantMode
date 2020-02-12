using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;

namespace ExtendedVariants.Variants {
    class ExplodeLaunchSpeed : AbstractExtendedVariant {

        private ILHook seekerRegenerateHook;

        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.ExplodeLaunchSpeed;
        }

        public override void SetValue(int value) {
            Settings.ExplodeLaunchSpeed = value;
        }

        public override void Load() {
            // we cannot hook ExplodeLaunch because of https://github.com/EverestAPI/Everest/issues/66
            // so we'll wrap each call of it instead.
            IL.Celeste.Bumper.OnPlayer += wrapExplodeLaunchCall;
            IL.Celeste.Puffer.Explode += wrapExplodeLaunchCall;
            IL.Celeste.TempleBigEyeball.OnPlayer += wrapExplodeLaunchCall;
            seekerRegenerateHook = ExtendedVariantsModule.HookCoroutine("Celeste.Seeker", "RegenerateCoroutine", wrapExplodeLaunchCall);
        }

        public override void Unload() {
            IL.Celeste.Bumper.OnPlayer -= wrapExplodeLaunchCall;
            IL.Celeste.Puffer.Explode -= wrapExplodeLaunchCall;
            IL.Celeste.TempleBigEyeball.OnPlayer -= wrapExplodeLaunchCall;
            if (seekerRegenerateHook != null) seekerRegenerateHook.Dispose();
        }

        private void wrapExplodeLaunchCall(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("ExplodeLaunch"))) {
                Logger.Log("ExtendedVariantMode/ExplodeLaunchSpeed", $"Adding call after ExplodeLaunch at {cursor.Index} in IL code for {il.Method.DeclaringType.Name}.{il.Method.Name}");
                cursor.EmitDelegate<Action>(correctExplodeLaunchSpeed);
            }
        }

        private void correctExplodeLaunchSpeed() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player != null) player.Speed *= (Settings.ExplodeLaunchSpeed / 10f);
        }
    }
}
