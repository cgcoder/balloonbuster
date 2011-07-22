using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Juicy
{
    public class FrameAnimator
    {

        private int fpsWidth;
        private int fpsHeight;

        private int maxFrames;
        private Texture2D sprite;

        private GameObj gameObj;

        private Dictionary<string, AnimationSequence> animations;
        private AnimationSequence currentAnimation;
        private Rectangle spriteBoundary;


        int deltaTick;

        public FrameAnimator(int fpsWidth, int fpsHeight)
        {
            this.fpsWidth = fpsWidth;
            this.fpsHeight = fpsHeight;
            maxFrames = 0;
            spriteBoundary = new Rectangle(0, 0, fpsWidth, fpsHeight);
            deltaTick = 0;
        }

        public Texture2D Sprite
        {
            set
            {
                sprite = value;
                maxFrames = (int) Math.Floor(((float) sprite.Width) / fpsWidth);
            }
            get
            {
                return sprite;
            }
        }

        public GameObj GO
        {
            get { return gameObj; }
            set { gameObj = value; }
        }

        public string CurrentAnimationName
        {
            get { return currentAnimation.Name; }
            set { currentAnimation = animations[value]; }
        }

        public void AddAnimation(AnimationSequence anse)
        {
            if (animations == null) animations = new Dictionary<string, AnimationSequence>();

            animations.Add(anse.Name, anse);
            currentAnimation = anse;
        }

        public void Update(long time, GameObj obj)
        {
            if (currentAnimation == null || !currentAnimation.isRunning()) return;

            deltaTick++;
            if (deltaTick == 8)
            {
                deltaTick = 0;
                currentAnimation.Update(time, obj);
            }

       }

        public void Draw(SpriteBatch batch, GameObj obj)
        {
            if (currentAnimation == null) return; 

            int currentFrame = currentAnimation.CurrentFrame;

            spriteBoundary.X = currentFrame*fpsWidth;

            batch.Draw(sprite, obj.Position, 
                spriteBoundary, Color.White,
                0.0f, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        
    }
}
