using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.UI
{
    public class MovementHelper
    {
        private bool fakedMove;

        public bool FakedMove
        {
            get { return fakedMove; }
            set { fakedMove = value; }
        }

        private int windowId;

        public int WindowId
        {
            get { return windowId; }
            set { windowId = value; }
        }
    }
}
