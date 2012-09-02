using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SgsCore
{
    class Trigger
    {
        protected bool enabled;

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        protected int priority;

        public int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public abstract void Run(GameEvent gameEvent, Object eventArgs);
                
    }
}
