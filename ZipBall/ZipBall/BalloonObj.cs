using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Juicy;

namespace ZipBall
{
    class BalloonObj : GameObj
    {
        public delegate void BalloonOutOfScreenEventNotifier(BalloonObj obj);

        private List<GameObj> subObjects;

        public enum BalloonType
        {
            SINGLE, TRIPLE, BOMB, SILVER_STAR, GOLD_STAR, GIFT
        };

        public static readonly string[] TYPE_STR = { "", "Triple", "Bomb", "Silver_Star", "Gold_Star", "Gift" };

        public enum BalloonColor
        {
            RED, GREEN, BLUE, YELLOW
        };

        public static readonly string[] COLOR_STR = { "Red", "Green", "Blue", "Yellow" };

        public enum TreasureType
        {
            NONE, TRAP, FAN, FASTER, SLOW
        };

        private BalloonType type;
        private TreasureType treasure;
        private BalloonColor color;

        private float speedX;
        private float speedY;
        private float tempX;
        private float tempY;

        private BalloonOutOfScreenEventNotifier balloonOutNotifier;
        private int touchesRequired = 1;

        public BalloonObj() : base()
        {
        }

        public BalloonType Type
        {
            get { return type; }
            set { type = value; }
        }

        public BalloonColor Color
        {
            get { return color; }
            set { color = value; }
        }

        public TreasureType Treasure
        {
            get { return treasure; }
            set { treasure = value; }
        }

        public int TouchesRequiredToBust
        {
            get { return touchesRequired; }
            set { touchesRequired = value; }
        }

        public void AddSubObjects(GameObj obj)
        {
            if (subObjects == null)
            {
                subObjects = new List<GameObj>();
            }
            subObjects.Add(obj);
        }

        public List<GameObj> getSubObjects()
        {
            return subObjects;
        }

        public void ClearAllSubObjs()
        {
            subObjects.Clear();
            subObjects = null;
        }

        public BalloonOutOfScreenEventNotifier BalloonOutNotifier
        {
            get { return balloonOutNotifier; }
            set { balloonOutNotifier = value; }
        }
        public void updateSpeed(float x, float y)
        {
            speedX = x; speedY = y;
        }

        public override void Update(long timer)
        {
            int tx, ty;

            tempX += speedX;
            tempY += speedY;

            tx = (int) Math.Floor(tempX);
            ty = (int) Math.Floor(tempY);

            tempX -= tx;
            tempY -= ty;

            base.UpdatePosition((int)position.X + tx, (int)position.Y + ty);

            base.Update(timer);

            if (position.Y < -50 && balloonOutNotifier != null)
            {
                balloonOutNotifier(this);
            }
        }
    }
}
