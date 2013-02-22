using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;
using Sanguosha.Core.Heroes;
using System.Diagnostics;

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
        public static int UnknownSPHeroId = -4;

        public bool RevealOnce { get; set; }

        public Card()
        {
            Suit = SuitType.None;
            Rank = 0;
            Type = null;
            RevealOnce = false;
            attributes = null;
            Log = new UI.ActionLog();
        }

        public Card(SuitType t, int r, CardHandler c)
        {
            Suit = t;
            Rank = r;
            Type = c;
            attributes = null;
            Log = new UI.ActionLog();
        }

        public void CopyFrom(Card c)
        {
            Suit = c.Suit;
            Rank = c.Rank;
            Type = (CardHandler)c.Type.Clone();
            Trace.Assert(Type != null);
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
            Log = new UI.ActionLog();
        }

        public Card(ICard c)
        {
            Suit = c.Suit;
            Rank = c.Rank;
            Type = c.Type;
            RevealOnce = false;
            Place = c.Place;
            Id = -1;
            Attributes = c.Attributes;
            Log = new UI.ActionLog();
        }

        public DeckPlace HistoryPlace2 { get; set; }
        public DeckPlace HistoryPlace1 { get; set; }
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
                    Place.DeckType == DeckType.Equipment || Place.DeckType is StagingDeckType)
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

        public SuitType Suit { get; set; }

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

        public int Rank { get; set; }

        public CardHandler Type { get; set; }

        Dictionary<CardAttribute, int> attributes;

        public Dictionary<CardAttribute, int> Attributes
        {
            get { return attributes; }
            private set { attributes = value; }
        }

        public int this[CardAttribute key]
        {
            get
            {
                if (attributes == null)
                {
                    attributes = new Dictionary<CardAttribute, int>();
                }
                if (!attributes.ContainsKey(key))
                {
                    return 0;
                }
                else
                {
                    return attributes[key];
                }
            }
            set
            {
                if (attributes == null)
                {
                    attributes = new Dictionary<CardAttribute, int>();
                }
                if (!attributes.ContainsKey(key))
                {
                    attributes.Add(key, value);
                }
                else if (attributes[key] == value)
                {
                    return;
                }
                attributes[key] = value;
            }
        }

        public static CardAttribute IsLastHandCard = CardAttribute.Register("IsLastHandCard");

        #region UI Related
        UI.ActionLog log;

        public UI.ActionLog Log
        {
            get { return log; }
            set { log = value; }
        }

        public DeckPlace PlaceOverride { get; set; }
        #endregion
    }
}
