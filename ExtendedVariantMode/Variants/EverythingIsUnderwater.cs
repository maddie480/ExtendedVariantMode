using Celeste;
using ExtendedVariants.Entities;
using Monocle;
using System.Collections;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Celeste.Mod;

namespace ExtendedVariants.Variants {
    class EverythingIsUnderwater : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.EverythingIsUnderwater ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.EverythingIsUnderwater = (value != 0);
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
            On.Celeste.Level.TransitionRoutine += onTransitionRoutine;
            IL.Celeste.Player.NormalUpdate += addNullChecksToWaterTopSurface;
            IL.Celeste.WaterFall.Update += addNullChecksToWaterTopSurface;

            // if already in a map, add the underwater switch controller right away.
            if (Engine.Scene is Level level) {
                level.Add(new UnderwaterSwitchController(Settings));
                level.Entities.UpdateLists();
            }
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
            On.Celeste.Level.TransitionRoutine -= onTransitionRoutine;
            IL.Celeste.Player.NormalUpdate -= addNullChecksToWaterTopSurface;
            IL.Celeste.WaterFall.Update -= addNullChecksToWaterTopSurface;
        }

        private void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (!self.Session?.LevelData?.Underwater ?? false) {
                // inject a controller that will spawn/despawn water depending on the extended variant setting.
                self.Add(new UnderwaterSwitchController(Settings));

                // when transitioning, don't update lists. this messes with sandwich lava, and the hook below will take care of updating lists.
                if (playerIntro != Player.IntroTypes.Transition) {
                    self.Entities.UpdateLists();
                }
            }
        }

        private IEnumerator onTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            IEnumerator vanillaRoutine = orig(self, next, direction);

            // execute the beginning of the routine, *then* update lists.
            // sandwich lava doesn't like it otherwise.
            if (vanillaRoutine.MoveNext()) {
                self.Entities.UpdateLists();
                yield return vanillaRoutine.Current;
            }

            // go on with the rest of the vanilla routine.
            while (vanillaRoutine.MoveNext()) {
                yield return vanillaRoutine.Current;
            }
        }

        private void addNullChecksToWaterTopSurface(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.OpCode == OpCodes.Callvirt && (instr.Operand as MethodReference).Name == "DoRipple")) {
                Logger.Log("ExtendedVariantMode/EverythingIsUnderwater", $"Patching in null check at {cursor.Index} in IL for {cursor.Method.FullName}");

                cursor.Remove();
                cursor.EmitDelegate<Action<Water.Surface, Vector2, float>>((surface, position, multiplier) => {
                    // do the same as vanilla code, but with an added null check.
                    surface?.DoRipple(position, multiplier);
                });
            }
        }
    }
}
