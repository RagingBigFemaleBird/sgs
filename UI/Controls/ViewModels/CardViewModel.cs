using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;

namespace Sanguosha.UI.Controls
{
    public class CardViewModel : SelectableItem
    {
        #region Fields
        private Card _card;
        private Card _uiCard;
        public Card Card 
        {
            get { return _card; }
            set 
            {
                if (_card == value) return;
                _card = value;
                if (GameEngine.CardSet.Count > _card.Id && _card.Id >= 0)
                {
                    _uiCard = GameEngine.CardSet[_card.Id];
                }
                else
                {
                    _uiCard = null;
                }
                OnPropertyChanged("Suit");
            }
        }

        private string footnote;

        public string Footnote
        {
            get { return footnote; }
            set 
            {
                if (footnote == value) return;
                footnote = value;
                OnPropertyChanged("Footnote");
            }
        }
        
        #endregion

        #region Mediated Card Properties
        public SuitType Suit
        {
            get
            {
                if (_uiCard == null)
                {
                    return SuitType.None;
                }
                else
                {
                    return _uiCard.Suit;
                }
            }
        }

        public SuitColorType SuitColor
        {
            get
            {
                if (_uiCard == null)
                {
                    return SuitColorType.None;
                }
                else
                {
                    return _uiCard.SuitColor;
                }
            }
        }

        /// <summary>
        /// Returns rank in the ordinary string format("A" for 1, "J" for 11).
        /// </summary>
        public string RankString
        {
            get
            {
                if (_uiCard == null || _uiCard.Rank <= 0 || _uiCard.Rank > 13)
                {
                    return string.Empty;
                }
                else if (_uiCard.Rank == 1)
                {
                    return "A";
                }
                else if (_uiCard.Rank <= 10)
                {
                    return _uiCard.Rank.ToString();
                }
                else if (_uiCard.Rank == 11)
                {
                    return "J";
                }
                else if (_uiCard.Rank == 12)
                {
                    return "Q";
                }
                else
                {
                    Trace.Assert(_uiCard.Rank == 13);
                    return "K";
                }
            }
        }

        public static string UnknownCardTypeString = "Unknown";

        public static string UnknownHeroCardTypeString = "UnknownHero";

        /// <summary>
        /// Return the name of card face.
        /// </summary>
        /// <remarks>
        /// An unrecognized card will have a TypeString of "Unknown". Hero cards' TypeStrings are
        /// the hero names of the cards. The other cards' card type string is defined by its
        /// card handler's class name.
        /// </remarks>
        public string TypeString
        {
            get
            {
                if (_uiCard == null || _uiCard.Id == Card.UnknownCardId)
                {
                    return UnknownCardTypeString;
                }
                else if (_uiCard.Id == Card.UnknownHeroId)
                {
                    return UnknownHeroCardTypeString;
                }
                else if (_uiCard.Type is HeroCardHandler)
                {
                    var cardType = _uiCard.Type as HeroCardHandler;
                    return cardType.Hero.Name;
                }
                else if (_uiCard.Type is Equipment)
                {
                    return _uiCard.Type.CardType;
                }
                else
                {
                    return _uiCard.Type.GetType().Name;
                }
            }
        }

        #endregion
    }
}
