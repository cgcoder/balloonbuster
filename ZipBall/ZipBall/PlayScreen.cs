using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Juicy;

namespace ZipBall
{
    class PlayScreen : JuicyScreen
    {
        private enum GameState
        {
            WAITING, PLAY, OVER
        };

        private enum CreationMode
        {
            NORMAL, LOT_OF_GIFT, LOT_OF_GIFT_BOMB, LOT_OF_BOMB_BALLOON, LOT_OF_BOMB_SUDDEN_NORMAL
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

        private float BombSpeedVar;
        private float SpeedVar;
        private float GiftSpeedVar;
        private int BombPropability;
        private int BalloonProbability;
        private int GiftProbability;
        private int DifferBalloonProb;

        private long RequestedTimeGap;
        private int explosionsRequired;
        private GameObj pauseObj;
        private GameState gameState;
        // when was the last time pause button was touched
        // this is too avoid quick touching to pause button twice, 
        // when intented to do only once.
        private long lastPauseTouch;
        private GameObj pauseTextObj;
        private GameObj gameOverObj;
        private ButtonObj quitButton;

        public PlayScreen(string bgResname) : base(bgResname)
        {
            random = new Random(System.DateTime.UtcNow.Millisecond);
            freqDivider = 0;
            scorePos = new Vector2(5, 3);
            MinBalloonSpeed = -1.5f;
            isBannerOn = false;
            Level = 0;
            CreateMode = CreationMode.NORMAL;

            UpdateLevelParameters();
            explosionsRequired = 5;
            gameState = GameState.PLAY;
            lastPauseTouch = 0;

            // pause button
            pauseObj = new GameObj();
            pauseObj.SpriteName = "pause";
            pauseObj.TouchNotifier = PauseTouchEvent;
            this.addScreenObject(pauseObj);

            pauseTextObj = new GameObj();
            pauseTextObj.SpriteName = "pausetext";
            pauseTextObj.TouchNotifier = PauseTextTouchEvent;
            pauseTextObj.Visible = false;
            this.addScreenObject(pauseTextObj);

            quitButton = new ButtonObj("Quit");
            quitButton.SpriteName = "buttonBg";
            quitButton.TouchNotifier = delegate(TouchLocation loc, GameObj go)
            {
                reset();
                game.UnPauseGame();
                game.SetCurrentScreen(1);
            };
            quitButton.Visible = false;
            this.addScreenObject(quitButton);

            gameOverObj = new GameObj();
            gameOverObj.SpriteName = "gameover";
            gameOverObj.TouchNotifier = delegate(TouchLocation loc, GameObj go)
            {
                int screen = 1;
                if (Guide.IsVisible) return;

                if (game.ScoreManager.IsHighScore(Score))
                {
                    startInput();
                    // show input.. get name
                    // save high score..
                    // screen = 3;
                }
                else
                {
                    reset();
                    game.UnPauseGame();
                    game.SetCurrentScreen(screen);
                }
            };
            gameOverObj.Visible = false;
            this.addScreenObject(gameOverObj);
        }

        private void startInput()
        {
            if (!Guide.IsVisible)
                Guide.BeginShowKeyboardInput(PlayerIndex.One, "Enter Name", "You got high score! enter your name: ", "anonymous", new AsyncCallback(doneInput), null);
        }

        private void doneInput(IAsyncResult result)
        {
            string name = Guide.EndShowKeyboardInput(result);
            Score s = new Score
            {
                PlayerName = name,
                PlayerScore = RunningScore
            };

            game.ScoreManager.CheckInsertHighScore(s);

            reset();
            game.UnPauseGame();
            game.SetCurrentScreen(3);
        }

        public override void LoadSprites(ContentManager conMan)
        {
            base.LoadSprites(conMan);
            game.SprManager.LoadSprite("Red_Balloon");
            game.SprManager.LoadSprite("Green_Balloon");
            game.SprManager.LoadSprite("Red_Balloon_Gift");
            game.SprManager.LoadSprite("Red_Balloon_Bomb");
            game.SprManager.LoadSprite("Yellow_Balloon");
            game.SprManager.LoadSprite("Red_Balloon_Triple");
            game.SprManager.LoadSprite("gift");
            game.SprManager.LoadSprite("awesome");
            game.SprManager.LoadSprite("explosive");
            game.SprManager.LoadSprite("gameover");
            game.SprManager.LoadSprite("pause");
            game.SprManager.LoadSprite("pausetext");
            game.SprManager.LoadSprite("buttonBg");
        }

        public override void AfterSpriteLoad()
        {
            quitButton.Font = GameConfig.me().MenuFont;

            pauseObj.UpdatePosition(5, game.Graphics.PreferredBackBufferHeight - 35);

            pauseTextObj.UpdatePosition((game.Graphics.PreferredBackBufferWidth - pauseTextObj.W) / 2,
                (game.Graphics.PreferredBackBufferHeight - pauseTextObj.H) / 2);

            quitButton.UpdatePosition((game.Graphics.PreferredBackBufferWidth - quitButton.W) / 2,
                (game.Graphics.PreferredBackBufferHeight - pauseTextObj.H) / 2 + 150);

            gameOverObj.UpdatePosition((game.Graphics.PreferredBackBufferWidth - gameOverObj.W) / 2,
                (game.Graphics.PreferredBackBufferHeight - gameOverObj.H) / 2);
        }

