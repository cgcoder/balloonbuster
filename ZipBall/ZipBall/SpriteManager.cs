using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Juicy
{
    public class SpriteManager
    {
        protected ContentManager conMan;
        protected Dictionary<string, Texture2D> assets;

        public SpriteManager(ContentManager m)
        {
            conMan = m;
            assets = new Dictionary<string, Texture2D>();
        }

        public void LoadSprite(string resc)
        {
            Texture2D spr = conMan.Load<Texture2D>(resc);
            assets.Add(resc, spr);
        }

        public Texture2D GetSprite(string asset)
        {
            return assets[asset];
        }
    }
}
