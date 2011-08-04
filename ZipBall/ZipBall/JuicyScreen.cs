using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;

namespace Juicy
{
    public class JuicyScreen
    {
        protected JuicyGame game;
        protected List<GameObj> gameObjects;
        protected Texture2D backgroundImg;
        protected Rectangle backgroundRect;

        // objects to appear on the top!
        protected List<GameObj> screenObjects;

        private List<GameObj> tempList;
        private List<GameObj> tempAddlist;

        protected string backgroundResName;

        public JuicyScreen() : this(null)
        {
            
        }

        public JuicyScreen(string bgResName)
        {
            gameObjects = new List<GameObj>();
            tempAddlist = new List<GameObj>();
            tempList = new List<GameObj>();
            screenObjects = new List<GameObj>();

            this.backgroundResName = bgResName;
        }

        public void addScreenObject(GameObj obj)
        {
            screenObjects.Add(obj);
        }

        public void addObject(GameObj obj)
        {
            gameObjects.Add(obj);
        }

        public void addAfterInit(GameObj obj)
        {
            tempAddlist.Add(obj);
        }

        public virtual void Init()
        {
        }

        public virtual void onAdd(JuicyGame game)
        {
            this.game = game;
        }

        public virtual void ScreenBecomesCurrent()
        {

        }

        public virtual void LoadSprites(ContentManager conMan)
        {

        }

        public virtual void AfterSpriteLoad()
        {

        }

        public virtual void LoadContent(ContentManager conMan)
        {
            LoadSprites(conMan);

            if (backgroundResName != null)
            {
                backgroundImg = game.Content.Load<Texture2D>(backgroundResName);

                backgroundRect = new Rectangle();
                backgroundRect.Width = backgroundImg.Width;
                backgroundRect.Height = backgroundImg.Height;
                backgroundRect.X = (game.Graphics.PreferredBackBufferWidth - backgroundImg.Width) / 2;
                backgroundRect.Y = (game.Graphics.PreferredBackBufferHeight - backgroundImg.Height) / 2;
            }
            else
            {
                backgroundImg = null;
            }
        }

        public virtual void UnLoadContent()
        {
            if (backgroundImg != null)
            {
                backgroundImg.Dispose();
                backgroundImg = null;
            }
        }

        public virtual void UpdateSpriteReferences(SpriteManager manager)
        {
            foreach (GameObj go in gameObjects)
            {
                go.UpdateSpriteReference(manager);
            }

            foreach (GameObj go in screenObjects)
            {
                go.UpdateSpriteReference(game.SprManager);
            }
        }

        public virtual void Update(long time)
        {
            int i = 0;

            foreach (GameObj go in screenObjects)
            {
                go.Update(time);
                if (go.MarkForDelete)
                    tempList.Add(go);
            }

            foreach (GameObj go in gameObjects)
            {
                go.Update(time);
                if (go.MarkForDelete)
                    tempList.Add(go);
                i++;
            }

            foreach (GameObj o in tempList)
            {
                if (!gameObjects.Remove(o))
                {
                    screenObjects.Remove(o);
                }
            }
            tempList.Clear();

            if (tempAddlist.Count > 0)
            {
                foreach (GameObj o in tempAddlist)
                {
                    gameObjects.Add(o);
                }
                tempAddlist.Clear();
            }
        }

        public virtual void Draw(SpriteBatch batch)
        {
            batch.Draw(backgroundImg, backgroundRect, Color.White);

            foreach (GameObj go in gameObjects)
            {
                go.Draw(batch);
            }
            //TODO: refactor to accomodate z-rodering
            // now draw screen objects so that they appear on top..
            foreach (GameObj go in screenObjects)
            {
                go.Draw(batch);
            }
        }

        public virtual void HandleTouch(TouchCollection tc)
        {
            TouchLocation tl = tc[0];
            Vector2 pos = tl.Position;

            // handle touch for game objects, only if game is not paused
            if (!game.IsPaused)
            {
                foreach (GameObj go in gameObjects)
                {
                    if (!go.Touchable || !go.Visible) continue;

                    if (go.Position.X <= pos.X && go.Position.Y <= pos.Y &&
                        go.Position.X + go.W >= pos.X && go.Position.Y + go.H >= pos.Y)
                    {
                        go.onTouch(tl);
                    }
                }
            }

            // for screen objects always handle event
            foreach (GameObj go in screenObjects)
            {
                if (!go.Touchable || !go.Visible) continue;

                if (go.Position.X <= pos.X && go.Position.Y <= pos.Y &&
                    go.Position.X + go.W >= pos.X && go.Position.Y + go.H >= pos.Y)
                {
                    go.onTouch(tl);
                }
            }
        }

        public void ClearObjects(bool onlyGameObjs)
        {
            gameObjects.Clear();
            if (!onlyGameObjs)
                screenObjects.Clear();
        }

        public virtual void HandleGesture(GestureSample gs)
        {

        }
    }
}
