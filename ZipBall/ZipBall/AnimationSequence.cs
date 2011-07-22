using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juicy
{
    public class AnimationSequence
    {

        public enum AnimationMode { STOP_AT_END, LOOP, STOP_AT_START };
        public enum AnimationStatus { STOP, RUNNING };

        private string name;
        private int frameFrom;
        private int frameTo;
        private int currentFrame;

        private AnimationMode mode;
        private AnimationStatus status;

        public AnimationSequence(string name, int frameFrom, int frameTo)
        {
            this.name = name;
            this.frameFrom = frameFrom;
            this.frameTo = frameTo;
            currentFrame = frameFrom;
            status = AnimationStatus.RUNNING;
            mode = AnimationMode.STOP_AT_END;
        }

        public string Name
        {
            get { return name; }
        }

        public int FrameFrom
        {
            get { return frameFrom; }
        }

        public int FrameTo
        {
            get { return frameTo; }
        }

        public AnimationMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public void Update(long time, GameObj obj)
        {
            currentFrame++;
            if (currentFrame > frameTo)
            {
                switch (mode)
                {
                    case AnimationMode.LOOP:
                        currentFrame = frameFrom;
                        break;
                    case AnimationMode.STOP_AT_START:
                        status = AnimationStatus.STOP;
                        obj.animationEnded(this);
                        currentFrame = frameTo;
                        break;
                    case AnimationMode.STOP_AT_END:
                        status = AnimationStatus.STOP;
                        obj.animationEnded(this);
                        currentFrame--;
                        break;
                }
            }
        }

        public int CurrentFrame
        {
            get { return currentFrame; }
        }

        public bool isRunning()
        {
            return status == AnimationStatus.RUNNING;
        }
    }
}
