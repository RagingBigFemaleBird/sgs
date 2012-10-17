using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using Sanguosha.Core.Cards;
using System.Windows.Media;

namespace Sanguosha.UI.Controls
{
    public class PlayerInfoViewBase : UserControl, IDeckContainer
    {
        CardStack handCardArea;

        public CardStack HandCardArea
        {
            get { return handCardArea; }
            set { handCardArea = value; }
        }

        public PlayerInfoViewBase()
        {
            handCardArea = new CardStack();
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

        public void AddCards(DeckType deck, IList<CardView> cards)
        {
            if (deck == DeckType.Hand)
            {
                handCardArea.AddCards(cards, 0.5);
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
                    Trace.Assert(found);
                }

                handCardArea.RemoveCards(cardsToRemove);
            }
            else
            {
                throw new NotImplementedException();
            }
            return cardsToRemove;
        }

        public void UpdateCardAreas()
        {
            handCardArea.RearrangeCards(0.1);
        }
    }
}
