using Celeste.Mod;
using Monocle;
using MonoMod.Utils;
using NLua;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExtendedVariants {
    // a leak prevention patch that's way too hacky to have its place in Everest.
    // instead of fixing the root issue, it tries to mitigate it by making NLua forget about any entity that isn't in the scene anymore.
    internal static class LeakPreventionHack {
        private static Dictionary<object, int> nluaReferenceMap;
        private static List<int> discardQueue = new List<int>();

        private static ObjectTranslator nluaObjectTranslator;
        private static MethodInfo nluaCollectObject;

        static LeakPreventionHack() {
            if (Everest.LuaLoader.Context != null) {
                // break NLua open and get its reference map. it stays the same, so we only have to do that once.
                nluaObjectTranslator = new DynData<Lua>(Everest.LuaLoader.Context).Get<ObjectTranslator>("_translator");
                nluaReferenceMap = new DynData<ObjectTranslator>(nluaObjectTranslator).Get<Dictionary<object, int>>("_objectsBackMap");
                nluaCollectObject = typeof(ObjectTranslator).GetMethod("CollectObject", BindingFlags.NonPublic | BindingFlags.Instance, null,
                    CallingConventions.Any, new Type[] { typeof(int) }, null);
            }
        }

        public static void Load() {
            On.Monocle.Entity.Removed += onEntityRemoved;
            On.Celeste.Level.End += onLevelEnd;
        }

        public static void Unload() {
            On.Monocle.Entity.Removed -= onEntityRemoved;
            On.Celeste.Level.End -= onLevelEnd;
        }

        private static void onEntityRemoved(On.Monocle.Entity.orig_Removed orig, Entity self, Scene scene) {
            orig(self, scene);
            queueReferenceToEntity(self);
        }

        private static void queueReferenceToEntity(Entity self) {
            queueReferenceToObject(self);

            // also clear up reference to any of this entity's components.
            // for example, it happens that Player.StateMachine is leaked by NLua as well.
            foreach (Component c in self.Components) {
                queueReferenceToObject(c);
            }
        }

        private static void queueReferenceToObject(object self) {
            if (nluaReferenceMap != null && nluaReferenceMap.TryGetValue(self, out int entityRef)) {
                // add the object ID to the queue.
                Logger.Log("ExtendedVariantMode/LeakPreventionHack", $"Queuing reference of NLua to {self.GetType().FullName} {entityRef} for deletion");
                discardQueue.Add(entityRef);
            }
        }

        private static void onLevelEnd(On.Celeste.Level.orig_End orig, Celeste.Level self) {
            // we're quitting the level, so we need to get rid of all of its entities.
            foreach (Entity entity in self.Entities) {
                queueReferenceToEntity(entity);
            }

            // now we can terminate all queued objects.
            cleanUpReferencesToQueuedObjects();

            orig(self);

            if (nluaReferenceMap != null && nluaReferenceMap.TryGetValue(self, out int levelRef)) {
                // it seems NLua can't dispose entities by itself, so we need to help it a bit.
                Logger.Log("ExtendedVariantMode/LeakPreventionHack", $"Cleaning up reference of NLua to {self.GetType().FullName} {levelRef}");
                nluaCollectObject.Invoke(nluaObjectTranslator, new object[] { levelRef });
            }
        }

        private static void cleanUpReferencesToQueuedObjects() {
            foreach (int reference in discardQueue) {
                // it seems NLua can't dispose entities by itself, so we need to help it a bit.
                Logger.Log("ExtendedVariantMode/LeakPreventionHack", $"Cleaning up reference of NLua to {reference}");
                nluaCollectObject.Invoke(nluaObjectTranslator, new object[] { reference });
            }
            discardQueue.Clear();
        }
    }
}
