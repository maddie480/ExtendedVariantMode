using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Linq;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class RisingLavaEverywhere : AbstractExtendedVariant {

        public RisingLavaEverywhere() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;

            IL.Celeste.SandwichLava.Update += modRisingLavaUpdate;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;

            IL.Celeste.SandwichLava.Update -= modRisingLavaUpdate;
        }

        private static void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                addRisingLavaToLevel(self);
            }
        }

        private static IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            yield return new SwapImmediately(orig(self, next, direction));
            addRisingLavaToLevel(self);
        }

        private static void addRisingLavaToLevel(Level level) {
            if (GetVariantValue<bool>(Variant.RisingLavaEverywhere) && level.Entities.All(entity => entity.GetType() != typeof(RisingLava) && entity.GetType() != typeof(SandwichLava))) {
                // we should add a rising lava entity to the level, since there isn't any at the moment.
                Player player = level.Tracker.GetEntity<Player>();
                if (player != null) {
                    // spawn lava if the player is at the bottom of the screen, ice if they are at the top.
                    bool shouldBeIce = (player.Y < level.Bounds.Center.Y);
                    level.Add(new ExtendedVariantSandwichLava(shouldBeIce, player.X - 10f));
                    level.Entities.UpdateLists();
                }
            }
        }

        private static void modRisingLavaUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // the constant for lava speed is 20 (base.Y += 20f * Engine.DeltaTime) => multiply that with our multiplier
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(20f))) {
                Logger.Log("ExtendedVariantMode/RisingLavaEverywhere", $"Applying factor to lava speed at {cursor.Index} in IL code for SandwichLava.Update");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<SandwichLava, float>>(getRisingLavaSpeedFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private static float getRisingLavaSpeedFactor(SandwichLava self) {
            // if we are dealing with modded SandwichLava, make it stop if the player just respawned.
            if (self.GetType() == typeof(ExtendedVariantSandwichLava)) {
                Player player = self.SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null && player.JustRespawned) return 0;
            }

            // otherwise (or if the player did not just respawn), just return the factor.
            return GetVariantValue<float>(Variant.RisingLavaSpeed);
        }
    }
}
