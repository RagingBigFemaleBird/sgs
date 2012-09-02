using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SgsCore
{
    class GameEngine
    {
        List<Card> cardSet;

        public List<Card> CardSet
        {
            get { return cardSet; }
            set { cardSet = value; }
        }
    }
}
