using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Juicy;

namespace Juicy
{
    public class GameObj
    {
        public delegate void AnimationEndedDelegate(FrameAnimator animator, AnimationSequence.AnimationStatus status);
        public delegate void TouchedDelegate(TouchLocation loc, GameObj go);
        public delegate void TransformEndedDelegate(GameObj o);

        protected Vector2 position;

        protected Texture2D sprite;
        protected string spriteName;
        protected Rectangle boundary;

        protected AnimationEndedDelegate animationNotifier;
        protected TouchedDelegate touchNotifier;
        protected TransformEndedDelegate transformNotifier;

        protected bool markForDelete;
        protected List<ITransform> transforms;
        protected int maxTick;
        protected int tick;
        private bool touchable;
        protected FrameAnimator animator;
        protected bool disableTransforms = false;

        protected bool visible;
        protected float scale;

        public GameObj()
        {
            animationNotifier = null;
            markForDelete = false;
            touchable = true;
            visible = true;
            scale = 1;
        }

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public AnimationEndedDelegate AnimationNotifier
        {
            get { return animationNotifier; }
            set { animationNotifier = value; }
        }

        public TouchedDelegate TouchNotifier
        {
            get { return touchNotifier; }
            set { touchNotifier = value; }
        }

        public TransformEndedDelegate TransformNotifier
        {
            get { return transformNotifier; }
            set { transformNotifier = value; }
        }

        public bool MarkForDelete
        {
            get { return markForDelete; }
            set { markForDelete = value; }
        }

        public bool Touchable
        {
            get { return touchable; }
            set { touchable = false; }
        }

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        public string SpriteName
        {
            get { return spriteName; }
            set { spriteName = value; }
        }

        public bool DisableTransform
        {
            get { return disableTransforms; }
            set { disableTransforms = value; }
        }

        public void AddTransform(ITransform transform)
        {
            if (transforms == null)
            {
                transforms = new List<ITransform>();
                maxTick = transform.EndTime;
            }
            if (maxTick < transform.EndTime)
                maxTick = transform.EndTime;
            transforms.Add(transform);
            
        }

        public Texture2D Sprite
        {
            get { return sprite; }
            set { sprite = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; updateChildObjs();  }
        }

        public virtual void Move(int dx, int dy)
        {
            position.X += dx;
            position.Y += dy;
            updateChildObjs();
        }

        public void UpdatePosition(int x, int y)
        {
            position.X = x;
            position.Y = y;
            updateChildObjs();
        }

        protected virtual void updateChildObjs()
        {

        }

        public int W
        {
            get { return boundary.Width; }
            set { boundary.Width = value; }
        }

        public int H
        {
            get { return boundary.Height; }
            set { boundary.Height = value; }
        }

        public FrameAnimator Animator
        {
            get { return animator; }
            set { animator = value; animator.GO = this;  }
        }

        public virtual void UpdateSpriteReference(SpriteManager manager)
        {
            if (spriteName == null || spriteName.Length == 0) return;

            sprite = manager.GetSprite(spriteName);
            boundary = new Rectangle(0, 0, (int) (sprite.Width*scale), (int) (sprite.Height*scale));
            if (animator != null)
            {
                animator.Sprite = sprite;
            }
        }

        public virtual void Draw(SpriteBatch batch)
        {
            if (!visible) return;

            if (animator != null)
            {
                animator.Draw(batch, this);
            }
            else
            {
                batch.Draw(sprite, Position,
                    boundary, Color.White,
                    0.0f, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
        }

        public virtual void Update(long timer)
        {
            if (disableTransforms == false && transforms != null && transforms.Count > 0)
            {
                tick++;
                if (tick == maxTick + 1)
                {

                }

                if (tick > maxTick)
                {
                    transformationsEnded();
                    foreach (ITransform t in transforms)
                    {
                        if (t.AutoReset)
                            t.Reset();
                    }
                    tick = 0;
                }
                else
                {
                    foreach (ITransform t in transforms)
                    {
                        t.Apply(this, timer);
                    }
                }
            }

            if (animator != null)
            {
                animator.Update(timer, this);
            }
        }

        public void animationEnded(AnimationSequence seq)
        {
            if (animationNotifier != null)
            {
                animationNotifier(animator, AnimationSequence.AnimationStatus.STOP);
            }
        }

        private void transformationsEnded()
        {
            if (transformNotifier != null)
            {
                transformNotifier(this);
            }
        }

        public void clearTransforms()
        {
            transforms.Clear();
        }

        public virtual void onTouch(TouchLocation l)
        {
            if (touchNotifier != null)
            {
                touchNotifier(l, this);
            }
        }
    }
}
