using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;
using Juicy;

namespace ZipBall
{
    class MenuScreen : JuicyScreen
    {
        private ButtonObj[] menuButtons;
        private Texture2D backgroundImg;
        private Rectangle backgroundRect;
        private GameObj titleObj;

        private ITransform moveCenter;
        private ITransform moveOut;
        private int currentMenuButton;

        private bool menuHiding;
        private int nextScreeen;

        public MenuScreen(string bgResname) : base(bgResname)
        {
            /*
            FrameAnimator a = new FrameAnimator(100, 101);
            AnimationSequence anse = new AnimationSequence("loop", 0, 7);
            anse.Mode = AnimationSequence.AnimationMode.LOOP;
            a.AddAnimation(anse);
            obj.Animator = a;
            */
            
            menuButtons = new ButtonObj[4];

            menuButtons[0] = new ButtonObj("Play");
            menuButtons[0].SpriteName = "buttonBg";
            menuButtons[0].TouchNotifier = touchEvent;

            menuButtons[1] = new ButtonObj("High Score");
            menuButtons[1].SpriteName = "buttonBg";
            menuButtons[1].TouchNotifier = touchEvent;

            menuButtons[2] = new ButtonObj("About");
            menuButtons[2].SpriteName = "buttonBg";
            menuButtons[2].TouchNotifier = touchEvent;

            menuButtons[3] = new ButtonObj("Exit");
            menuButtons[3].SpriteName = "buttonBg";
            menuButtons[3].TouchNotifier = touchEvent;

            foreach (ButtonObj bo in menuButtons)
            {
                base.addObject(bo);
            }

            currentMenuButton = 0;

            titleObj = new GameObj();
            titleObj.SpriteName = "title";
            base.addObject(titleObj);
        }
        // first call back, 2. update sprites, 3. afterspriteload
        public override void LoadSprites(ContentManager conMan)
        {
            base.LoadSprites(conMan);

            game.SprManager.LoadSprite("buttonBg");
            game.SprManager.LoadSprite("title");
        }

        public override void AfterSpriteLoad()
        {
            base.AfterSpriteLoad();

            foreach (ButtonObj bo in menuButtons)
            {
                bo.Font = GameConfig.me().MenuFont;
            }

            titleObj.UpdatePosition((game.Graphics.PreferredBackBufferWidth - titleObj.W) / 2,
                30);

            moveCenter = new LinearTransform(0, 10, (game.Graphics.PreferredBackBufferWidth + menuButtons[0].W) / 2,
                    0);
            moveCenter.AutoReset = false;
            moveCenter.TransformCompleteEventHandler = moveToCenterComplete;

            moveOut = new LinearTransform(0, 10, game.Graphics.PreferredBackBufferWidth / 2 + menuButtons[0].W, 0);
            moveOut.AutoReset = false;
            moveOut.TransformCompleteEventHandler = moveOutCenterComplete;
        }

        public override void ScreenBecomesCurrent()
        {
            moveCenter.Reset();
            moveOut.Reset();

            menuButtons[0].Position = new Vector2(-250, 200);
            menuButtons[1].Position = new Vector2(-250, 270);
            menuButtons[2].Position = new Vector2(-250, 340);
            menuButtons[3].Position = new Vector2(-250, 410);

            foreach (ButtonObj bo in menuButtons)
            {
                bo.clearTransforms();
                bo.AddTransform(moveCenter);
                bo.DisableTransform = true;
            }

            menuButtons[0].DisableTransform = false;

        }

        public void moveOutCenterComplete(ITransform t)
        {
            moveToCenterComplete(t);
            if (currentMenuButton == 0)
            {
                if (nextScreeen < 0)
                {
                    game.Exit();
                }
                else
                {
                    game.SetCurrentScreen(nextScreeen);
                }
            }

        }

        public void moveToCenterComplete(ITransform t)
        {
            t.Reset();

            menuButtons[currentMenuButton].DisableTransform = true;
            currentMenuButton++;
            if (currentMenuButton == menuButtons.Length)
            {
                currentMenuButton = 0;
            }
            else
            {
                menuButtons[currentMenuButton].DisableTransform = false;
            }
        }

        protected void startMenuHiding()
        {
            foreach(ButtonObj bo in menuButtons)
            {
                bo.clearTransforms();
                bo.AddTransform(moveOut);
                bo.DisableTransform = true;
            }

            menuButtons[0].DisableTransform = false;
        }

        public void touchEvent(TouchLocation tl, GameObj go)
        {
            if (tl.State == TouchLocationState.Released)
            {
                if (go == menuButtons[0]) 
                {
                    nextScreeen = 2;
                    startMenuHiding();
                }
                else if (go == menuButtons[1])
                {
                    nextScreeen = 3;
                    startMenuHiding();
                }
                else if (go == menuButtons[3])
                {
                    nextScreeen = -1;
                    startMenuHiding();
                }
                else if (go == menuButtons[2])
                {
                    nextScreeen = 4;
                    startMenuHiding();
                }
            }
        }
    }
}
