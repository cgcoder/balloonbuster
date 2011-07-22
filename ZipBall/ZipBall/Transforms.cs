using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juicy
{
    public class BaseTransform : ITransform
    {
        #region ITransform Members

        protected int startFrame;
        protected int endFrame;
        protected int changeX;
        protected int changeY;

        protected bool autoReset;
        protected bool enabled;

        protected TransformCompleteEvent completeEventHandler;

        public virtual int StartTime
        {
            get
            {
                return startFrame;
            }
            set
            {
                startFrame = value;
            }
        }

        public virtual int EndTime
        {
            get
            {
                return endFrame;
            }
            set
            {
                endFrame = value;
            }
        }

        public virtual bool AutoReset
        {
            get
            {
                return autoReset;
            }
            set
            {
                autoReset = value;
            }
        }

        public virtual bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        public virtual void Apply(GameObj obj, long timer)
        {
            throw new NotImplementedException();
        }

        public virtual void Reset()
        {
            
        }

        public virtual TransformCompleteEvent TransformCompleteEventHandler
        {
            get
            {
                return completeEventHandler;
            }

            set
            {
                completeEventHandler = value;
            }
        }

        #endregion
    }

    class LinearTransform : BaseTransform
    {
        private float stepX;
        private float stepY;

        private float deltaX;
        private float deltaY;

        private int currentFrame;

        public LinearTransform(int sf, int ef, int cx, int cy)
        {
            startFrame = sf; endFrame = ef;
            changeX = cx; changeY = cy;
            currentFrame = 0;
            int d = ef - sf;

            if (d > 0)
            {
                stepX = ((float) cx) / d;
                stepY = ((float) cy) / d;
            }

        }

        public override void Apply(GameObj obj, long timer)
        {
            currentFrame++;
            int tx, ty;
            if (currentFrame >= startFrame && currentFrame <= endFrame)
            {
                deltaX = deltaX + stepX;
                deltaY = deltaY + stepY;

                tx = (int) Math.Floor(deltaX);
                ty = (int) Math.Floor(deltaY);

                deltaX = deltaX - tx; deltaY = deltaY - ty;

                if (Math.Abs(tx) > 0 || Math.Abs(ty) > 0)
                    obj.Move(tx, ty);

                if (currentFrame == endFrame && completeEventHandler != null)
                {
                    completeEventHandler(this);
                }
            }
        }

        public override void Reset()
        {
            deltaX = 0;
            deltaY = 0;
            currentFrame = 0;
        }
    }
}
