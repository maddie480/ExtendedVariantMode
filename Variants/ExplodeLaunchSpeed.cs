using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    public class ExplodeLaunchSpeed : AbstractExtendedVariant {
        private static readonly FieldInfo playerExplodeLaunchBoostTimer = typeof(Player).GetField("explodeLaunchBoostTimer", BindingFlags.NonPublic | BindingFlags.Instance);

        public ExplodeLaunchSpeed() : base(variantType: typeof(float), defaultVariantValue: 1f) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value / 10f;
        }

        public override void Load() {
            On.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += correctExplodeLaunchSpeed;
        }

        public override void Unload() {
            On.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= correctExplodeLaunchSpeed;
        }

        private Vector2 correctExplodeLaunchSpeed(On.Celeste.Player.orig_ExplodeLaunch_Vector2_bool_bool orig, Player self, Vector2 from, bool snapUp, bool sidesOnly) {
            Vector2 result = orig(self, from, snapUp, sidesOnly);

            Player player = Engine.Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                player.Speed *= GetVariantValue<float>(Variant.ExplodeLaunchSpeed);

                if (GetVariantValue<bool>(Variant.DisableSuperBoosts)) {
                    if (Input.MoveX.Value == Math.Sign(player.Speed.X)) {
                        // cancel super boost
                        player.Speed.X /= 1.2f;
                    } else {
                        // cancel super boost leniency on the Celeste beta (this field does not exist on stable)
                        playerExplodeLaunchBoostTimer?.SetValue(player, 0f);
                    }
                }
            }

            return result;
        }
    }
}
