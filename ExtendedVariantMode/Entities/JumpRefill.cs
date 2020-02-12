using Celeste;
using Celeste.Mod.Entities;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedVariants.Entities {
    [CustomEntity(
        "ExtendedVariantMode/JumpRefill = RecoverJumpRefill",
        "ExtendedVariantMode/RecoverJumpRefill = RecoverJumpRefill",
        "ExtendedVariantMode/ExtraJumpRefill = ExtraJumpRefill")]
    class JumpRefill : Refill {
        private static FieldInfo f_sprite = typeof(Refill).GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo f_flash = typeof(Refill).GetField("flash", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo f_outline = typeof(Refill).GetField("outline", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo f_wiggler = typeof(Refill).GetField("wiggler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo f_respawnTimer = typeof(Refill).GetField("respawnTimer", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo m_RefillRoutine = typeof(Refill).GetMethod("RefillRoutine", BindingFlags.NonPublic | BindingFlags.Instance);

        public static JumpRefill RecoverJumpRefill(Level level, LevelData levelData, Vector2 offset, EntityData data) => new JumpRefill(data, offset, false);
        public static JumpRefill ExtraJumpRefill(Level level, LevelData levelData, Vector2 offset, EntityData data) => new JumpRefill(data, offset, true);

        private bool extraJumpRefill = false;
        private int extraJumps;
        private JumpCount jumpCountVariant;
        private bool capped;

        public JumpRefill(EntityData data, Vector2 offset, bool extraJumpRefill)
            : base(data, offset) {

            this.extraJumpRefill = extraJumpRefill;
            extraJumps = data.Int("extraJumps", 1);
            capped = data.Bool("capped", false);
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
            Sprite sprite;
            Sprite flash;
            Image outline;
            Wiggler wiggler;

            Add(outline = new Image(GFX.Game[$"objects/{texture}/outline"]));
            outline.CenterOrigin();
            outline.Visible = false;

            Add(sprite = new Sprite(GFX.Game, $"objects/{texture}/idle"));
            sprite.AddLoop("idle", "", 0.1f);
            sprite.Play("idle");
            sprite.CenterOrigin();

            Add(flash = new Sprite(GFX.Game, $"objects/{texture}/flash"));
            flash.Add("flash", "", 0.05f);
            flash.OnFinish = delegate {
                flash.Visible = false;
            };
            flash.CenterOrigin();

            Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v) {
                sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
            }));

            f_sprite.SetValue(this, sprite);
            f_outline.SetValue(this, outline);
            f_flash.SetValue(this, flash);
            f_wiggler.SetValue(this, wiggler);

            // wire the collider to our implementation instead.
            Add(new PlayerCollider(OnPlayer));
        }

        private void OnPlayer(Player player) {
            if (refillJumps()) {
                Audio.Play("event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;

                // prepare for respawning using vanilla code
                Add(new Coroutine((IEnumerator) m_RefillRoutine.Invoke(this, new object[] { player })));
                f_respawnTimer.SetValue(this, 2.5f);
            }
        }

        private bool refillJumps() {
            if (extraJumpRefill) {
                return jumpCountVariant.AddJumps(extraJumps, capped);
            } else {
                return jumpCountVariant.RefillJumpBuffer();
            }
        }
    }
}
