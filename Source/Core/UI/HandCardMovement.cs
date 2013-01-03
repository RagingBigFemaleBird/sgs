using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.UI
{
    [Serializable]
    public struct HandCardMovement
    {
        public int playerId;
        public int from;
        public int to;
    }
}
