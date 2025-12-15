using Celeste;
using Celeste.Mod;
using Celeste.Mod.DJMapHelper.Entities;
using ExtendedVariants.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Linq;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class OshiroEverywhere : AbstractExtendedVariant {

        public OshiroEverywhere() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;
            IL.Celeste.AngryOshiro.Update += modAngryOshiroUpdate;
            On.Celeste.HeartGem.Collect += modHeartGemCollect;
            On.Celeste.Player.Update += onPlayerUpdate;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;
            IL.Celeste.AngryOshiro.Update -= modAngryOshiroUpdate;
            On.Celeste.HeartGem.Collect -= modHeartGemCollect;
            On.Celeste.Player.Update -= onPlayerUpdate;
        }

        private static bool wasActiveOnLastFrame = false;

        private static void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                addOshiroToLevel(self);
            }

            wasActiveOnLastFrame = GetVariantValue<bool>(Variant.OshiroEverywhere);
        }

        private static IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            yield return new SwapImmediately(orig(self, next, direction));
            addOshiroToLevel(self);
        }

        private static void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            orig(self);

            if (!wasActiveOnLastFrame && GetVariantValue<bool>(Variant.OshiroEverywhere)) {
                addOshiroToLevel(Engine.Scene as Level, false);
            }

            wasActiveOnLastFrame = GetVariantValue<bool>(Variant.OshiroEverywhere);
        }

        private static void addOshiroToLevel(Level level, bool updateLists = true) {
            bool oshiroAdded = false;

            if (GetVariantValue<bool>(Variant.OshiroEverywhere)) {
                for (int i = level.Tracker.CountEntities<AngryOshiro>(); i < GetVariantValue<int>(Variant.OshiroCount); i++) {
                    // this replicates the behavior of Oshiro Trigger in vanilla Celeste
                    Vector2 position = new Vector2(level.Bounds.Left - 32, level.Bounds.Top + level.Bounds.Height / 2);
                    level.Add(new AutoDestroyingAngryOshiro(position, false, i * 0.5f));
                    oshiroAdded = true;
                }

                // check if reverse Oshiros have to be added
                if (ExtendedVariantsModule.Instance.DJMapHelperInstalled && GetVariantValue<int>(Variant.ReverseOshiroCount) > 0) {
                    addReverseOshiroToLevel(level);
                    oshiroAdded = true;
                }
            }

            if (oshiroAdded && updateLists)
                level.Entities.UpdateLists();
        }

        private static void addReverseOshiroToLevel(Level level) {
            for (int i = level.Tracker.CountEntities<AngryOshiroRight>(); i < GetVariantValue<int>(Variant.ReverseOshiroCount); i++) {
                // this replicates the behavior of Oshiro Trigger in vanilla Celeste
                Vector2 position = new Vector2(level.Bounds.Right + 32, level.Bounds.Top + level.Bounds.Height / 2);
                AngryOshiroRight oshiro = new AngryOshiroRight(position);
                oshiro.Add(new AutoDestroyingReverseOshiroModder(i * 0.5f));
                level.Add(oshiro);
            }
        }

        private static void modAngryOshiroUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to add ourselves in here: entity != null && !entity.Dead && base.CenterX < entity.CenterX + 4f
            // this is the condition used to apply slowdown
            // we use entity.Dead since this is the only time it is used in the method
            // (ps: I saw there was a StopControllingTime method, but it makes Oshiro stop controlling anxiety as well.)
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("get_Dead"))) {
                // we're pointing at a branch instruction that checks that our thing is false, since this is !entity.Dead. Store it and step past it.
                Instruction branchInstruction = cursor.Next;
                cursor.Index++;

                Logger.Log("ExtendedVariantMode/OshiroEverywhere", $"Adding condition for time control at {cursor.Index} in CIL code for AngryOshiro.Update");

                // inject !isOshiroSlowdownDisabled
                cursor.EmitDelegate<Func<bool>>(isOshiroSlowdownDisabled);
                cursor.Emit(branchInstruction.OpCode, branchInstruction.Operand);
            }
        }

        private static bool isOshiroSlowdownDisabled() {
            return GetVariantValue<bool>(Variant.DisableOshiroSlowdown);
        }

        private static void modHeartGemCollect(On.Celeste.HeartGem.orig_Collect orig, HeartGem self, Player player) {
            // tell all extended variant Oshiros to stop controlling time
            foreach (AutoDestroyingAngryOshiro oshiro in self.Scene.Entities.OfType<AutoDestroyingAngryOshiro>()) {
                oshiro.StopControllingTime();
            }
            // tell all reverse Oshiros spawned by Extended Variants to do the same
            if (ExtendedVariantsModule.Instance.DJMapHelperInstalled) {
                tellReverseOshirosToStopControllingTime(self);
            }

            orig(self, player);
        }

        private static void tellReverseOshirosToStopControllingTime(HeartGem self) {
            foreach (AngryOshiroRight oshiro in self.Scene.Entities.OfType<AngryOshiroRight>()) {
                if (oshiro.Any(component => component.GetType() == typeof(AutoDestroyingReverseOshiroModder))) {
                    oshiro.StopControllingTime();
                }
            }
        }
    }
}