        public override void LoadContent(ContentManager conMan)
        {
            this.contentMan = conMan;
            base.LoadContent(conMan);
        }

        public override void Draw(SpriteBatch batch)
        {
            //obj.Draw(batch);
            base.Draw(batch);
            drawScoreBoard(batch);
        }

        public override void ScreenBecomesCurrent()
        {
            reset();
        }

        private void drawScoreBoard(SpriteBatch batch)
        {
            string text = "1.8v Score : " + Convert.ToString(RunningScore) + "/" + "Missed : " + BalloonsMissed;
            batch.DrawString(GameConfig.me().ScoreFont, text, scorePos, white);
        }

        public void reset()
        {
            Level = 0;
            Score = 0;
            BalloonCount = 0;
            BalloonsMissed = 0;
            RunningScore = 0;
            explosionsRequired = 5;

            pauseTextObj.Visible = false;
            quitButton.Visible = false;

            gameState = GameState.PLAY;
            gameOverObj.Visible = false;

            ClearObjects(true);
        }

        public override void Update(long time)
        {

            // update game only when its running!

            framesSinceLastCreate++;
            base.Update(time);

            if (RequestedTimeGap > 0)
            {
                RequestedTimeGap--;
                if (RequestedTimeGap == 0) AfterTimeDuration(); 
            }

            if (canCreateBalloon())
            {
                randomlyPickBalloonAndCreate();
            }

            freqDivider++;

            if (freqDivider == 2)
            {
                if (RunningScore < Score) RunningScore++;
                CheckLevelAndUpdate();
                freqDivider = 0;
            }
        }

        private void CheckLevelAndUpdate()
        {
            if (RunningScore == 500)
            {
                goToSpecialMode(CreationMode.LOT_OF_BOMB_SUDDEN_NORMAL);
                RequestCallback(2000);
            }
            else if (RunningScore == 600)
            {
                Level = 1;
                goToSpecialMode(CreationMode.NORMAL);
            }
            else if (RunningScore == 1200)
            {
                Level = 2;
                goToSpecialMode(CreationMode.NORMAL);
            }
            else if (RunningScore == 1800)
            {
                Level = 3;
                goToSpecialMode(CreationMode.NORMAL);
            }
            else if (RunningScore == 2200)
            {
                goToSpecialMode(CreationMode.LOT_OF_GIFT_BOMB);
                RequestCallback(2500);
            }
            else if (RunningScore == 3500)
            {
                Level = 4;
                goToSpecialMode(CreationMode.NORMAL);
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

        public void PauseTextTouchEvent(TouchLocation loc, GameObj go)
        {
            gameState = GameState.PLAY;
            go.Visible = false;
        }

        public void PauseTouchEvent(TouchLocation loc, GameObj go)
        {
            if (game.CurrentFrameTime - lastPauseTouch > 20 && gameState == GameState.PLAY)
            {
                if (game.IsPaused) game.UnPauseGame();
                else game.PauseGame();

                lastPauseTouch = game.CurrentFrameTime;
                pauseTextObj.Visible = game.IsPaused;
                quitButton.Visible = game.IsPaused;
            }
        }

        public void balloonTouched(TouchLocation loc, GameObj go)
        {
            if (loc.State != TouchLocationState.Released || gameState == GameState.OVER) return;

            BalloonObj b = (BalloonObj)go;
            b.TouchesRequiredToBust = b.TouchesRequiredToBust - 1;

            if (b.Type == BalloonObj.BalloonType.BOMB)
            {
                b.Touchable = false;
                b.MarkForDelete = true;
                explosionsRequired = 3;
                gameState = GameState.OVER;

                addExplosion(false, b.Position);
                addExplosion(true, b.Position);
                addExplosion(true, b.Position);
            }
            else if (b.TouchesRequiredToBust == 0)
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
                        go2.UpdatePosition((int)b.Position.X, (int)b.Position.Y + 40);
                    }
                    i++;
                }
            }
        }

