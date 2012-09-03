using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Sanguosha.Core.Cards;

namespace Sanguosha.Core.Engine
{
    class GameEngine
    {
        List<TerminalCard> cardSet;

        public List<TerminalCard> CardSet
        {
            get { return cardSet; }
            set { cardSet = value; }
        }
    }
}
