using Celeste;
using Celeste.Mod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;

namespace ExtendedVariants.Variants {
    class DontRefillDashOnGround : AbstractExtendedVariant {
        private ILHook patchOrigUpdate;

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
            On.Celeste.Seeker.RegenerateCoroutine += patchSeekerRegenerateCoroutine;

            patchOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update"), patchNoRefills);
        }

        public override void Unload() {
            IL.Celeste.Player.Bounce -= patchNoRefills;
            IL.Celeste.Player.SuperBounce -= patchNoRefills;
            IL.Celeste.Player.SideBounce -= patchNoRefills;
            IL.Celeste.Player.DreamDashEnd -= patchNoRefills;

            On.Celeste.Bumper.OnPlayer -= patchBumperOnPlayer;
            On.Celeste.Puffer.Explode -= patchPufferExplode;
            On.Celeste.TempleBigEyeball.OnPlayer -= patchTempleBigEyeballOnPlayer;
            On.Celeste.Seeker.RegenerateCoroutine -= patchSeekerRegenerateCoroutine;

            patchOrigUpdate?.Dispose();
            patchOrigUpdate = null;
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

        private IEnumerator patchSeekerRegenerateCoroutine(On.Celeste.Seeker.orig_RegenerateCoroutine orig, Seeker self) {
            IEnumerator original = orig(self);

            Session session = self.SceneAs<Level>().Session;
            bool origNoRefills = false;

            while (original.MoveNext()) {
                yield return original.Current;
                if (original.Current != null && original.Current.GetType() == typeof(float) && (float) original.Current == 0.15f) {
                    // modify dash refills between the last "yield return" and the end of the method.
                    origNoRefills = session.Inventory.NoRefills;
                    session.Inventory.NoRefills = areRefillsOnGroundDisabled(origNoRefills);
                }
            }
            session.Inventory.NoRefills = origNoRefills;
        }
    }
}
