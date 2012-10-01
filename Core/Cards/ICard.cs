using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public enum SuitType
    {
        None,
        Club,
        Spade,
        Heart,
        Diamond,
    }

    public enum SuitColorType
    {
        None,
        Black,
        Red,
    }

    public interface ICard
    {
        Player Owner { get; }
        DeckPlace Place { get; set; }
        int Rank { get; set; }
        CardHandler Type { get; set; }
        SuitType Suit { get; set; }
        SuitColorType SuitColor { get; }
    }
}
