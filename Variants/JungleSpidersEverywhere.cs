using Celeste;
using Celeste.Mod;
using Celeste.Mod.JungleHelper.Entities;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class JungleSpidersEverywhere : AbstractExtendedVariant {
        public enum SpiderType { Disabled, Blue, Purple, Red }

        private static Hook hookShouldPause;
        private static bool spawnedSpider;

        public JungleSpidersEverywhere() : base(variantType: typeof(SpiderType), defaultVariantValue: SpiderType.Disabled) { }

        public override object ConvertLegacyVariantValue(int value) {
            return (SpiderType) value;
        }

        public override void Load() {
            On.Celeste.Level.LoadLevel += modLoadLevel;
            On.Celeste.Level.TransitionRoutine += modTransitionRoutine;

            hookShouldPause = new Hook(
                typeof(SpiderBoss).GetMethod("shouldPause", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(JungleSpidersEverywhere).GetMethod("shouldPause", BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= modLoadLevel;
            On.Celeste.Level.TransitionRoutine -= modTransitionRoutine;

            hookShouldPause?.Dispose();
            hookShouldPause = null;
        }

        private void modLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                addSpiderToLevel(self);
            }
        }

        private IEnumerator modTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction) {
            yield return new SwapImmediately(orig(self, next, direction));
            addSpiderToLevel(self);
        }

        private void addSpiderToLevel(Level self) {
            spawnedSpider = false;

            // do not do anything if the variant is disabled (obviously)
            if (GetVariantValue<SpiderType>(Variant.JungleSpidersEverywhere) == SpiderType.Disabled) return;

            // do not do anything if the vanilla level already has spider bosses
            if (self.Entities.OfType<SpiderBoss>().Count() > 0) return;

            // spawn a spider!
            EntityData data = new EntityData();
            data.Values = new Dictionary<string, object> {
                { "color", GetVariantValue<SpiderType>(Variant.JungleSpidersEverywhere).ToString() }
            };
            SpiderBoss spider = new SpiderBoss(data, Vector2.Zero);
            self.Add(spider);
            self.Entities.UpdateLists();
            spawnedSpider = true;
        }

        private static bool shouldPause(Func<Entity, bool> orig, Entity self) {
            return orig(self) || (spawnedSpider && ExtendedVariantsModule.ShouldEntitiesAutoDestroy(self.Scene?.Tracker.GetEntity<Player>()));
        }
    }
}
