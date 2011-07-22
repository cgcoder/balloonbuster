using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;

using Juicy;

namespace ZipBall
{
    class PlayScreen : JuicyScreen
    {
        private enum GameState
        {
            WAITING, PAUSED, PLAY, OVER
        };

        private enum CreationMode
        {
            NORMAL, LOT_OF_GIFT, LOT_OF_GIFT_BOMB, LOT_OF_BOMB_BALLOON
        };

        private CreationMode CreateMode;
        private Random random;
        private int Level;
        private int Score;
        private int BalloonCount;
        private int MaxBalloon;
        private int BalloonsMissed;
        private int framesSinceLastCreate;
        private float MinBalloonSpeed;
        private int RunningScore;
        private int freqDivider;
        private Vector2 scorePos;
        private ContentManager contentMan;
        private Color white = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        private int screenHeight;
        private bool isBannerOn;
        private int bout;
        private int maxFrameGap;
        private int BallCreateProbability;

        private float BombSpeedVar;
        private float SpeedVar;
        private float GiftSpeedVar;

        public PlayScreen(string bgResname) : base(bgResname)
        {
            random = new Random(System.DateTime.UtcNow.Millisecond);
            freqDivider = 0;
            scorePos = new Vector2(5, 3);
            MinBalloonSpeed = -1.5f;
            isBannerOn = false;
            Level = 5;
            CreateMode = CreationMode.NORMAL;

            UpdateLevelParameters();
        }

        public override void LoadContent(ContentManager conMan)
        {
            base.LoadContent(conMan);

            game.SprManager.LoadSprite("Red_Balloon");
            game.SprManager.LoadSprite("Green_Balloon");
            game.SprManager.LoadSprite("Red_Balloon_Gift");
            game.SprManager.LoadSprite("Red_Balloon_Bomb");
            game.SprManager.LoadSprite("Yellow_Balloon");
            game.SprManager.LoadSprite("Red_Balloon_Triple");
            game.SprManager.LoadSprite("gift");
            game.SprManager.LoadSprite("awesome");
            this.contentMan = conMan;
        }

        public override void Draw(SpriteBatch batch)
        {
            //obj.Draw(batch);
            base.Draw(batch);
            drawScoreBoard(batch);
        }

        private void drawScoreBoard(SpriteBatch batch)
        {
            string text = "1.4v Score : " + Convert.ToString(RunningScore) + "/" + "Bs: " + Convert.ToString(BalloonCount);
            batch.DrawString(GameConfig.me().ScoreFont, text, scorePos, white);
        }

        public void reset()
        {
            Level = 0;
            Score = 0;
            BalloonCount = 0;
            MaxBalloon = 2;
            //fix
            BalloonsMissed = 0;
            RunningScore = 0;
        }

        public override void Update(long time)
        {
            framesSinceLastCreate++;
            base.Update(time);

            if (canCreateBalloon())
            {
                randomlyPickBalloonAndCreate();
            }

            freqDivider++;

            if (freqDivider == 2)
            {
                if (RunningScore < Score) RunningScore++;
                freqDivider = 0;
            }
        }

        private TextObj GetText(string txt, GameObj go)
        {
            TextObj scoreText = new TextObj();
            scoreText.Position = go.Position;
            scoreText.Font = GameConfig.me().ScoreFont;
            scoreText.Text = txt;

            scoreText.AddTransform(new LinearTransform(0, 60, (int)(50 - scoreText.Position.X), (int)(10 - scoreText.Position.Y)));

            scoreText.TransformNotifier = ScoreTransformEnded;

            return scoreText;
        }

        public void balloonTouched(TouchLocation loc, GameObj go)
        {
            if (loc.State != TouchLocationState.Released) return;

            BalloonObj b = (BalloonObj)go;
            b.TouchesRequiredToBust = b.TouchesRequiredToBust - 1;

            if (b.TouchesRequiredToBust == 0)
            {
                b.Touchable = false;

                // update animation for busted
                // based on balloon type add another object (for diamon, or star)
                //b.MarkForDelete = true;
                b.Animator.CurrentAnimationName = "bursting";
                b.updateSpeed(0, 0);

                base.addAfterInit(GetText("+10", b));

                // now add subobjects of the balloon...
                if (b.getSubObjects() == null) return;
                int i = 0;
                foreach (GameObj go2 in b.getSubObjects())
                {
                    go2.Visible = true;
                    if (b.Type == BalloonObj.BalloonType.TRIPLE)
                    {
                        int tx = i == 0 ? (int) (b.Position.X - 30 - random.Next(20)) : (int)b.Position.X + 30 + random.Next(20);
                        // make sure balloons are within the screen
                        if (tx < 5) tx = 10;
                        if (tx > game.Graphics.PreferredBackBufferWidth - 20)
                            tx = game.Graphics.PreferredBackBufferWidth - 30;

                        go2.UpdatePosition(tx, (int)b.Position.Y);
                    }
                    else
                    {
                        go2.UpdatePosition((int)b.Position.X, (int)b.Position.Y);
                    }
                    i++;
                }
            }
        }

