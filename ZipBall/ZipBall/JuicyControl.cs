using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juicy
{
    class JuicyControl
    {
        private String name;
        private JuicyControl parentControl;
        private List<JuicyControl> childControls;

        public JuicyControl()
        {
            childControls = new List<JuicyControl>();
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual void onControlAdded()
        {
        }

        public JuicyControl ParentControl
        {
            get { return parentControl; }
            set { parentControl = value; }
        }

        public virtual void update(long tick)
        {
        }

        protected virtual void selfUpdate(long tick)
        {
        }

        protected virtual void updateChild(long tick)
        {
        }


    }
}
