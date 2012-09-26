using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Exceptions
{
    public enum TriggerResult
    {
        Fail,
        Success,
        End,
        Abort,
    }
    public class TriggerResultException : SgsException
    {
        TriggerResult status;

        public TriggerResult Status
        {
            get { return status; }
            set { status = value; }
        }
    }
}
