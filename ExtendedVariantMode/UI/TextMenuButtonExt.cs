using Celeste;
using Microsoft.Xna.Framework;
using System;

namespace ExtendedVariants.UI {
    public class TextMenuButtonExt : TextMenu.Button {
        /// <summary>
        /// Function that should determine if the button has to be highlighted or not.
        /// Defaults to false.
        /// </summary>
        public Func<bool> GetHighlight { get; set; } = () => false;

        public TextMenuButtonExt(string label) : base(label) { }

        /// <summary>
        /// This is the same as the vanilla method, except it calls getUnselectedColor() to get the button color
        /// instead of always picking white.
        /// This way, when we change Highlight to true, the button is highlighted like all the "non-default value" options are.
        /// </summary>
        public override void Render(Vector2 position, bool highlighted) {
            float alpha = Container.Alpha;
            Color color = Disabled ? Color.DarkSlateGray : ((highlighted ? Container.HighlightColor : getUnselectedColor()) * alpha);
            Color strokeColor = Color.Black * (alpha * alpha * alpha);
            bool flag = Container.InnerContent == TextMenu.InnerContentMode.TwoColumn && !AlwaysCenter;
            Vector2 position2 = position + (flag ? Vector2.Zero : new Vector2(Container.Width * 0.5f, 0f));
            Vector2 justify = (flag && !AlwaysCenter) ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            ActiveFont.DrawOutline(Label, position2, justify, Vector2.One, color, 2f, strokeColor);
        }

        private Color getUnselectedColor() {
            if (GetHighlight()) return Color.Goldenrod;
            return Color.White;
        }
    }
}
