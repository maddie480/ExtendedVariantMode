using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using ExtendedVariants.Entities.ForMappers;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class TheoCrystalsEverywhere : AbstractExtendedVariant {
        private static ILHook hookTempleGateClose = null;
        private static ILHook hookCrystallineHelperFlagCrystal = null;

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetDefaultVariantValue() {
            return false;
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.EnforceBounds += modEnforceBounds;
            IL.Celeste.TempleGate.TheoIsNearby += addExtendedVariantTheoCrystalToCollideCheck;

            hookTempleGateClose = new ILHook(
                typeof(TempleGate).GetMethod("CloseBehindPlayerAndTheo", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                addExtendedVariantTheoCrystalToCollideCheck);
        }

        public static void Initialize() {
            if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "CrystallineHelper", Version = new Version(1, 15, 0) })) {
                MethodInfo flagCrystalTheoIsNearby = Everest.Modules.FirstOrDefault(m => m.Metadata?.Name == "CrystallineHelper").GetType().Assembly
                    .GetType("vitmod.FlagCrystal").GetMethod("TempleGate_TheoIsNearby");

                hookCrystallineHelperFlagCrystal = new ILHook(flagCrystalTheoIsNearby, addExtendedVariantTheoCrystalToCollideCheck);
            }
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.EnforceBounds -= modEnforceBounds;
            IL.Celeste.TempleGate.TheoIsNearby -= addExtendedVariantTheoCrystalToCollideCheck;

            hookTempleGateClose?.Dispose();
            hookTempleGateClose = null;

            hookCrystallineHelperFlagCrystal?.Dispose();
            hookCrystallineHelperFlagCrystal = null;
        }

        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                injectTheoCrystal(self);

            } else if ((GetVariantValue<bool>(Variant.AllowLeavingTheoBehind) || GetVariantValue<bool>(Variant.AllowThrowingTheoOffscreen)) &&
                !(self.Tracker.GetEntity<Player>()?.Holding?.Entity is TheoCrystal)) {

                // player is transitioning into a new room, but doesn't have Theo with them.
                injectTheoCrystalAfterTransition(self);

            } else if (self.Tracker.CountEntities<ExtendedVariantTheoCrystal>() == 0) {
                // player is transitioning into a new room, and no Theo crystal is in the room: we should add one if the variant is enabled.
                injectTheoCrystalAfterTransition(self);
            }
        }

        private void injectTheoCrystal(Level level) {
            if (GetVariantValue<bool>(Variant.TheoCrystalsEverywhere)) {
                Player player = level.Tracker.GetEntity<Player>();
                bool hasCrystalInBaseLevel = level.Tracker.CountEntities<TheoCrystal>() != 0;

                // check if the base level already has a crystal
                if (player != null && !hasCrystalInBaseLevel) {
                    // add a Theo Crystal where the player is
                    level.Add(GetVariantValue<bool>(Variant.AllowThrowingTheoOffscreen) ? new ExtendedVariantTheoCrystalGoingOffscreen(player.Position) : new ExtendedVariantTheoCrystal(player.Position));
                    level.Entities.UpdateLists();
                }
            }
        }

        private void injectTheoCrystalAfterTransition(Level level) {
            if (GetVariantValue<bool>(Variant.TheoCrystalsEverywhere)) {
                Player player = level.Tracker.GetEntity<Player>();
                bool hasCrystalInBaseLevel = level.Session.LevelData.Entities.Any(entity => entity.Name == "theoCrystal");

                // check if the base level already has a crystal
                if (player != null && !hasCrystalInBaseLevel) {
                    // add a Theo Crystal where the spawn point nearest to player is
                    Vector2 spawn = level.GetSpawnPoint(player.Position);
                    level.Add(GetVariantValue<bool>(Variant.AllowThrowingTheoOffscreen) ? new ExtendedVariantTheoCrystalGoingOffscreen(spawn) : new ExtendedVariantTheoCrystal(spawn));
                    level.Entities.UpdateLists();
                }
            }
        }

        private void modEnforceBounds(On.Celeste.Level.orig_EnforceBounds orig, Level self, Player player) {
            if (isTheoCrystalsEverywhere() && !isAllowLeavingTheoBehind() &&
                // the player is holding nothing...
                (player.Holding == null || player.Holding.Entity == null || !player.Holding.IsHeld ||
                // or this thing is not a Theo crystal (f.e. jellyfish)
                (player.Holding.Entity.GetType() != typeof(TheoCrystal) && player.Holding.Entity.GetType() != typeof(ExtendedVariantTheoCrystal) && player.Holding.Entity.GetType() != typeof(ExtendedVariantTheoCrystalGoingOffscreen))) &&
                // but there is a Theo crystal on screen so the player has to grab it
                self.Tracker.CountEntities<ExtendedVariantTheoCrystal>() != 0) {

                // the player does not hold Theo, check if they try to leave the screen without him
                Rectangle bounds = self.Bounds;
                if (player.Left < bounds.Left) {
                    // prevent the player from going left
                    player.Left = bounds.Left;
                    player.OnBoundsH();
                } else if (player.Right > bounds.Right) {
                    // prevent the player from going right
                    player.Right = bounds.Right;
                    player.OnBoundsH();
                } else if (player.Top < bounds.Top) {
                    // prevent the player from going up
                    player.Top = bounds.Top;
                    player.OnBoundsV();
                } else if (player.Bottom > bounds.Bottom) {
                    if (SaveData.Instance.Assists.Invincible) {
                        // bounce the player off the bottom of the screen (= falling into a pit with invincibility on)
                        player.Play("event:/game/general/assist_screenbottom");
                        player.Bounce(bounds.Bottom);
                    } else {
                        // kill the player if they try to go down
                        player.Die(Vector2.Zero);
                    }
                } else {
                    // no screen transition detected => proceed to vanilla
                    orig(self, player);
                }
            } else {
                // the variant is disabled, we are allowed to leave Theo behind, or the player is holding Theo => we have no business here.
                orig(self, player);
            }
        }

        private bool isTheoCrystalsEverywhere() {
            // the variant is on, or an Extended Variant Theo crystal is in the level.
            return GetVariantValue<bool>(Variant.TheoCrystalsEverywhere) ||
                Engine.Scene.Tracker.GetEntities<ExtendedVariantTheoCrystal>().OfType<ExtendedVariantTheoCrystal>().Any(t => t.SpawnedAsEntity);
        }

        private bool isAllowLeavingTheoBehind() {
            // get all Theo crystals in the level that were placed in Ahorn.
            IEnumerable<ExtendedVariantTheoCrystal> entities =
                Engine.Scene.Tracker.GetEntities<ExtendedVariantTheoCrystal>().OfType<ExtendedVariantTheoCrystal>().Where(t => t.SpawnedAsEntity);

            if (entities.Count() == 0) {
                // there is none! just use the extended variant setting.
                return GetVariantValue<bool>(Variant.AllowLeavingTheoBehind);
            } else {
                // allow leaving Theo behind of all Theos on the screen can be left behind.
                return entities.All(t => t.AllowLeavingBehind);
            }
        }

        private static void addExtendedVariantTheoCrystalToCollideCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Callvirt && (instr.Operand as MethodReference).FullName == "T Monocle.Tracker::GetEntity<Celeste.TheoCrystal>()")) {
                Logger.Log("ExtendedVariantMode/TheoCrystalsEverywhere", $"Patching Theo collide check to include extended variant Theo crystals at {cursor.Index} in IL for {il.Method.FullName}");

                cursor.EmitDelegate<Func<TheoCrystal, TheoCrystal>>(orig => {
                    if (orig != null) {
                        return orig;
                    }

                    return Engine.Scene.Tracker.GetEntity<ExtendedVariantTheoCrystal>();
                });
            }
        }
    }
}
