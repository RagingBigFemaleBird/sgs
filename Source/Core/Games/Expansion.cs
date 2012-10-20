using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Games
{
    public abstract class Expansion
    {
        List<Card> cardSet;
        
        /// <summary>
        /// Set of all available hero cards and hand cards in this expansion.
        /// </summary>
        public List<Card> CardSet
        {
            get { return cardSet; }
            set { cardSet = value; }
        }
    }
}
