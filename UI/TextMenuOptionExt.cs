using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace ExtendedVariants.UI {
    public class TextMenuOptionExt<T> : TextMenu.Option<T> {

        private int lastDir;
        private float sine;

        private int defaultIndex;

        public TextMenuOptionExt(string label, int defaultIndex) : base(label) {
            this.defaultIndex = defaultIndex;
        }

        // these overrides just allow to maintain lastDir and sine, since I can't access them
        public override void LeftPressed() {
            base.LeftPressed();
            if (Index > 0) lastDir = -1;
        }
        public override void RightPressed() {
            base.RightPressed();
            if (Index < Values.Count - 1) lastDir = 1;
        }
        public override void ConfirmPressed() {
            base.ConfirmPressed();
            if (Values.Count == 2) lastDir = (Index == 1) ? 1 : -1;
        }
        public override void Update() {
            base.Update();
            sine += Engine.RawDeltaTime;
        }

        public void ResetToDefault() {
            // replicate the vanilla behaviour
            PreviousIndex = Index;
            Index = defaultIndex;
            ValueWiggler.Start();
        }


        /// <summary>
        /// This is essentially the base method, but with a twist: the non-selected color is not always white.
        /// </summary>
        public override void Render(Vector2 position, bool highlighted) {
            float alpha = Container.Alpha;
            Color strokeColor = Color.Black * (alpha * alpha * alpha);
            Color color = Disabled ? Color.DarkSlateGray : ((highlighted ? this.Container.HighlightColor : getUnselectedColor()) * alpha);
            ActiveFont.DrawOutline(Label, position, new Vector2(0f, 0.5f), Vector2.One, color, 2f, strokeColor);
            if (Values.Count > 0) {
                float num = RightWidth();
                ActiveFont.DrawOutline(Values[Index].Item1, position + new Vector2(Container.Width - num * 0.5f + lastDir * ValueWiggler.Value * 8f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, color, 2f, strokeColor);
                Vector2 vector = Vector2.UnitX * (highlighted ? ((float) Math.Sin(sine * 4f) * 4f) : 0f);
                bool flag = this.Index > 0;
                Color color2 = flag ? color : (Color.DarkSlateGray * alpha);
                Vector2 position2 = position + new Vector2(Container.Width - num + 40f + ((lastDir < 0) ? (-ValueWiggler.Value * 8f) : 0f), 0f) - (flag ? vector : Vector2.Zero);
                ActiveFont.DrawOutline("<", position2, new Vector2(0.5f, 0.5f), Vector2.One, color2, 2f, strokeColor);
                bool flag2 = Index < Values.Count - 1;
                Color color3 = flag2 ? color : (Color.DarkSlateGray * alpha);
                Vector2 position3 = position + new Vector2(Container.Width - 40f + ((lastDir > 0) ? (ValueWiggler.Value * 8f) : 0f), 0f) + (flag2 ? vector : Vector2.Zero);
                ActiveFont.DrawOutline(">", position3, new Vector2(0.5f, 0.5f), Vector2.One, color3, 2f, strokeColor);
            }
        }

        /// <summary>
        /// This is the method responsible for setting non-selected color.
        /// </summary>
        /// <returns>The non-selected color</returns>
        private Color getUnselectedColor() {
            if (Index == defaultIndex) {
                return Color.White;
            }
            return Color.Goldenrod;
        }
    }
}
