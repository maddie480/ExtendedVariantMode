using Celeste;
using Celeste.Mod;
using Celeste.Mod.UI;
using ExtendedVariants.Module;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.UI
{
    public class InputSearchUI : Entity
    {
        private static FieldInfo overworld_inputEase = typeof(Overworld).GetField("inputEase", BindingFlags.NonPublic | BindingFlags.Instance);

        public VirtualButton Key;
        public static InputSearchUI Instance;
        public InputSearchUI(Overworld overworld)
        {
            Instance = this;

            Tag = Tags.HUD | Tags.PauseUpdate;
            Depth = -10000;
            Add(Wiggle);
            Overworld = overworld;
            Key = Input.QuickRestart;
        }
        private float WiggleDelay;
        private Wiggler Wiggle = Wiggler.Create(0.4f, 4f, null, false, false);

        public float inputEase;
        public bool ShowSearchUI;
        public Overworld Overworld;
        public float Overworld_inputEase { get => Overworld == null ? 0f : (float)overworld_inputEase.GetValue(Overworld); }

        public override void Update()
        {
            if (Key.Pressed && WiggleDelay <= 0f)
            {
                Wiggle.Start();
                WiggleDelay = 0.5f;
            }
            WiggleDelay -= Engine.DeltaTime;
            inputEase = Calc.Approach(inputEase, (ShowSearchUI ? 1 : 0), Engine.DeltaTime * 4f);
            base.Update();
        }
        public override void Render()
        {
            if (inputEase > 0f)
            {
                float num = 0.5f;
                float num2 = Overworld_inputEase > 0f ? 48f : 0f;
                string label = Dialog.Clean("MAPLIST_SEARCH");
                float num3 = ButtonUI.Width(label, Key);

                Vector2 position = new Vector2(1880f, 1024f - num2);
                position.X += (40f + num3 * num + 32f) * (1f - Ease.CubeOut(inputEase));
                ButtonUI.Render(position, label, Key, num, 1f, Wiggle.Value * 0.05f, 1f);
            }
        }
    }
}