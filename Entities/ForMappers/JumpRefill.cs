using Celeste;
using Celeste.Mod;
using Celeste.Mod.BetterRefillGems;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ExtendedVariants.Entities.ForMappers {
    [CustomEntity(
        "ExtendedVariantMode/JumpRefill = RecoverJumpRefill",
        "ExtendedVariantMode/RecoverJumpRefill = RecoverJumpRefill",
        "ExtendedVariantMode/ExtraJumpRefill = ExtraJumpRefill")]
    public class JumpRefill : Refill {
        public static JumpRefill RecoverJumpRefill(Level level, LevelData levelData, Vector2 offset, EntityData data) => new JumpRefill(data, offset, false);
        public static JumpRefill ExtraJumpRefill(Level level, LevelData levelData, Vector2 offset, EntityData data) => new JumpRefill(data, offset, true);

        private bool extraJumpRefill = false;
        private int extraJumps;
        private JumpCount jumpCountVariant;
        private bool capped;
        private int cap;
        private float respawnTime;
        private bool breakEvenWhenFull;

        public JumpRefill(EntityData data, Vector2 offset, bool extraJumpRefill)
            : base(data, offset) {

            this.extraJumpRefill = extraJumpRefill;
            extraJumps = data.Int("extraJumps", 1);
            capped = data.Bool("capped", false);
            cap = data.Int("cap", -1);
            respawnTime = data.Float("respawnTime", defaultValue: 2.5f);
            breakEvenWhenFull = data.Bool("breakEvenWhenFull", false);
            jumpCountVariant = ExtendedVariantsModule.Instance.VariantHandlers[ExtendedVariantsModule.Variant.JumpCount] as JumpCount;

            string texture = data.Attr("texture", "ExtendedVariantMode/jumprefill");

            // clean up stuff from vanilla we don't want.
            List<Component> toRemove = new List<Component>();
            foreach (Component c in this) {
                if (c.GetType() == typeof(Sprite) || c.GetType() == typeof(Image) || c.GetType() == typeof(PlayerCollider) || c.GetType() == typeof(Wiggler))
                    toRemove.Add(c);
            }
            Remove(toRemove.ToArray());

            // load our own sprites and create a wiggler to attach to them.
            Add(outline = new Image(GFX.Game[$"objects/{texture}/outline"]));
            outline.CenterOrigin();
            outline.Visible = false;

            bool oneUseSprite = data.Bool("oneUse") && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "BetterRefillGems", Version = new Version(1, 0, 1) })
                && betterRefillGemsEnabled() && GFX.Game.Has($"objects/{texture}/oneuse_idle00");

            if (texture == "ExtendedVariantMode/jumprefill") {
                Add(sprite = GFX.SpriteBank.Create("ExtendedVariantMode_JumpRefill_Green"));
                Add(flash = GFX.SpriteBank.Create("ExtendedVariantMode_JumpRefill_Green"));

            } else if (texture == "ExtendedVariantMode/jumprefillblue") {
                Add(sprite = GFX.SpriteBank.Create("ExtendedVariantMode_JumpRefill_Blue"));
                Add(flash = GFX.SpriteBank.Create("ExtendedVariantMode_JumpRefill_Blue"));

            } else {
                // build new sprites from scratch!
                Add(sprite = new Sprite(GFX.Game, $"objects/{texture}/"));
                sprite.AddLoop("idle", "idle", 0.1f);
                if (oneUseSprite) {
                    sprite.AddLoop("oneuse_idle", "oneuse_idle", 0.1f);
                }
                sprite.CenterOrigin();

                Add(flash = new Sprite(GFX.Game, $"objects/{texture}/flash"));
                flash.Add("flash", "", 0.05f);
                flash.CenterOrigin();
            }

            sprite.Play(oneUseSprite ? "oneuse_idle" : "idle");

            flash.OnFinish = delegate {
                flash.Visible = false;
            };

            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v) {
                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
            }));

            // wire the collider to our implementation instead.
            Add(new PlayerCollider(OnPlayerNew));
        }

        private bool betterRefillGemsEnabled() {
            return BetterRefillGemsModule.Settings.Enabled;
        }

        private void OnPlayerNew(Player player) {
            if (refillJumps() || breakEvenWhenFull) {
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;

                // prepare for respawning using vanilla code
                Add(new Coroutine(RefillRoutine(player)));
                respawnTimer = respawnTime;
            }
        }

        private bool refillJumps() {
            if (extraJumpRefill) {
                return jumpCountVariant.AddJumps(extraJumps, capped, cap);
            } else {
                return JumpCount.RefillJumpBuffer();
            }
        }
    }
}
