using Celeste;
using ExtendedVariants.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace ExtendedVariants.Entities {
	// an simplified copy-paste of RisingLava making it falling ice instead.
	public class ExtendedVariantFallingIce : Entity {

		private bool waiting;

		private float lerp;

		private LavaRect topRect;

		private float delay;

		private SoundSource loopSfx;

		public ExtendedVariantFallingIce() {
			Depth = -1000000;
			Collider = new Hitbox(340f, 120f, 0f, -280f);
			Visible = false;
			Add(new PlayerCollider(OnPlayer));
			Add(loopSfx = new SoundSource());
			Add(topRect = new LavaRect(400f, 200f, 4));
			topRect.Position = new Vector2(-40f, -360f);
			topRect.OnlyMode = LavaRect.OnlyModes.OnlyBottom;
			topRect.SmallWaveAmplitude = 2f;
			topRect.SurfaceColor = Calc.HexToColor("33ffe7");
			topRect.EdgeColor = Calc.HexToColor("4ca2eb");
			topRect.CenterColor = Calc.HexToColor("0151d0");
			topRect.Fade = 128;
		}

		public override void Added(Scene scene) {
			base.Added(scene);
			X = SceneAs<Level>().Bounds.Left - 10;
			Y = SceneAs<Level>().Bounds.Top - 16;
			loopSfx.Play("event:/game/09_core/rising_threat", "room_state", 1);
			loopSfx.Position = new Vector2(Width / 2f, 0f);
		}

		public override void Awake(Scene scene) {
			base.Awake(scene);

			Player entity = Scene.Tracker.GetEntity<Player>();
			if (entity != null && entity.JustRespawned) {
				waiting = true;
			}
		}

		private void OnPlayer(Player player) {
			if (SaveData.Instance.Assists.Invincible) {
				if (delay <= 0f) {
					float from = Y;
					float to = Y - 48f;
					player.Speed.Y = 200f;
					Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, delegate (Tween t) {
						Y = MathHelper.Lerp(from, to, t.Eased);
					});
					delay = 0.5f;
					loopSfx.Param("rising", 0f);
					Audio.Play("event:/game/general/assist_screenbottom", player.Position);
				}
			} else {
				player.Die(-Vector2.UnitY);
			}
		}

		public override void Update() {
			delay -= Engine.DeltaTime;
			X = SceneAs<Level>().Camera.X;
			Player player = Scene.Tracker.GetEntity<Player>();

			// check if the entity should auto-destroy with everything else.
			if(ExtendedVariantsModule.ShouldEntitiesAutoDestroy(player)) {
				RemoveSelf();
				return;
			}

			base.Update();
			Visible = true;
			if (waiting) {
				loopSfx.Param("rising", 0f);
				if (player != null && player.JustRespawned) {
					Y = Calc.Approach(Y, player.Y - 32f, 32f * Engine.DeltaTime);
				} else {
					waiting = false;
				}
			} else {
				float screenTop = SceneAs<Level>().Camera.Top + 12f;
				if (Bottom < screenTop - 96f) {
					Bottom = screenTop - 96f;
				}
				float speed = (Bottom > screenTop ? Calc.ClampedMap(Bottom - screenTop, 0f, 32f, 1f, 0.5f) : Calc.ClampedMap(screenTop - Bottom, 0f, 96f, 1f, 2f));
				if (delay <= 0f) {
					loopSfx.Param("rising", 1f);
					Y += 30f * speed * Engine.DeltaTime;
				}
			}
			lerp = Calc.Approach(lerp, 1, Engine.DeltaTime * 4f);
			topRect.Spikey = lerp * 5f;
			topRect.UpdateMultiplier = (1f - lerp) * 2f;
		}
	}

}