        private void UpdateLevelParameters()
        {
            BallCreateProbability = 50;
            BombSpeedVar = 1.0f;
            GiftSpeedVar = 2.5f;
            SpeedVar = 1.75f;

            if (Level == 0)
            {
                MinBalloonSpeed = -1.5f;
                MaxBalloon = 2;
                maxFrameGap = 40;
            }
            else if (Level == 1)
            {
                MinBalloonSpeed = -2.0f;
                MaxBalloon = 3;
            }
            else if (Level == 2)
            {
                MaxBalloon = 5;
                maxFrameGap = 30; 
            }
            else if (Level == 3)
            {
                MinBalloonSpeed = -2.5f;
                MaxBalloon = 3;
            }
            else if (Level == 4)
            {
                MinBalloonSpeed = -2.0f;
                MaxBalloon = 6;
            }
            else if (Level == 5)
            {
                MinBalloonSpeed = -2.0f;
                MaxBalloon = 7;
                maxFrameGap = 40;
            }
        }

        private bool canCreateBalloon()
        {
            // random, method to check if we can create the next balloon
           
            if (framesSinceLastCreate >= maxFrameGap)
            {
                if (BalloonCount < 0) 
                    BalloonCount = 0; // should never happen
                return true;
            }

            if (Level == 0 && framesSinceLastCreate <= 100)
            {
                return false;
            }
            else if (Level == 1 && framesSinceLastCreate <= 100)
            {
                return false;
            }
            return (random.Next(BallCreateProbability) == 10);
            
        }

        private void goToSpecialMode(CreationMode NewMode)
        {
            CreateMode = NewMode;
            switch (CreateMode)
            {
                case CreationMode.NORMAL:
                    UpdateLevelParameters(); // reset to normal params
                break;
            }
        }

        private void randomlyPickBalloonAndCreate()
        {
            int r = random.Next(10);

            if (CreateMode == CreationMode.LOT_OF_GIFT)
            {
                r = 6 + random.Next(3);
            }
            else if (CreateMode == CreationMode.LOT_OF_BOMB_BALLOON)
            {
                r = random.Next(5) <= 3 ? 3 : 7; // create lot of bomb with occasion good ones!
            }

            if (r == 2)
            {
                if (Level > 3)
                {
                    createBalloon(BalloonObj.BalloonType.TRIPLE, BalloonObj.BalloonColor.RED);
                }
                else
                {
                    r = 3;
                }
            }

            if (r == 3)
            {
                createBalloon(BalloonObj.BalloonType.BOMB, BalloonObj.BalloonColor.RED);
            }
            else if (r == 4 || r == 5)
            {
                createBalloon(BalloonObj.BalloonType.SINGLE, BalloonObj.BalloonColor.YELLOW);
            }
            else if (r <= 7)
            {
                createBalloon(BalloonObj.BalloonType.SINGLE, BalloonObj.BalloonColor.RED);
            }
            else if (r == 8)
            {
                createBalloon(BalloonObj.BalloonType.GIFT, BalloonObj.BalloonColor.RED);
            }
            else
            {
                createBalloon(BalloonObj.BalloonType.SINGLE, BalloonObj.BalloonColor.GREEN);
            }
        }

        private BalloonObj makeBalloon(BalloonObj.BalloonType type, BalloonObj.BalloonColor color)
        {
            BalloonObj balloon = new BalloonObj();
            balloon.SpriteName = formBalloonName(type, color);
            balloon.Scale = 2;
            balloon.TouchNotifier = balloonTouched;
            balloon.Type = type;
            balloon.Color = color;

            FrameAnimator a = new FrameAnimator(196 / 4, BalloonObj.BalloonType.GIFT == type || BalloonObj.BalloonType.BOMB == type ? 80 : 60);
            AnimationSequence flyingAnse = new AnimationSequence("flying", 0, 3);
            flyingAnse.Mode = AnimationSequence.AnimationMode.LOOP;

            AnimationSequence burstingAnse = new AnimationSequence("bursting", 4, 7);
            burstingAnse.Mode = AnimationSequence.AnimationMode.STOP_AT_END;

            a.AddAnimation(flyingAnse);
            a.AddAnimation(burstingAnse);

            balloon.Animator = a;
            balloon.AnimationNotifier = balloonBurstAnimationEnded;
            balloon.Animator.CurrentAnimationName = "flying";

            balloon.UpdateSpriteReference(game.SprManager);
            balloon.W = 100;
            balloon.UpdatePosition(10 + random.Next(game.Graphics.PreferredBackBufferWidth - 100), game.Graphics.PreferredBackBufferHeight + 20);
            screenHeight = game.Graphics.PreferredBackBufferHeight;

            balloon.BalloonOutNotifier = balloonOutEvent;
            balloon.TouchNotifier = balloonTouched;
            balloon.TouchesRequiredToBust = 1;

            if (BalloonObj.BalloonType.BOMB == type)
            {
                balloon.updateSpeed(0, -2.25f - ((float) random.NextDouble())*BombSpeedVar);
            }
            else if (BalloonObj.BalloonType.GIFT == type)
            {
                balloon.updateSpeed(0, -3.0f - ((float)random.NextDouble())*GiftSpeedVar);
            }
            else
            {
                balloon.updateSpeed(0, MinBalloonSpeed - ((float)random.NextDouble())*SpeedVar);
            }

            return balloon;
        }

