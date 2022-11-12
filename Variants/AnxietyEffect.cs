using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;

namespace ExtendedVariants.Variants {
    public class AnxietyEffect : AbstractExtendedVariant {
        private bool anxietyCustomized = false;

        public override Type GetVariantType() {
            return typeof(float);
        }

        public override object GetDefaultVariantValue() {
            return -1f;
        }

        public override object GetVariantValue() {
            return Settings.AnxietyEffect;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.AnxietyEffect = (float) value;
        }

        public override void SetLegacyVariantValue(int value) {
            if (value == -1) {
                Settings.AnxietyEffect = -1f;
            } else {
                Settings.AnxietyEffect = (value / 5f);
            }
        }

        public override void Load() {
            On.Celeste.Level.Update += onLevelUpdate;
            IL.Celeste.Distort.Render += modDistortRender;
        }

        public override void Unload() {
            On.Celeste.Level.Update -= onLevelUpdate;
            IL.Celeste.Distort.Render -= modDistortRender;

            // restore the anxiety to its default value
            anxietyCustomized = false;
            Distort.Anxiety = Distort.Anxiety;
            Distort.AnxietyOrigin = Distort.AnxietyOrigin;
        }

        private void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            orig(self);

            if (Settings.AnxietyEffect != -1) {
                anxietyCustomized = true;

                // set the anxiety intensity
                GFX.FxDistort.Parameters["anxiety"].SetValue(Celeste.Settings.Instance.DisableFlashes ? 0f : Settings.AnxietyEffect);

                Vector2 camera = self.Camera.Position;
                Player player = self.Tracker.GetEntity<Player>();
                if (player != null) {
                    // the anxiety comes from the player
                    GFX.FxDistort.Parameters["anxietyOrigin"].SetValue(new Vector2((player.Center.X - camera.X) / 320f, (player.Center.Y - camera.Y) / 180f));
                } else {
                    // there is no player; the anxiety come from the screen center
                    GFX.FxDistort.Parameters["anxietyOrigin"].SetValue(new Vector2(0.5f, 0.5f));
                }
            } else if (anxietyCustomized) {
                // restore the anxiety to its default value
                anxietyCustomized = false;
                Distort.Anxiety = Distort.Anxiety;
                Distort.AnxietyOrigin = Distort.AnxietyOrigin;
            }
        }

        private void modDistortRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // make the vanilla Distort.Render method aware of manually modded anxiety
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld(typeof(Distort), "anxiety"))) {
                Logger.Log("ExtendedVariantMode/AnxietyEffect", $"Modding anxiety value at {cursor.Index} in IL for Distort.Render");
                cursor.EmitDelegate<Func<float, float>>(transformAnxietyValue);
            }
        }

        private float transformAnxietyValue(float originalValue) {
            if (Settings.AnxietyEffect != -1) {
                // anxiety is modded
                return Settings.AnxietyEffect;
            }
            // anxiety is vanilla
            return originalValue;
        }
    }
}
