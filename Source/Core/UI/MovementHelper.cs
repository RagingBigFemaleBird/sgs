using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.UI
{
    public class MovementHelper
    {
        private bool isFakedMove;

        /// <summary>
        /// the movement is not really a game move but a logical move designed to facilitate user choices, e.g. 遗计
        /// </summary>
        public bool IsFakedMove
        {
            get { return isFakedMove; }
            set { isFakedMove = value; }
        }

        private int windowId;

        public int WindowId
        {
            get { return windowId; }
            set { windowId = value; }
        }

        private bool isWuGu;

        public bool IsWuGu
        {
            get { return isWuGu; }
            set { isWuGu = value; }
        }
    }
}