        private void UpdateLevelParameters()
        {
            BombSpeedVar = 1.0f;
            GiftSpeedVar = 2.5f;
            SpeedVar = 1.75f;

            maxFrameGap = 60;
            BombPropability = 200;
            BalloonProbability = 70;
            GiftProbability = 150;
            DifferBalloonProb = 3;

            if (Level == 0)
            {
                MinBalloonSpeed = -1.5f;
                MaxBalloon = 2;
            }
            else if (Level == 1)
            {
                MinBalloonSpeed = -2.0f;
                MaxBalloon = 3;
                maxFrameGap = 40;
            }
            else if (Level == 2)
            {
                MaxBalloon = 5;
                maxFrameGap = 30;
                BalloonProbability = 60;
            }
            else if (Level == 3)
            {
                MinBalloonSpeed = -2.5f;
                BalloonProbability = 40;
                maxFrameGap = 40;
                MaxBalloon = 3;
                DifferBalloonProb = 4;
            }
            else if (Level == 4)
            {
                MinBalloonSpeed = -2.5f;
                MaxBalloon = 6;
                maxFrameGap = 20;
                BalloonProbability = 30;
                GiftProbability = 70;
                BombPropability = 120;
                DifferBalloonProb = 4;
            }
            else if (Level == 5)
            {
                MinBalloonSpeed = -2.0f;
                MaxBalloon = 7;
                maxFrameGap = 40;
                DifferBalloonProb = 4;
            }

            // special tweaking for special modes..
            if (CreateMode == CreationMode.NORMAL)
                return;

            if (CreateMode == CreationMode.LOT_OF_BOMB_SUDDEN_NORMAL)
            {
                MinBalloonSpeed = -3.5f;
                BombPropability = 30;
                maxFrameGap = 30;
                GiftProbability = 0;
                BalloonProbability = 150;
                DifferBalloonProb = 1;
            }
            else if (CreateMode == CreationMode.LOT_OF_GIFT_BOMB)
            {
                MinBalloonSpeed = -2.5f;
                BombPropability = 150;
                maxFrameGap = 20;
                GiftProbability = 20;
                BalloonProbability = 1;
                DifferBalloonProb = 3;
            }
        }

        private bool canCreateBalloon()
        {
            // random, method to check if we can create the next balloon
            if (gameState != GameState.PLAY) return false;

            if (framesSinceLastCreate >= maxFrameGap)
            {
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
            return (random.Next(BalloonProbability) == 10);
            
        }

        private void addExplosion(bool afterEffect, Vector2 pos)
        {
            GameObj obj = new GameObj();

            obj.SpriteName = "explosive";

            FrameAnimator a = new FrameAnimator(200, 200);
            AnimationSequence bombing = new AnimationSequence("exploding", 0, 9);
            bombing.Mode = AnimationSequence.AnimationMode.STOP_AT_END;

            AnimationSequence bombing2 = new AnimationSequence("exploding2", 2, 9);

            a.AddAnimation(bombing);
            a.AddAnimation(bombing2);

            obj.Animator = a;
            obj.AnimationNotifier = explodeAnimationEnded;

            obj.Animator.CurrentAnimationName = afterEffect ? "exploding2" : "exploding";

            obj.UpdateSpriteReference(game.SprManager);
            obj.UpdatePosition((int) pos.X - 100 + random.Next(150), (int) pos.Y - 100 + random.Next(150));

            addAfterInit(obj);
        }

        private void goToSpecialMode(CreationMode NewMode)
        {
            CreateMode = NewMode;
            UpdateLevelParameters();
        }

        private void randomlyPickBalloonAndCreate()
        {
            int r = 0;

            if (random.Next(BombPropability) == 1)
            {
                createBalloon(BalloonObj.BalloonType.BOMB, BalloonObj.BalloonColor.RED);
            }
            else if (random.Next(GiftProbability) == 1)
            {
                createBalloon(BalloonObj.BalloonType.GIFT, BalloonObj.BalloonColor.RED);
            }
            
            if ((random.Next(BalloonProbability)) == 1 || framesSinceLastCreate >= 5*maxFrameGap)
            {
                r = random.Next(DifferBalloonProb);

                switch (r % DifferBalloonProb)
                {
                    case 0:
                        createBalloon(BalloonObj.BalloonType.SINGLE, BalloonObj.BalloonColor.RED);
                        break;
                    case 1:
                        createBalloon(BalloonObj.BalloonType.SINGLE, BalloonObj.BalloonColor.YELLOW);
                        break;
                    case 2:
                        createBalloon(BalloonObj.BalloonType.SINGLE, BalloonObj.BalloonColor.GREEN);
                        break;
                    case 3:
                        createBalloon(BalloonObj.BalloonType.TRIPLE, BalloonObj.BalloonColor.RED);
                        break;
                }

                return;
            }
            else if (CreateMode == CreationMode.LOT_OF_GIFT)
            {
                r = 6 + random.Next(3);
            }
            else if (CreateMode == CreationMode.LOT_OF_BOMB_BALLOON)
            {
                r = random.Next(5) <= 3 ? 3 : 7; // create lot of bomb with occasion good ones!
            }
        }

        private void RequestCallback(long rtg)
        {
            RequestedTimeGap = rtg;
        }

        private void AfterTimeDuration()
        {
            CreateMode = CreationMode.NORMAL;
            UpdateLevelParameters();
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
            balloon.W = balloon.Type == BalloonObj.BalloonType.BOMB ? 50 : 75;
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
            g.W = 50;

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
                if (b.Type == BalloonObj.BalloonType.SINGLE || b.Type == BalloonObj.BalloonType.TRIPLE)
                {
                    BalloonsMissed++;
                }
            }
        }

        private void explodeAnimationEnded(FrameAnimator animator, Juicy.AnimationSequence.AnimationStatus status)
        {
            animator.GO.MarkForDelete = true;
            explosionsRequired--;
            if (explosionsRequired == 0)
            {
                gameOverObj.Visible = true;
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
        }
    }
}
