using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juicy
{
    public delegate void TransformCompleteEvent(ITransform transform);

    public interface ITransform
    {

        int StartTime
        {
            get;
            set;
        }

        int EndTime
        {
            get;
            set;
        }

        bool AutoReset
        {
            get;
            set;
        }

        bool Enabled
        {
            get;
            set;
        }

        TransformCompleteEvent TransformCompleteEventHandler
        {
            get;
            set;
        }

        void Apply(GameObj obj, long timer);
        void Reset();
    }
}
