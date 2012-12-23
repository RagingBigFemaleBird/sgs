using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Cards;

namespace Sanguosha.UI.Controls
{
    public interface IDeckContainer
    {
        /// <summary>
        /// Add cards to a deck.
        /// </summary>
        /// <param name="deck">Destination deck.</param>
        /// <param name="cards">Cards to be added.</param>
        /// <param name="isFaked">If true, append rather than move cards to the end of the deck.</param>
        void AddCards(DeckType deck, IList<CardView> cards, bool isFaked = false);
        
        /// <summary>
        /// Remove cards from a deck.
        /// </summary>
        /// <param name="deck">Source deck.</param>
        /// <param name="cards">Cards to be removed.</param>
        /// <param name="isCopy">If true, the returned cards will be a copy of cards appearning at the same position as they are in the deck.</param>
        /// <returns>CardView representing <paramref name="cards"/>.</returns>
        IList<CardView> RemoveCards(DeckType deck, IList<Card> cards, bool isCopy = false);
    }
}
