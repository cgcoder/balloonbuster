using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;


using ZipBall;
using Microsoft.Xna.Framework;

namespace Juicy
{
    public class ButtonObj : GameObj
    {
        public delegate void ButtonClicked();

        private ButtonClicked buttonClickHandler;
        private string text;
        private SpriteFont font;
        private Vector2 pos;
        private Vector2 fontMetric;

        public ButtonObj(string text)
        {
            pos = new Vector2();
            this.text = text;
            reCalSize();
        }

        protected void reCalSize()
        {
            if (font != null && boundary != null)
            {
                fontMetric = font.MeasureString(text);
            }
        }

        public SpriteFont Font
        {
            get { return font; }
            set { font = value; reCalSize(); }
        }

        public ButtonClicked ButtonClickHandler
        {
            get { return buttonClickHandler; }
            set { buttonClickHandler = value; }
        }

        public override void UpdateSpriteReference(SpriteManager manager)
        {
            base.UpdateSpriteReference(manager);
            reCalSize();
        }
        
        protected override void updateChildObjs()
        {
            base.updateChildObjs();
            pos.X = Position.X + (boundary.Width - fontMetric.X) / 2;
            pos.Y = Position.Y + (boundary.Height - fontMetric.Y) / 2;
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
            batch.DrawString(font, text, pos, Color.White);
        }
    }
}
