using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using Sanguosha.Core.Cards;
using System.Windows.Media;
using System.Windows.Input;

namespace Sanguosha.UI.Controls
{
    public class PlayerInfoViewBase : UserControl, IDeckContainer
    {
        #region Constructors

        public PlayerInfoViewBase()
        {
            handCardArea = new CardStack();
        }

        #endregion

        CardStack handCardArea;

        #region Fields
        public PlayerInfoViewModel PlayerModel
        {
            get
            {
                return DataContext as PlayerInfoViewModel;
            }
        }

        public CardStack HandCardArea
        {
            get { return handCardArea; }
            set { handCardArea = value; }
        }

        private GameView parentGameView;
        
        public GameView ParentGameView 
        {
            get
            {
                return parentGameView;
            }
            set
            {
                parentGameView = value;
                handCardArea.ParentGameView = value;
            }
        }
        #endregion

        #region Abstract Functions

        // The following functions are in its essence abstract function. They are not declared
        // abstract only to make designer happy to render their subclasses. (VS and blend will
        // not be able to create designer view for abstract class bases.

        protected virtual void AddEquipment(CardView card)
        {
        }

        protected virtual CardView RemoveEquipment(Card card)
        {
            return null;
        }

        protected virtual void AddDelayedTool(CardView card)
        {
        }

        protected virtual CardView RemoveDelayedTool(Card card)
        {
            return null;
        }

        #endregion

        public void AddCards(DeckType deck, IList<CardView> cards)
        {
            foreach (CardView card in cards)
            {
                card.CardViewModel.IsEnabled = false;                
            }
            if (deck == DeckType.Hand)
            {
                handCardArea.AddCards(cards, 0.5d);
                PlayerModel.HandCardCount += cards.Count;
            }
            else if (deck == DeckType.Equipment)
            {
                foreach (var card in cards)
                {
                    Equipment equip = card.Card.Type as Equipment;
                    
                    if (equip != null)
                    {
                        EquipCommand command = new EquipCommand(){ Card = card.Card };
                        switch(equip.Category)
                        {
                            case CardCategory.Weapon:
                                PlayerModel.WeaponCommand = command;
                                break;
                            case CardCategory.Armor:
                                PlayerModel.ArmorCommand = command;
                                break;
                            case CardCategory.DefensiveHorse:
                                PlayerModel.DefensiveHorseCommand = command;
                                break;
                            case CardCategory.OffensiveHorse:
                                PlayerModel.OffensiveHorseCommand = command;
                                break;
                        }
                    }
                    AddEquipment(card);
                }
            }
            else if (deck == DeckType.DelayedTools)
            {
                foreach (var card in cards)
                {
                    AddDelayedTool(card);
                }
            }
        }

        public IList<CardView> RemoveCards(DeckType deck, IList<Card> cards)
        {
            List<CardView> cardsToRemove = new List<CardView>();
            if (deck == DeckType.Hand)
            {
                foreach (var card in cards)
                {
                    bool found = false;
                    foreach (var cardView in handCardArea.Cards)
                    {
                        CardViewModel viewModel = cardView.DataContext as CardViewModel;
                        Trace.Assert(viewModel != null);
                        if (viewModel.Card == card)
                        {
                            cardsToRemove.Add(cardView);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        cardsToRemove.Add(CardView.CreateCard(card));
                    }
                }
                Trace.Assert(cardsToRemove.Count == cards.Count);
                handCardArea.RemoveCards(cardsToRemove);                
                PlayerModel.HandCardCount -= cardsToRemove.Count;
            }
            else if (deck == DeckType.Equipment)
            {
                foreach (var card in cards)
                {
                    Equipment equip = card.Type as Equipment;

                    if (equip != null)
                    {
                        switch (equip.Category)
                        {
                            case CardCategory.Weapon:
                                PlayerModel.WeaponCommand = null;
                                break;
                            case CardCategory.Armor:
                                PlayerModel.ArmorCommand = null;
                                break;
                            case CardCategory.DefensiveHorse:
                                PlayerModel.DefensiveHorseCommand = null;
                                break;
                            case CardCategory.OffensiveHorse:
                                PlayerModel.OffensiveHorseCommand = null;
                                break;
                        }
                    }
                    CardView cardView = RemoveEquipment(card);
                    cardsToRemove.Add(cardView);
                }
            }
            else if (deck == DeckType.DelayedTools)
            {
                foreach (var card in cards)
                {
                    CardView cardView = RemoveDelayedTool(card);
                    cardsToRemove.Add(cardView);
                }
            }
            return cardsToRemove;
        }

        public void UpdateCardAreas()
        {
            handCardArea.RearrangeCards(0d);
        }
    }
}
