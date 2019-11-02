using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    class DontRefillDashOnGround : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.DontRefillDashOnGround ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.DontRefillDashOnGround = (value != 0);
        }

        public override void Load() {
            // this is everywhere the NoRefills inventory field is checked before refilling dashes.
            IL.Celeste.Player.Bounce += patchNoRefills;
            IL.Celeste.Player.SuperBounce += patchNoRefills;
            IL.Celeste.Player.SideBounce += patchNoRefills;
            // IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += patchNoRefills;
            IL.Celeste.Player.DreamDashEnd += patchNoRefills;

            On.Celeste.Player.Update += patchUpdate;
            On.Celeste.Player.RefillDash += killRefillDashIfNeeded;
        }

        public override void Unload() {
            IL.Celeste.Player.Bounce -= patchNoRefills;
            IL.Celeste.Player.SuperBounce -= patchNoRefills;
            IL.Celeste.Player.SideBounce -= patchNoRefills;
            // IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= patchNoRefills;
            IL.Celeste.Player.DreamDashEnd -= patchNoRefills;

            On.Celeste.Player.Update -= patchUpdate;
            On.Celeste.Player.RefillDash -= killRefillDashIfNeeded;
        }

        private void patchNoRefills(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to if(!Inventory.NoRefills)
            while(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<PlayerInventory>("NoRefills"))) {
                Logger.Log("ExtendedVariantMode", $"Patching no refill condition at {cursor.Index} in IL code for Player.{il.Method.Name}");

                // turn it into if(!(Inventory.NoRefills || Settings.DontRefillDashOnGround))
                // => if(!Inventory.NoRefills && !Settings.DontRefillDashOnGround)
                cursor.EmitDelegate<Func<bool>>(areRefillsOnGroundDisabled);
                cursor.Emit(OpCodes.Or);
            }
        }

        bool killDashRefills = false;

        private bool killRefillDashIfNeeded(On.Celeste.Player.orig_RefillDash orig, Player self) {
            if (!Settings.DontRefillDashOnGround || !killDashRefills) {
                return orig(self);
            }
            // dash was NOT refilled.
            return false;
        }

        private void patchUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            // there are 3 invocations of RefillDash in Update: one of them enforces the Infinite Dashes assist,
            // the 2 others should be killed if DontRefillDashOnGround is enabled.
            killDashRefills = (Settings.DontRefillDashOnGround &&
                (SaveData.Instance.Assists.DashMode != Assists.DashModes.Infinite || self.SceneAs<Level>().InCutscene));

            orig(self);

            killDashRefills = false;
        }

        private bool areRefillsOnGroundDisabled() {
            return Settings.DontRefillDashOnGround;
        }
    }
}
