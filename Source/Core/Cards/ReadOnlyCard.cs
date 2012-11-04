using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;
using System.Diagnostics;

namespace Sanguosha.Core.Cards
{
    public class ReadOnlyCard : ICard
    {
        public ReadOnlyCard(ICard card)
        {
            type = card.Type;
            place = card.Place;
            rank = card.Rank;
            suit = card.Suit;
            owner = card.Owner;
            suitColor = card.SuitColor;
        }
        Player owner;
        public Player Owner { get { return owner; } }
        DeckPlace place;
        public DeckPlace Place { get { return place; } set { Trace.Assert(false); } }
        int rank;
        public int Rank { get { return rank; } set { Trace.Assert(false); } }
        CardHandler type;
        public CardHandler Type { get { return type; } set { Trace.Assert(false); } }
        SuitType suit;
        public SuitType Suit { get { return suit; } set { Trace.Assert(false); } }
        SuitColorType suitColor;
        public SuitColorType SuitColor { get { return suitColor; } }
    }
}
