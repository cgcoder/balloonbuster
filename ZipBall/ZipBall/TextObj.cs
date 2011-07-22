using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Juicy;

namespace ZipBall
{
    public class TextObj  : GameObj
    {
        private SpriteFont font;
        private string text;
        private Color color;

        public TextObj()
        {
            color = Color.White;
        }

        public SpriteFont Font
        {
            get { return font; }
            set { font = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public Color FontColor
        {
            get { return color; }
            set { color = value; }
        }

        public override void Draw(SpriteBatch batch)
        {
            if (text != null && font != null)
            {
                batch.DrawString(font, text, position, color);
            }
        }
    }
}
