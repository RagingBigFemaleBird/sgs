using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Cards;

namespace Sanguosha.UI.Controls
{
    public interface IDeckContainer
    {
        void AddCards(DeckType deck, IList<CardView> cards);
        IList<CardView> RemoveCards(DeckType deck, IList<Card> cards);
    }
}
