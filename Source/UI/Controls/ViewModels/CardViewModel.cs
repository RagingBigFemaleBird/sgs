using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.UI;
using System.Windows;

namespace Sanguosha.UI.Controls
{
    public class CardViewModel : SelectableItem
    {
        #region Constructors
        public CardViewModel()
        {
            isFootnoteVisible = false;
        }
        #endregion
        
        #region Fields
        private Card _card;
        private Card _uiCard;
        public virtual Card Card 
        {
            get { return _card; }
            set 
            {
                if (_card == value) return;
                _card = value;
                Update();
            }
        }

        public int Id
        {
            get
            {
                return _uiCard.Id;
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

        public void UpdateFootnote()
        {
            Footnote = LogFormatter.TranslateCardFootnote(_card.Log);
        }

        private bool isFootnoteVisible;

        public bool IsFootnoteVisible
        {
            get { return isFootnoteVisible; }
            set
            {
                if (isFootnoteVisible == value) return;
                isFootnoteVisible = value;
                OnPropertyChanged("IsFootnoteVisible");
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

        public string _TypeStringHelper(Card card)
        {
            if (card == null)
            {
                return string.Empty;
            }
            else
            {
                return card.Type.CardType;
            }
        }

        /// <summary>
        /// Return the name of the transformed card face.
        /// </summary>
        /// <remarks>
        /// An unrecognized card will have a TypeString of "Unknown". Hero cards' TypeStrings are
        /// the hero names of the cards. The other cards' card type string is defined by its
        /// card handler's class name.
        /// </remarks>
        public string ActualTypeString
        {
            get
            {
                return _TypeStringHelper(_card);
            }
        }

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
                return _TypeStringHelper(_uiCard);
            }
        }

        public CardCategory Category
        {            
            get
            {
                return _uiCard.Type.Category;
            }
        }

        public HeroViewModel HeroModel
        {
            get;
            set;            
        }

        public int AttackRange
        {
            get
            {
                if (_uiCard == null) return 0;
                
                Weapon weapon = _uiCard.Type as Weapon;
                if (weapon == null)
                {
                    Trace.TraceInformation("Trying to obtain the distance of non-weapon");
                    return 0;
                }
                return weapon.AttackRange;
            }
        }

        #endregion

        public void Update()
        {
            if (_card != null)
            {
                if (_uiCard != null && _uiCard.Id == _card.Id) return;
                if (GameEngine.CardSet.Count > _card.Id && _card.Id >= 0)
                {
                    _uiCard = GameEngine.CardSet[_card.Id];
                }
                else
                {
                    _uiCard = new Card();
                    _uiCard.Id = _card.Id;
                    if (_uiCard.Id == Card.UnknownCardId)
                    {
                        _uiCard.Type = new UnknownCardHandler();
                    }
                    else if (_uiCard.Id == Card.UnknownHeroId)
                    {
                        _uiCard.Type = new UnknownHeroCardHandler();
                    }
                    else if (_uiCard.Id == Card.UnknownRoleId)
                    {
                        _uiCard.Type = new UnknownRoleCardHandler();
                    }           
                }
                
                var heroCard = _uiCard.Type as HeroCardHandler;
                if (heroCard != null)
                {
                    HeroModel = new HeroViewModel(heroCard.Hero);
                }
                else
                {
                    if (HeroModel != null) HeroModel.Hero = null;
                    HeroModel = null;
                }

                if (_card.Log != null)
                {
                    Footnote = LogFormatter.TranslateCardFootnote(_card.Log);
                }
            }
            else
            {
                if (_uiCard == null) return;
                _uiCard = null;
            }
            OnPropertyChanged("Id");
            OnPropertyChanged("Suit");
            OnPropertyChanged("SuitColor");            
            OnPropertyChanged("ActualTypeString");
            OnPropertyChanged("RankString");
            OnPropertyChanged("TypeString");
            OnPropertyChanged("Category");
            OnPropertyChanged("AttackRange");
            OnPropertyChanged("HeroModel");
        }
    }
}
