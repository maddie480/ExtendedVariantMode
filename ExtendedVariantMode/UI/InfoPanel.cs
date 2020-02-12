using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;

namespace ExtendedVariants.UI {
    // This class is heavily based on https://github.com/mrstaneh/CrowControl/blob/master/UI/InfoPanel.cs
    public class InfoPanel {
        private Vector2 uiPos = new Vector2(15, 219);
        private Texture2D pixel = null;

        private List<string> texts = new List<string>();
        private int maxWidth = 0;

        public void Update(List<string> texts) {
            this.texts = texts;
            maxWidth = (int) findMaxWidth();
        }

        private float findMaxWidth() {
            if (texts.Count == 0) {
                return 0;
            }

            float maxWidth = float.MinValue;
            foreach (string str in texts) {
                float width = ActiveFont.Measure(str).X * 0.7f;

                if (width > maxWidth) {
                    maxWidth = width;
                }
            }

            return maxWidth;
        }

        public void Render() {
            if (pixel == null) {
                pixel = new Texture2D(Draw.SpriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData(new Color[1] { Color.White });
            }

            if (texts.Count != 0) {
                Draw.SpriteBatch.Draw(pixel, new Rectangle((int) uiPos.X, (int) uiPos.Y + 5, maxWidth + 10, (texts.Count * 35) + 10), new Color(10, 10, 10, 200));

                for (int i = 0; i < texts.Count; i++) {
                    ActiveFont.Draw(texts[i], new Vector2(uiPos.X + 5, uiPos.Y + 5 + (i * 35)), new Vector2(0, 0), new Vector2(0.7f, 0.7f), Color.White);
                }
            }
        }
    }
}
