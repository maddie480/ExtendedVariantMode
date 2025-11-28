using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities.ForMappers;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class TheoCrystalsEverywhere : AbstractExtendedVariant {
        public TheoCrystalsEverywhere() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.EnforceBounds += modEnforceBounds;
            IL.Celeste.Level.EnforceBounds += excludeExtendedVariantTheoCrystalFromEnforceBounds;
            IL.Celeste.CassetteBlock.BlockedCheck += excludeExtendedVariantTheoCrystalFromCassetteBlockBlockedCheck;
            IL.Celeste.TheoCrystal.Added += excludeExtendedVariantTheoCrystalFromAdded;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.EnforceBounds -= modEnforceBounds;
            IL.Celeste.Level.EnforceBounds -= excludeExtendedVariantTheoCrystalFromEnforceBounds;
            IL.Celeste.CassetteBlock.BlockedCheck -= excludeExtendedVariantTheoCrystalFromCassetteBlockBlockedCheck;
            IL.Celeste.TheoCrystal.Added -= excludeExtendedVariantTheoCrystalFromAdded;
        }

        private static void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
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

        private static void injectTheoCrystal(Level level) {
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

        private static void injectTheoCrystalAfterTransition(Level level) {
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

        private static void modEnforceBounds(On.Celeste.Level.orig_EnforceBounds orig, Level self, Player player) {
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

        private static void excludeExtendedVariantTheoCrystalFromEnforceBounds(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Scene>("get_Tracker"))) {
                cursor.Index++;
                Logger.Log("ExtendedVariantMode/TheoCrystalsEverywhere", $"Excluding Extended Variant Theo Crystals at {cursor.Index} from Level.EnforceBounds");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<TheoCrystal, Level, TheoCrystal>>(searchNonExtendedTheoCrystal);
            }
        }
        private static TheoCrystal searchNonExtendedTheoCrystal(TheoCrystal orig, Level self) {
            if (orig is not ExtendedVariantTheoCrystal) return orig;
            return findFirstNonExtendedTheoCrystal(self.Tracker.GetEntities<TheoCrystal>());
        }

        private static void excludeExtendedVariantTheoCrystalFromAdded(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Scene>("get_Tracker"))) {
                cursor.Index++;
                Logger.Log("ExtendedVariantMode/TheoCrystalsEverywhere", $"Excluding Extended Variant Theo Crystals at {cursor.Index} from TheoCrystal.Added");

                cursor.EmitDelegate<Func<List<Entity>, List<Entity>>>(filterOutExtendedCrystals);
            }
        }
        private static List<Entity> filterOutExtendedCrystals(List<Entity> orig) {
            return orig.Where(entity => entity is not ExtendedVariantTheoCrystal).ToList();
        }

        private static void excludeExtendedVariantTheoCrystalFromCassetteBlockBlockedCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Call && ((MethodReference) instr.Operand).Name == "CollideFirst")) {
                Logger.Log("ExtendedVariantMode/TheoCrystalsEverywhere", $"Excluding Extended Variant Theo Crystals at {cursor.Index} from CassetteBlock.BlockedCheck");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<TheoCrystal, CassetteBlock, TheoCrystal>>(searchFirstNonExtendedTheoCrystalToCollideWith);
            }
        }
        private static TheoCrystal searchFirstNonExtendedTheoCrystalToCollideWith(TheoCrystal orig, CassetteBlock self) {
            if (orig is not ExtendedVariantTheoCrystal) return orig;
            return findFirstNonExtendedTheoCrystal(self.CollideAll<TheoCrystal>());
        }

        private static TheoCrystal findFirstNonExtendedTheoCrystal(List<Entity> theoCrystals) {
            return theoCrystals
                .Where(theoCrystal => theoCrystal is not ExtendedVariantTheoCrystal)
                .OfType<TheoCrystal>()
                .FirstOrDefault();
        }

        private static bool isTheoCrystalsEverywhere() {
            // the variant is on, or an Extended Variant Theo crystal is in the level.
            return GetVariantValue<bool>(Variant.TheoCrystalsEverywhere) ||
                Engine.Scene.Tracker.GetEntities<ExtendedVariantTheoCrystal>().OfType<ExtendedVariantTheoCrystal>().Any(t => t.SpawnedAsEntity);
        }

        private static bool isAllowLeavingTheoBehind() {
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
    }
}
