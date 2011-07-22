using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;

using ZipBall;

namespace Juicy
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class JuicyGame : Microsoft.Xna.Framework.Game
    {
        protected GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        protected Dictionary<int, JuicyScreen> screens;
        protected SpriteManager spriteManager;
        protected JuicyScreen currentScreen;
        protected bool isPaused;

        public JuicyGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            screens = new Dictionary<int, JuicyScreen>();
            this.IsMouseVisible = true;
        }

        public GraphicsDeviceManager Graphics
        {
            get { return graphics; }
        }

        public bool IsPaused
        {
            get { return isPaused; }
        }

        public SpriteManager SprManager
        {
            get { return spriteManager; }
        }

        public void PauseGame()
        {
            isPaused = true;
        }

        public void UnPauseGame()
        {
            isPaused = false;
        }

        public virtual void AddScreen(int id, JuicyScreen scr)
        {
            screens.Add(id, scr);
            scr.onAdd(this);
        }

        public void SetCurrentScreen(int id)
        {
            currentScreen = screens[id];
        }

        protected virtual void AddScreens()
        {
            
            AddScreen(1, new MenuScreen("menu_background"));
            AddScreen(2, new PlayScreen("playBg"));
            SetCurrentScreen(1);
        
        }

        protected override void Initialize()
        {
            AddScreens();

            foreach (KeyValuePair<int, JuicyScreen> pair in screens)
            {
                pair.Value.Init();
            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteManager = new SpriteManager(this.Content);

            GameConfig.initConfig(this);

            foreach (KeyValuePair<int, JuicyScreen> pair in screens)
            {
                pair.Value.LoadContent(this.Content);
            }

            foreach (KeyValuePair<int, JuicyScreen> pair in screens)
            {
                pair.Value.UpdateSpriteReferences(spriteManager);
            }

            TouchPanel.EnabledGestures = GestureType.VerticalDrag;
        }

        protected override void UnloadContent()
        {
            foreach (KeyValuePair<int, JuicyScreen> pair in screens)
            {
                pair.Value.UnLoadContent();
            }
            GameConfig.uninitConfig(this);
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            
            TouchCollection touchLocations = TouchPanel.GetState();

            if (TouchPanel.IsGestureAvailable && currentScreen != null)
            {
                GestureSample gestureSample = TouchPanel.ReadGesture();
                currentScreen.HandleGesture(gestureSample);
            }

            if (touchLocations.Count > 0)
            {
                currentScreen.HandleTouch(touchLocations);
            }

            if (currentScreen != null && !isPaused)
            {
                currentScreen.Update((long)gameTime.ElapsedGameTime.TotalMilliseconds);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            if (currentScreen != null)
            {
                currentScreen.Draw(spriteBatch);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected void HandleTouch(TouchCollection tc)
        {
            if (currentScreen != null && tc.Count > 0)
            {
                currentScreen.HandleTouch(tc);
            }
        }
    }
}
