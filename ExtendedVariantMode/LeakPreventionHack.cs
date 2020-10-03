using Celeste.Mod;
using Monocle;
using MonoMod.Utils;
using NLua;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExtendedVariants {
    // a leak prevention patch that's way too hacky to have its place in Everest.
    // instead of fixing the root issue, it tries to mitigate it by cutting references to the level in all things leaking in the NLua reference maps.
    // but hey, it can make the collab benefit at least.
    internal static class LeakPreventionHack {
        private static Dictionary<object, int> nluaReferenceMap;
        static LeakPreventionHack() {
            if (Everest.LuaLoader.Context != null) {
                // break NLua open and get its reference map. it stays the same, so we only have to do that once.
                nluaReferenceMap = new DynData<ObjectTranslator>(new DynData<Lua>(Everest.LuaLoader.Context).Get<ObjectTranslator>("_translator"))
                    .Get<Dictionary<object, int>>("_objectsBackMap");
            }
        }

        public static void Load() {
            On.Monocle.Entity.Removed += onEntityRemoved;
            On.Monocle.Entity.DissociateFromScene += onEntityDissociatedFromScene;
        }

        public static void Unload() {
            On.Monocle.Entity.Removed -= onEntityRemoved;
            On.Monocle.Entity.DissociateFromScene -= onEntityDissociatedFromScene;
        }

        private static void onEntityRemoved(On.Monocle.Entity.orig_Removed orig, Entity self, Scene scene) {
            orig(self, scene);
            clearUpReferencesToLevel(self);
        }

        private static void onEntityDissociatedFromScene(On.Monocle.Entity.orig_DissociateFromScene orig, Entity self) {
            orig(self);
            clearUpReferencesToLevel(self);
        }

        private static void clearUpReferencesToLevel(Entity self) {
            if (nluaReferenceMap?.ContainsKey(self) ?? false) {
                Type type = self.GetType();
                Logger.Log("ExtendedVariantMode/LeakPreventionHack", $"=== Entity of type {type} is getting checked");
                while (type != typeof(object)) {
                    Logger.Log("ExtendedVariantMode/LeakPreventionHack", $"Check of type {type}");

                    // look for a public Level field, or a private level field, and set it to null.
                    FieldInfo level = type.GetField("level", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (level != null) {
                        Logger.Log(LogLevel.Debug, "ExtendedVariantMode/LeakPreventionHack", $"=> Field level in type {type} is getting set to null");
                        level.SetValue(self, null);
                    }
                    level = type.GetField("Level", BindingFlags.Instance | BindingFlags.Public);
                    if (level != null) {
                        Logger.Log(LogLevel.Debug, "ExtendedVariantMode/LeakPreventionHack", $"=> Field Level in type {type} is getting set to null");
                        level.SetValue(self, null);
                    }

                    type = type.BaseType;
                }
            }
        }
    }
}
