using System;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace ExtendedVariants.Variants {
    class AllStrawberriesAreGoldens : AbstractExtendedVariant {

        private static FieldInfo wiggler = typeof(Strawberry).GetField("wiggler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo rotateWiggler = typeof(Strawberry).GetField("rotateWiggler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo bloom = typeof(Strawberry).GetField("bloom", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo onAnimate = typeof(Strawberry).GetMethod("OnAnimate", BindingFlags.NonPublic | BindingFlags.Instance);

        private ILHook collectRoutineHook;
        private ILHook strawberryUpdateHook;

        public override int GetDefaultValue() {
            return 0;
        }

        public override int GetValue() {
            return Settings.AllStrawberriesAreGoldens ? 1 : 0;
        }

        public override void SetValue(int value) {
            Settings.AllStrawberriesAreGoldens = (value != 0);
        }

        public override void Load() {
            On.Celeste.Player.Die += onPlayerDie;
            IL.Celeste.Strawberry.Added += patchAllGoldenFlags;
            IL.Celeste.Strawberry.OnAnimate += patchAllGoldenFlags;

            collectRoutineHook = ExtendedVariantsModule.HookCoroutine("Celeste.Strawberry", "CollectRoutine", patchAllGoldenFlags);
            strawberryUpdateHook = new ILHook(typeof(Strawberry).GetMethod("orig_Update"), onStrawberryUpdate);
        }

        public override void Unload() {
            On.Celeste.Player.Die -= onPlayerDie;
            IL.Celeste.Strawberry.Added -= patchAllGoldenFlags;
            IL.Celeste.Strawberry.OnAnimate -= patchAllGoldenFlags;

            if (collectRoutineHook != null) collectRoutineHook.Dispose();
            if (strawberryUpdateHook != null) strawberryUpdateHook.Dispose();
        }

        private PlayerDeadBody onPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
            if(!Settings.AllStrawberriesAreGoldens) {
                return orig(self, direction, evenIfInvincible, registerDeathInStats);
            }

            // get the first following strawberry before it is detached by the orig method
            Strawberry firstStrawberry = null;
            foreach (Follower follower in self.Leader.Followers) {
                if (follower.Entity is Strawberry) {
                    firstStrawberry = (follower.Entity as Strawberry);
                    break;
                }
            }

            Level level = self.SceneAs<Level>();

            // call the orig method
            PlayerDeadBody deadBody = orig(self, direction, evenIfInvincible, registerDeathInStats);

            if(deadBody != null && !deadBody.HasGolden && firstStrawberry != null) {
                // the player is dead, doesn't have the actual golden but has a strawberry.
                // we have to do magic to make the game believe they had the golden.
                // (actually, we just do what vanilla does, but with a regular berry instead.)
                deadBody.HasGolden = true;
                deadBody.DeathAction = () => {
                    Engine.Scene = new LevelExit(LevelExit.Mode.GoldenBerryRestart, level.Session) {
                        GoldenStrawberryEntryLevel = firstStrawberry.ID.Level
                    };
                };
            }

            // proceed
            return deadBody;
        }

        private void onStrawberryUpdate(ILContext il) {
            patchAllGoldenFlags(il);

            ILCursor cursor = new ILCursor(il);

            // jump to the first Winged usage: we want to insert ourselves just above it.
            cursor.Index = 0;
            if (cursor.TryGotoNext(MoveType.Before, 
                instr => instr.MatchLdarg(0),
                instr => instr.MatchCallvirt<Strawberry>("get_Winged"))) {
                // inject our method to potentially change the sprite from/to a golden one
                Logger.Log("ExtendedVariantMode/AllStrawberriesAreGoldens", $"Adding delegate call updating strawberry sprite at {cursor.Index} in IL code for Strawberry.Update");

                FieldInfo sprite = typeof(Strawberry).GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);

                // we painstakingly build "sprite = updateStrawberrySprite(this, this.sprite);"
                cursor.Emit(OpCodes.Ldarg_0); // for stfld
                cursor.Emit(OpCodes.Ldarg_0); // for the first updateStrawberrySprite argument
                cursor.Emit(OpCodes.Ldarg_0); // for this.sprite
                cursor.Emit(OpCodes.Ldfld, sprite);
                cursor.EmitDelegate<Func<Strawberry, Sprite, Sprite>>(updateStrawberrySprite);
                cursor.Emit(OpCodes.Stfld, sprite);
            }
        }

        private void patchAllGoldenFlags(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchCallvirt<Strawberry>("get_Golden")))  {
                // replace the Golden flag with a method call (that returns Golden || AllStrawberriesAreGoldens)
                Logger.Log("ExtendedVariantMode/AllStrawberriesAreGoldens", $"Patching golden strawberry check at {cursor.Index} in IL code for Strawberry.{cursor.Method.Name}");

                cursor.Remove();
                cursor.EmitDelegate<Func<Strawberry, bool>>(strawberryHasGoldenCollectBehavior);
            }
        }

        private Sprite updateStrawberrySprite(Strawberry self, Sprite currentSprite) {
            bool isGolden = currentSprite.Texture?.AtlasPath?.Contains("gold") ?? false;
            // in vanilla, if a berry happens to be a moon golden strawberry, it will appear as a moon berry.
            bool shouldBeGolden = !self.Moon && (self.Golden || Settings.AllStrawberriesAreGoldens);

            if(isGolden == shouldBeGolden) {
                // there's nothing to do though...
                return currentSprite;
            } else {
                // which sprite is it supposed to be?
                string spriteName;
                bool isGhostBerry = SaveData.Instance.CheckStrawberry(self.ID);
                if (isGhostBerry) {
                    if (self.Moon) spriteName = "moonghostberry";
                    else if (shouldBeGolden) spriteName = "goldghostberry";
                    else spriteName = "ghostberry";
                }
                else if (self.Moon) spriteName = "moonberry";
                else if (shouldBeGolden) spriteName = "goldberry";
                else spriteName = "strawberry";

                // let's do some sprite swapping dirty work

                // first, let's remove the components we will replace
                self.Remove(currentSprite);
                self.Remove((Wiggler)wiggler.GetValue(self));
                self.Remove((Wiggler)rotateWiggler.GetValue(self));

                // create the new components
                Sprite newSprite = GFX.SpriteBank.Create(spriteName);
                if (self.Winged) {
                    newSprite.Play("flap");
                }
                if(isGhostBerry) {
                    newSprite.Color = Color.White * 0.8f;
                }
                newSprite.OnFrameChange = id => onAnimate.Invoke(self, new object[] { id });
                Wiggler newWiggler = Wiggler.Create(0.4f, 4f, v => {
                    newSprite.Scale = Vector2.One * (1f + v * 0.35f);
                });
                Wiggler newRotateWiggler = Wiggler.Create(0.5f, 4f, v => {
                    newSprite.Rotation = v * 30f * ((float)Math.PI / 180f);
                });

                // fix the bloom (if switched from a strawberry to a golden for example)
                (bloom.GetValue(self) as BloomPoint).Alpha = (shouldBeGolden || self.Moon || isGhostBerry) ? 0.5f : 1f;

                // add the new components and inject them into the strawberry
                self.Add(newSprite);
                self.Add(newWiggler);
                self.Add(newRotateWiggler);
                wiggler.SetValue(self, newWiggler);
                rotateWiggler.SetValue(self, newRotateWiggler);
                return newSprite;
            }
        }

        private bool strawberryHasGoldenCollectBehavior(Strawberry berry) {
            return berry.Golden || Settings.AllStrawberriesAreGoldens;
        }
    }
}
