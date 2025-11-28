using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class RestoreDashesOnRespawn : AbstractExtendedVariant {
        private static List<ILHook> ilHooks = new List<ILHook>();

        public RestoreDashesOnRespawn() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            ilHooks.Add(new ILHook(typeof(Cassette).GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), updateDashCountOnRespawnPointChange));
            ilHooks.Add(new ILHook(typeof(Level).GetMethod("orig_TransitionRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), updateDashCountOnRespawnPointChange));

            IL.Celeste.SummitCheckpoint.Awake += updateDashCountOnRespawnPointChange;
            IL.Celeste.SummitCheckpoint.Update += updateDashCountOnRespawnPointChange;
            IL.Celeste.ChangeRespawnTrigger.OnEnter += updateDashCountOnRespawnPointChange;
            IL.Celeste.Level.TeleportTo += updateDashCountOnRespawnPointChange;

            On.Celeste.Player.Added += onPlayerSpawn;
        }

        public override void Unload() {
            foreach (ILHook hook in ilHooks) {
                hook.Dispose();
            }
            ilHooks.Clear();

            IL.Celeste.SummitCheckpoint.Awake -= updateDashCountOnRespawnPointChange;
            IL.Celeste.SummitCheckpoint.Update -= updateDashCountOnRespawnPointChange;
            IL.Celeste.ChangeRespawnTrigger.OnEnter -= updateDashCountOnRespawnPointChange;
            IL.Celeste.Level.TeleportTo -= updateDashCountOnRespawnPointChange;

            On.Celeste.Player.Added -= onPlayerSpawn;
        }

        // save dash count when hitting a change respawn trigger
        private static void updateDashCountOnRespawnPointChange(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to the point(s) where the respawn point is changed
            while (cursor.TryGotoNext(instr => instr.MatchStfld<Session>("RespawnPoint"))) {
                Logger.Log("ExtendedVariantMode/RestoreDashesOnRespawn", $"Adding save for dash count on respawn change at {cursor.Index} in CIL code for " + il.Method.FullName);
                cursor.EmitDelegate<Action>(saveDashCountOnLastRespawn);

                cursor.Index++;
            }
        }
        private static void saveDashCountOnLastRespawn() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                ExtendedVariantsModule.Session.DashCountOnLatestRespawn = player.Dashes;
            }
        }

        private static void onPlayerSpawn(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig(self, scene);

            if (GetVariantValue<bool>(Variant.RestoreDashesOnRespawn) && ExtendedVariantsModule.Session.DashCountOnLatestRespawn != -1) {
                self.Dashes = ExtendedVariantsModule.Session.DashCountOnLatestRespawn;
            } else if (ExtendedVariantsModule.Session.DashCountOnLatestRespawn == -1) {
                // this is the first time we spawn in the level! we should save the dash count.
                ExtendedVariantsModule.Session.DashCountOnLatestRespawn = self.Dashes;
            }
        }
    }
}
