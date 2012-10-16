using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;

namespace Sanguosha.UI.Controls
{
    public class DiscardDeck : CardStack, IDeckContainer
    {
        public DiscardDeck()
        {
            MaxCardSpacing = 0;
        }

        public void AddCards(DeckType deck, IList<CardView> cards)
        {
            AddCards(cards);
        }

        public IList<CardView> RemoveCards(DeckType deck, IList<Card> cards)
        {
            IList<CardView> result = CardView.CreateCards(cards);

            RemoveCards(result);

            return result;
        }
    }
}