        private void createBalloon(BalloonObj.BalloonType type, BalloonObj.BalloonColor color)
        {
            BalloonCount++; // some data to optmize ball creation probability
            framesSinceLastCreate = 0; // to tweak game difficulty

            BalloonObj balloon = makeBalloon(type, color);

            if (type == BalloonObj.BalloonType.GIFT)
            {
                balloon.AddSubObjects(createGiftSubObj());
            }
            else if (type == BalloonObj.BalloonType.TRIPLE)
            {
                balloon.AddSubObjects(makeBalloon(BalloonObj.BalloonType.SINGLE, BalloonObj.BalloonColor.GREEN));
                balloon.AddSubObjects(makeBalloon(BalloonObj.BalloonType.SINGLE, BalloonObj.BalloonColor.RED));
            }

            if (balloon.getSubObjects() != null)
            {
                foreach (GameObj go in balloon.getSubObjects())
                {
                    go.Visible = false;
                    base.addObject(go);
                }
            }

            base.addObject(balloon);
        }

        private class GiftObj : GameObj
        {
            private float tempY;

            public override void Update(long timer)
            {
                base.Update(timer);
                int ty;
                tempY += 1.6f;

                ty = (int) Math.Floor(tempY);
                tempY -= ty;

                base.UpdatePosition((int)position.X, (int)position.Y + ty);

                base.Update(timer);

                if (position.Y > 420) //TODO: fix this
                {
                    this.MarkForDelete = true;
                }
            }
        }

        private GameObj createGiftSubObj()
        {
            GameObj g = new GiftObj();
            g.SpriteName = "gift";

            g.TouchNotifier = giftTouched;
            g.Scale = 1;
            FrameAnimator a = new FrameAnimator(45, 60);
            AnimationSequence flyingAnse = new AnimationSequence("dropping", 0, 6);
            flyingAnse.Mode = AnimationSequence.AnimationMode.LOOP;

            AnimationSequence burstingAnse = new AnimationSequence("bursting", 7, 9);
            burstingAnse.Mode = AnimationSequence.AnimationMode.STOP_AT_END;

            a.AddAnimation(flyingAnse);
            a.AddAnimation(burstingAnse);

            g.Animator = a;
            g.AnimationNotifier = giftAnimationEnded;
            g.Animator.CurrentAnimationName = "dropping";
            g.UpdateSpriteReference(game.SprManager);

            return g;
        }

        private void createAwesomeBanner()
        {
            if (!isBannerOn)
            {
                isBannerOn = true;
                GameObj g = new GameObj();
                g.SpriteName = "awesome";

                g.UpdateSpriteReference(game.SprManager);
                g.UpdatePosition(0, 150);

                g.AddTransform(new LinearTransform(0, 150, game.Graphics.PreferredBackBufferWidth, 0));
                g.TransformNotifier = delegate(GameObj o)
                {
                    o.MarkForDelete = true;
                    isBannerOn = false;
                };
                base.addAfterInit(g);
            }
        }

        private void giftTouched(TouchLocation loc, GameObj go)
        {
            //go.MarkForDelete = true;
            GiftObj gift = (GiftObj)go;
            gift.Touchable = false;
            gift.Animator.CurrentAnimationName = "bursting";
            base.addAfterInit(GetText("+30", go));

            createAwesomeBanner();
        }

        private string formBalloonName(BalloonObj.BalloonType type, BalloonObj.BalloonColor color)
        {
            string typeStr = BalloonObj.TYPE_STR[(int)type];
            return BalloonObj.COLOR_STR[(int)color] + "_Balloon" + (typeStr.Length > 1 ? "_" + typeStr : typeStr); 
        }

        private void balloonOutEvent(BalloonObj b)
        {
            if (!b.MarkForDelete)
            {
                b.MarkForDelete = true;
                if (b.Visible)
                    BalloonCount--;
                bout++;
            }
        }

        private void balloonBurstAnimationEnded(FrameAnimator animator, Juicy.AnimationSequence.AnimationStatus status)
        {
            if (animator.GO.MarkForDelete)
            {
                animator.GO.MarkForDelete = true;
            }
            else
            {
                animator.GO.MarkForDelete = true;
                BalloonCount--;
            }
        }

        private void giftAnimationEnded(FrameAnimator animator, Juicy.AnimationSequence.AnimationStatus status)
        {
            animator.GO.MarkForDelete = true;
        }

        private void ScoreTransformEnded(GameObj o)
        {
            TextObj t = (TextObj)o;
            Score += Convert.ToInt32(t.Text);
            o.MarkForDelete = true;
        }

        public override void HandleGesture(GestureSample gs)
        {
            if (gs.GestureType == GestureType.Pinch)
            {
                if (game.IsPaused)
                {
                    game.UnPauseGame();
                }
                else
                {
                    game.PauseGame();
                }
            }
        }
    }
}
