using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;

namespace ExtendedVariants.Variants {
    public class RestoreDashesOnRespawn : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.RestoreDashesOnRespawn ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.RestoreDashesOnRespawn = (value != 0);
        }

        public override void Load() {
            IL.Celeste.ChangeRespawnTrigger.OnEnter += modRespawnTriggerOnEnter;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;
            On.Celeste.Player.Added += onPlayerSpawn;
        }

        public override void Unload() {
            IL.Celeste.ChangeRespawnTrigger.OnEnter -= modRespawnTriggerOnEnter;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
            On.Celeste.Player.Added -= onPlayerSpawn;
        }

        // save dash count when hitting a change respawn trigger
        private void modRespawnTriggerOnEnter(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // simply jump into the "if" controlling whether the respawn should be changed or not
            // (yet again, this is brtrue.s in XNA and brfalse.s in FNA. Thanks compiler.)
            if (cursor.TryGotoNext(MoveType.After, instr => (instr.OpCode == OpCodes.Brtrue_S || instr.OpCode == OpCodes.Brfalse_S))) {
                Logger.Log("ExtendedVariantMode/ResetDashesOnRespawn", $"Adding save for dash count on respawn change at {cursor.Index} in CIL code for OnEnter in ChangeRespawnTrigger");
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Action<Player>>(player => ExtendedVariantsModule.Session.DashCountOnLatestRespawn = player.Dashes);
            }
        }

        // save dash count when changing screens
        private IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            ExtendedVariantsModule.Session.DashCountOnLatestRespawn = self.Tracker.GetEntity<Player>()?.Dashes ?? 0;
            return orig(self, next, direction);
        }

        private void onPlayerSpawn(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig(self, scene);

            if (Settings.RestoreDashesOnRespawn && ExtendedVariantsModule.Session.DashCountOnLatestRespawn != -1) {
                self.Dashes = ExtendedVariantsModule.Session.DashCountOnLatestRespawn;
            }
        }
    }
}
