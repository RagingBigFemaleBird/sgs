using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public class Card : ICard
    {
        static Card()
        {
            UnknownCard = new Card();
        }

        public Card()
        {
            suit = SuitType.None;
            rank = 0;
        }

        DeckPlace place;
        public DeckPlace Place
        {
            get { return place; }
            set { place = value; }
        }

        /// <summary>
        /// Computational owner of the card.
        /// </summary>
        /// <remarks>
        /// 每名角色的牌包括其手牌、装备区里的牌和判定牌。该角色的判定牌和其判定区里的牌都不为任何角色所拥有。
        /// </remarks>
        public Player Owner
        {
            get
            {
                if (place.DeckType == DeckType.Hand ||
                    place.DeckType == DeckType.Equipment ||
                    place.DeckType == DeckType.JudgeResult)
                {
                    return place.Player;
                }
                else 
                {
                    return null;
                }
            }
        }

        SuitType suit;
        public SuitType Suit
        {
            get { return suit; }
            set { suit = value; }
        }

        public SuitColorType SuitColor
        {
            get
            {
                if (suit == SuitType.Heart ||
                    suit == SuitType.Diamond)
                {
                    return SuitColorType.Red;
                }
                else if (suit == SuitType.Spade ||
                         suit == SuitType.Club)
                {
                    return SuitColorType.Black;
                }
                else
                {
                    return SuitColorType.None;
                }
            }
        }

        int rank;
        public int Rank
        {
            get { return rank; }
            set { rank = value; }
        }
        public static Card UnknownCard;

        string type;

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

    }    
}
