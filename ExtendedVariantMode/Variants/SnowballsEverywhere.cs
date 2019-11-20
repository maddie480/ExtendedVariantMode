using Celeste;
using Celeste.Mod;
using ExtendedVariants.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;

namespace ExtendedVariants.Variants {
    public class SnowballsEverywhere : AbstractExtendedVariant {
        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.SnowballsEverywhere ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.SnowballsEverywhere = (value != 0);
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;
            IL.Celeste.Snowball.Update += modSnowballUpdate;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
            IL.Celeste.Snowball.Update -= modSnowballUpdate;
        }

        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                addSnowballToLevel(self);
            }
        }
        
        private IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            // just make sure the whole transition routine is over
            IEnumerator origEnum = orig(self, next, direction);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

            addSnowballToLevel(self);

            yield break;
        }
        
        private void addSnowballToLevel(Level level) {
            // do pretty much the same thing as the so-called "Wind Attack Trigger" from vanilla
            if (Settings.SnowballsEverywhere && level.Entities.FindFirst<Snowball>() == null) {
                Snowball snowball = new AutoDestroyingSnowball();
                level.Add(snowball);
                level.Entities.UpdateLists();

                // send the snowball off-screen, so that the player gets 0.8 seconds before the first snowball
                snowball.X = level.Camera.Left - 60f;
            }
        }

        private void modSnowballUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // go everywhere where the 0.8 second delay is defined
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.8f))) {
                Logger.Log("ExtendedVariantMode/SnowballsEverywhere", $"Modding delay between snowballs at {cursor.Index} in CIL code for Update in Snowball");

                // and substitute it with our own value
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate<Func<float>>(determineDelayBetweenSnowballs);
            }
        }

        private float determineDelayBetweenSnowballs() {
            if(ExtendedVariantsModule.ShouldIgnoreCustomDelaySettings()) {
                return 0.8f;
            }
            return Settings.SnowballDelay / 10f;
        }
    }
}
