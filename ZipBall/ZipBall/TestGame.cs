using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juicy
{
    class TestGame : JuicyGame
    {

        public TestGame()
        {
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            //graphics.IsFullScreen = true;
        }

        protected override void AddScreens()
        {
            AddScreen(1, new MenuScreen());
            SetCurrentScreen(1);
        }
    }
}
