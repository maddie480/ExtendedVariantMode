using Celeste;
using Microsoft.Xna.Framework;
using System;

namespace ExtendedVariants.Variants {
    public class BounceEverywhere : AbstractExtendedVariant {
        public override object GetDefaultVariantValue() {
            return false;
        }

        public override Type GetVariantType() {
            return typeof(bool);
        }

        public override object GetVariantValue() {
            return Settings.BounceEverywhere;
        }

        protected override void DoSetVariantValue(object value) {
            Settings.BounceEverywhere = (bool) value;
        }

        public override void SetLegacyVariantValue(int value) {
            Settings.BounceEverywhere = (value != 0);
        }

        public override void Load() {
            On.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
        }

        public override void Unload() {
            On.Celeste.Player.NormalUpdate -= modPlayerNormalUpdate;
        }

        private int modPlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self) {
            int newState = orig(self);

            if (Settings.BounceEverywhere && newState == 0) {
                // we are still in the Normal state.

                // no bounce if the player is going to climb! Climbing should already take priority over bouncing.
                if (!SaveData.Instance.Assists.NoGrabbing && !self.Ducking && self.Stamina >= 20f && Input.GrabCheck && self.ClimbCheck((int) self.Facing)) {
                    return newState;
                }

                Level level = self.SceneAs<Level>();
                Rectangle hitbox = self.Collider.Bounds;

                bool bounce = false;

                // check for collision below
                hitbox.Height++;
                if (level.CollideCheck<Solid>(hitbox) && self.Speed.Y >= 0f) {
                    self.SuperBounce(self.Bottom);
                    bounce = true;
                } else {
                    // check for collision on the right
                    hitbox.Height--;
                    hitbox.Width++;

                    if (level.CollideCheck<Solid>(hitbox) && self.SideBounce(-1, self.Right, self.CenterY)) {
                        bounce = true;
                    } else {
                        // check for collision on the left
                        hitbox.X--;

                        if (level.CollideCheck<Solid>(hitbox) && self.SideBounce(1, self.Left, self.CenterY)) {
                            bounce = true;
                        }
                    }
                }

                if (bounce) {
                    Audio.Play("event:/game/general/spring", self.Center);
                }
            }

            return newState;
        }
    }
}
