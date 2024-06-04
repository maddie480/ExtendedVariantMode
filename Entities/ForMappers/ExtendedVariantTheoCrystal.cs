using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity("ExtendedVariantMode/TheoCrystal")]
    [Tracked(true)]
    public class ExtendedVariantTheoCrystal : TheoCrystal {
        private static ILHook hookTheoCrystalCtor;
        private static string spritePath = null;

        public static void Load() {
            hookTheoCrystalCtor = new ILHook(typeof(TheoCrystal).GetMethod("orig_ctor"), modTheoCrystalCtor);

            IL.Celeste.TheoCrystal.Die += modTheoCrystalDie;
        }

        public static void Unload() {
            hookTheoCrystalCtor?.Dispose();
            hookTheoCrystalCtor = null;

            IL.Celeste.TheoCrystal.Die -= modTheoCrystalDie;
        }

        private static void modTheoCrystalCtor(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("theo_crystal"))) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantTheoCrystal", $"Modding sprite path at {cursor.Index} in IL for TheoCrystal constructor");
                cursor.EmitDelegate<Func<string, string>>(orig => spritePath ?? orig);
            }
        }

        private static void modTheoCrystalDie(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_ForestGreen"))) {
                Logger.Log("ExtendedVariantMode/ExtendedVariantTheoCrystal", $"Modding death effect color at {cursor.Index} in IL for TheoCrystal.Die");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Color, TheoCrystal, Color>>(ModDeathEffectColor);
            }
        }

        private static Color ModDeathEffectColor(Color orig, TheoCrystal self) {
            return self is ExtendedVariantTheoCrystal crystal ? (crystal.deathEffectColor ?? orig) : orig;
        }

        // true if Theo can be left behind, false if the player is blocked if they leave Theo behind, null if it was spawned through the extended variant
        public bool AllowLeavingBehind { get; private set; } = false;
        public bool SpawnedAsEntity { get; private set; } = false;

        private bool forceSpawn = false;

        private Color? deathEffectColor = null;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            spritePath = entityData.Attr("sprite", defaultValue: "theo_crystal");

            ExtendedVariantTheoCrystal crystal;
            if (entityData.Bool("allowThrowingOffscreen")) {
                crystal = new ExtendedVariantTheoCrystalGoingOffscreen(entityData.Position + offset);
            } else {
                crystal = new ExtendedVariantTheoCrystal(entityData.Position + offset);
            }

            spritePath = null;

            crystal.SpawnedAsEntity = true;
            crystal.AllowLeavingBehind = entityData.Bool("allowLeavingBehind");
            crystal.forceSpawn = entityData.Bool("forceSpawn");

            if (entityData.Has("deathEffectColor")) {
                crystal.deathEffectColor = entityData.HexColor("deathEffectColor", defaultValue: Color.ForestGreen);
            }

            return crystal;
        }

        public ExtendedVariantTheoCrystal(Vector2 position) : base(position) {
            RemoveTag(Tags.TransitionUpdate); // I still don't know why vanilla Theo has this, but this causes issues with leaving him behind.
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (SpawnedAsEntity && !forceSpawn) {
                foreach (ExtendedVariantTheoCrystal entity in Scene.Tracker.GetEntities<ExtendedVariantTheoCrystal>()) {
                    if (entity != this && entity.Hold.IsHeld) {
                        RemoveSelf();
                    }
                }
            }
        }

        public override void Update() {
            Level level = SceneAs<Level>();

            // prevent the crystal from going offscreen by the right as well
            // (that's the only specificity of Extended Variant Theo Crystal.)
            if (Right > level.Bounds.Right) {
                Right = level.Bounds.Right;
                Speed.X *= -0.4f;
            }

            base.Update();

            // commit remove self if the variant is disabled mid-screen and we weren't spawned as an entity
            if (!SpawnedAsEntity && !((bool) ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.TheoCrystalsEverywhere))) {
                RemoveSelf();
            }
        }
    }
}
