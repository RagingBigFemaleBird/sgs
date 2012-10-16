using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;

namespace Sanguosha.UI.Controls
{
    public class DiscardDeck : CardStack, IDeckContainer
    {
        public void AddCards(DeckType deck, IList<CardView> cards)
        {
            AddCards(cards);
        }

        public IList<CardView> RemoveCards(DeckType deck, IList<Card> cards)
        {
            throw new NotImplementedException();
        }
    }
}
