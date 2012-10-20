using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Core.Cards
{
    public class Card : ICard
    {
        public bool IsUnknown
        {
            get { return (Id < 0); }
        }

        public static int UnknownCardId = -1;
        public static int UnknownHeroId = -2;
        public static int UnknownRoleId = -3;

        public bool RevealOnce { get; set; }

        public Card()
        {
            Suit = SuitType.None;
            Rank = 0;
            Type = null;
            RevealOnce = false;
        }

        public Card(SuitType t, int r, CardHandler c)
        {
            Suit = t;
            Rank = r;
            Type = c;
        }

        public void CopyFrom(Card c)
        {
            Suit = c.Suit;
            Rank = c.Rank;
            Type = c.Type;
            RevealOnce = false;
            Place = c.Place;
            Id = c.Id;
            Attributes = c.Attributes;
        }

        public Card(Card c)
        {
            Suit = c.Suit;
            Rank = c.Rank;
            Type = c.Type;
            RevealOnce = false;
            Place = c.Place;
            Id = c.Id;
            Attributes = c.Attributes;
        }

        public DeckPlace Place { get; set; }

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
                if (Place.DeckType == DeckType.Hand ||
                    Place.DeckType == DeckType.Equipment ||
                    Place.DeckType == DeckType.JudgeResult)
                {
                    return Place.Player;
                }
                else 
                {
                    return null;
                }
            }
        }

        public int Id { get; set; }

        public SuitType Suit {get; set;}

        public SuitColorType SuitColor
        {
            get
            {
                if (Suit == SuitType.Heart ||
                    Suit == SuitType.Diamond)
                {
                    return SuitColorType.Red;
                }
                else if (Suit == SuitType.Spade ||
                         Suit == SuitType.Club)
                {
                    return SuitColorType.Black;
                }
                else
                {
                    return SuitColorType.None;
                }
            }
        }

        public int Rank {get; set;}

        public Dictionary<string, int> Attributes { get; set; }

        public CardHandler Type { get; set; }
    }    
}
