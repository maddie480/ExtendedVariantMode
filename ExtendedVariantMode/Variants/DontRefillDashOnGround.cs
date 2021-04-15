using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace ExtendedVariants.Variants {
    class DontRefillDashOnGround : AbstractExtendedVariant {
        private ILHook patchOrigUpdate;
        private ILHook patchSeekerRegenerateRoutine;

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.DashRefillOnGroundState;
        }

        public override void SetValue(int value) {
            Settings.DashRefillOnGroundState = value;
        }

        public override void Load() {
            // this is everywhere the NoRefills inventory field is checked before refilling dashes.
            IL.Celeste.Player.Bounce += patchNoRefills;
            IL.Celeste.Player.SuperBounce += patchNoRefills;
            IL.Celeste.Player.SideBounce += patchNoRefills;
            IL.Celeste.Player.DreamDashEnd += patchNoRefills;

            // for some reason, hooking ExplodeLaunch makes the game explode on launching the method (... yeah)
            // so, hook usages of ExplodeLaunch instead and kill RefillDash while they run.
            On.Celeste.Bumper.OnPlayer += patchBumperOnPlayer;
            On.Celeste.Puffer.Explode += patchPufferExplode;
            On.Celeste.TempleBigEyeball.OnPlayer += patchTempleBigEyeballOnPlayer;

            patchOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update"), patchNoRefills);
            patchSeekerRegenerateRoutine = new ILHook(typeof(Seeker).GetMethod("RegenerateCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), patchSeekerRegenerateCoroutine);
        }

        public override void Unload() {
            IL.Celeste.Player.Bounce -= patchNoRefills;
            IL.Celeste.Player.SuperBounce -= patchNoRefills;
            IL.Celeste.Player.SideBounce -= patchNoRefills;
            IL.Celeste.Player.DreamDashEnd -= patchNoRefills;

            On.Celeste.Bumper.OnPlayer -= patchBumperOnPlayer;
            On.Celeste.Puffer.Explode -= patchPufferExplode;
            On.Celeste.TempleBigEyeball.OnPlayer -= patchTempleBigEyeballOnPlayer;

            patchOrigUpdate?.Dispose();
            patchOrigUpdate = null;

            patchSeekerRegenerateRoutine?.Dispose();
            patchSeekerRegenerateRoutine = null;
        }

        private void patchNoRefills(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to if(!Inventory.NoRefills)
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<PlayerInventory>("NoRefills"))) {
                Logger.Log("ExtendedVariantMode/DontRefillDashOnGround", $"Patching no refill condition at {cursor.Index} in IL code for Player.{il.Method.Name}");
                cursor.EmitDelegate<Func<bool, bool>>(areRefillsOnGroundDisabled);
            }
        }

        private void patchBumperOnPlayer(On.Celeste.Bumper.orig_OnPlayer orig, Bumper self, Player player) {
            swapNoRefillsTemporarily(() => orig(self, player), self.SceneAs<Level>().Session);
        }

        private void patchPufferExplode(On.Celeste.Puffer.orig_Explode orig, Puffer self) {
            swapNoRefillsTemporarily(() => orig(self), self.SceneAs<Level>().Session);
        }

        private void patchTempleBigEyeballOnPlayer(On.Celeste.TempleBigEyeball.orig_OnPlayer orig, TempleBigEyeball self, Player player) {
            swapNoRefillsTemporarily(() => orig(self, player), self.SceneAs<Level>().Session);
        }

        private void swapNoRefillsTemporarily(Action action, Session session) {
            bool origNoRefills = session.Inventory.NoRefills;
            session.Inventory.NoRefills = areRefillsOnGroundDisabled(origNoRefills);
            action();
            session.Inventory.NoRefills = origNoRefills;
        }

        private bool areRefillsOnGroundDisabled(bool vanilla) {
            switch (Settings.DashRefillOnGroundState) {
                case 0: return vanilla;
                case 2: return false;
                default: return true;
            }
        }

        private void patchSeekerRegenerateCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            bool origNoRefills = false;

            // position the cursor just after yield return 0.15f;
            if (cursor.TryGotoNext(instr => instr.MatchLdcR4(0.15f))
                && cursor.TryGotoNext(instr => instr.MatchRet())
                && cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchLdarg(0))) {

                Logger.Log("ExtendedVariantMode/DontRefillDashOnGround", $"Patching in no refills at {cursor.Index} in IL for Seeker.RegenerateCoroutine");

                FieldInfo f_this = typeof(Seeker).GetMethod("RegenerateCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget().DeclaringType.GetField("<>4__this");

                // take away refills if needed.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f_this);
                cursor.EmitDelegate<Action<Seeker>>(self => {
                    origNoRefills = self.SceneAs<Level>().Session.Inventory.NoRefills;
                    self.SceneAs<Level>().Session.Inventory.NoRefills = areRefillsOnGroundDisabled(origNoRefills);
                });

                // jump just before the following yield return
                cursor.GotoNext(instr => instr.MatchRet());

                // restore the normal no refills value.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f_this);
                cursor.EmitDelegate<Action<Seeker>>(self => self.SceneAs<Level>().Session.Inventory.NoRefills = origNoRefills);
            }
        }
    }
}
