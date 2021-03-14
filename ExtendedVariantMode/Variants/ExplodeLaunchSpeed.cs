using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExtendedVariants.Variants {
    class ExplodeLaunchSpeed : AbstractExtendedVariant {
        // a surely-non-exhaustive list of all methods from helpers calling ExplodeLaunch. :theoreticalwoke:
        private static readonly Dictionary<string, List<string>> modExplodeLaunchDictionary = new Dictionary<string, List<string>> {
            { "Anonhelper", new List<string> { "Celeste.Mod.Anonhelper.FeatherBumper.OnPlayer" } },
            { "CavernHelper", new List<string> { "Celeste.Mod.CavernHelper.CrystalBomb.Explode" } },
            { "CherryHelper", new List<string> { "Celeste.Mod.CherryHelper.ShadowBumper.OnPlayer" } },
            { "CrystallineHelper", new List<string> { "vitmod.BoostBumper.PlayerBoosted" } },
            { "FactoryHelper", new List<string> { "FactoryHelper.Entities.BoomBox.Explode" } },
            { "FrostHelper", new List<string> { "FrostHelper.StaticBumper.OnPlayer" } },
            { "VivHelper", new List<string> { "VivHelper.Entities.CrystalBombDetonationNeutralizer.Explode", "VivHelper.Entities.CustomSeeker.RegenerateCoroutine" } },
            { "VortexHelper", new List<string> { "Celeste.Mod.VortexHelper.Entities.BowlPuffer.DoEntityCustomInteraction", "Celeste.Mod.VortexHelper.Entities.VortexBumper.OnPlayer" } },
        };


        private static FieldInfo playerExplodeLaunchBoostTimer = typeof(Player).GetField("explodeLaunchBoostTimer", BindingFlags.NonPublic | BindingFlags.Instance);

        private bool initialized = false;
        private List<ILHook> appliedILHooks = new List<ILHook>();

        public override int GetDefaultValue() {
            return 10;
        }

        public override int GetValue() {
            return Settings.ExplodeLaunchSpeed;
        }

        public override void SetValue(int value) {
            Settings.ExplodeLaunchSpeed = value;
        }

        public override void Load() {
            // we cannot hook ExplodeLaunch because of https://github.com/EverestAPI/Everest/issues/66
            // so we'll wrap each call of it instead.
            IL.Celeste.Bumper.OnPlayer += wrapExplodeLaunchCall;
            IL.Celeste.Puffer.Explode += wrapExplodeLaunchCall;
            IL.Celeste.TempleBigEyeball.OnPlayer += wrapExplodeLaunchCall;

            appliedILHooks.Add(new ILHook(typeof(Seeker).GetMethod("RegenerateCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), wrapExplodeLaunchCall));

            if (initialized) {
                // Initialize() was already called, so don't wait for it to hook mods.
                hookModsUsingExplodeLaunch();
            }
        }

        public void Initialize() {
            if (Settings.MasterSwitch) {
                // hook mods now that they are all loaded!
                hookModsUsingExplodeLaunch();
            }
            initialized = true;
        }

        private void hookModsUsingExplodeLaunch() {
            // go through all mods that are known to use ExplodeLaunch.
            foreach (KeyValuePair<string, List<string>> mod in modExplodeLaunchDictionary) {
                EverestModule module = Everest.Modules.FirstOrDefault(m => m.Metadata.Name == mod.Key);
                if (module != null) {
                    // the mod is installed! go through all methods.
                    foreach (string method in mod.Value) {
                        // split "FrostHelper.StaticBumper.OnPlayer" in "FrostHelper.StaticBumper" and "OnPlayer"
                        string className = method.Substring(0, method.LastIndexOf("."));
                        string methodName = method.Substring(method.LastIndexOf(".") + 1);

                        // look for the method
                        MethodInfo methodInfo = module.GetType().Assembly.GetType(className)?.GetMethod(methodName);
                        if (methodInfo == null) {
                            // try again but with a private method this time
                            methodInfo = module.GetType().Assembly.GetType(className)?.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                        }

                        if (methodInfo == null) {
                            // method not found! maybe it disappeared in an update. this isn't a fatal error, just log it.
                            Logger.Log("ExtendedVariantMode/ExplodeLaunchSpeed", $"{mod.Key} is installed but method to hook was not found in it: {method}");
                        } else if (methodName == "RegenerateCoroutine") {
                            // this is the only one in the bunch that is a coroutine, so don't forget to GetStateMachineTarget
                            appliedILHooks.Add(new ILHook(methodInfo.GetStateMachineTarget(), wrapExplodeLaunchCall));
                        } else {
                            // IL hook the method
                            appliedILHooks.Add(new ILHook(methodInfo, wrapExplodeLaunchCall));
                        }
                    }
                }
            }
        }

        public override void Unload() {
            IL.Celeste.Bumper.OnPlayer -= wrapExplodeLaunchCall;
            IL.Celeste.Puffer.Explode -= wrapExplodeLaunchCall;
            IL.Celeste.TempleBigEyeball.OnPlayer -= wrapExplodeLaunchCall;

            foreach (ILHook hook in appliedILHooks) {
                hook.Dispose();
            }
            appliedILHooks.Clear();
        }

        private void wrapExplodeLaunchCall(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("ExplodeLaunch"))) {
                Logger.Log("ExtendedVariantMode/ExplodeLaunchSpeed", $"Adding call after ExplodeLaunch at {cursor.Index} in IL code for {il.Method.DeclaringType.Name}.{il.Method.Name}");
                cursor.EmitDelegate<Action>(correctExplodeLaunchSpeed);
            }
        }

        private void correctExplodeLaunchSpeed() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                player.Speed *= (Settings.ExplodeLaunchSpeed / 10f);

                if (Settings.DisableSuperBoosts) {
                    if (Input.MoveX.Value == Math.Sign(player.Speed.X)) {
                        // cancel super boost
                        player.Speed.X /= 1.2f;
                    } else {
                        // cancel super boost leniency on the Celeste beta (this field does not exist on stable)
                        playerExplodeLaunchBoostTimer?.SetValue(player, 0f);
                    }
                }
            }
        }
    }
}
