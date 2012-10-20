using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards
{
    public class CompositeCard : ICard
    {
        public CompositeCard()
        {
            Subcards = new List<Card>();
            attributes = null;
        }

        public CompositeCard(List<Card> cards)
        {
            Subcards = cards;
            attributes = null;
        }

        public List<Card> Subcards {get; set;}

        public Player Owner
        {
            get
            {
                var owners =
                    from card in Subcards
                    select card.Owner;
                owners = owners.Distinct();
                if (owners.Count() == 1)
                {
                    return owners.First();
                }
                else
                {
                    return null;
                }
            }
        }

        public DeckPlace Place
        {
            get
            {
                var places =
                    from card in Subcards
                    select card.Place;
                places = places.Distinct();
                if (places.Count() == 1)
                {
                    return places.First();
                }
                else
                {
                    return new DeckPlace(null, DeckType.None);
                }
            }
            set
            {
                foreach (Card card in Subcards)
                {
                    card.Place = value;
                }
            }
        }

        /// <summary>
        /// Rank of a composite card is always 0.
        /// </summary>
        public int Rank
        {
            get
            {
                return 0;
            }
            set
            {
                throw new NotSupportedException("Cannot set rank of a composite card.");
            }
        }

        /// <summary>
        /// Suit of a composite card is always None.
        /// </summary>
        public SuitType Suit
        {
            get
            {
                return SuitType.None;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Suit color of a composite card is the uniform color of its Subcards, or None if
        /// no uniform color exists.
        /// </summary>
        /// <remarks>
        /// 将多张牌当一张牌使用或打出时，除非这些牌的颜色均相同（视为相应颜色但无花色），否则视为无色且无花色。
        /// </remarks>
        public SuitColorType SuitColor
        {
            get
            {
                var colors =
                    from card in Subcards
                    select card.SuitColor;
                colors = colors.Distinct();
                if (colors.Count() == 1)
                {
                    return colors.First();
                }
                else
                {
                    return SuitColorType.None;
                }
            }
        }

        public CardHandler Type {get; set;}

        Dictionary<string, int> attributes;

        public Dictionary<string, int> Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        public int this[string key]
        {
            get
            {
                if (attributes == null)
                {
                    attributes = new Dictionary<string, int>();
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
                    attributes = new Dictionary<string, int>();
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

    }
}
