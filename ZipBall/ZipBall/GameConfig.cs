using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace ZipBall
{
    public class GameConfig
    {
        private static GameConfig config;

        private SpriteFont menuFont;
        private SpriteFont scoreFont;

        public static GameConfig me()
        {
            return config;
        }

        public static void initConfig(Juicy.JuicyGame game)
        {
            config = new GameConfig(game);
        }

        public static void uninitConfig(Juicy.JuicyGame game)
        {
            config.uninit(game);
        }

        private GameConfig(Juicy.JuicyGame game)
        {
            menuFont = game.Content.Load<SpriteFont>("menuText");
            scoreFont = game.Content.Load<SpriteFont>("scoreText");
        }

        private void uninit(Juicy.JuicyGame game)
        {
        }

        public SpriteFont DefaultFont
        {
            get { return menuFont; }
        }

        public SpriteFont MenuFont
        {
            get { return menuFont; }
        }

        public SpriteFont ScoreFont
        {
            get { return scoreFont; }
        }
    }
}
