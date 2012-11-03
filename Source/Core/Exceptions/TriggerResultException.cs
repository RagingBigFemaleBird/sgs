using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Exceptions
{
    public enum TriggerResult
    {
        Retry,
        Fail,
        Success,
        End,
        Abort,
        Skip,
    }
    public class TriggerResultException : SgsException
    {
        TriggerResult status;

        public TriggerResult Status
        {
            get { return status; }
            set { status = value; }
        }

        public TriggerResultException(TriggerResult r)
        {
            status = r;
        }
    }
}
