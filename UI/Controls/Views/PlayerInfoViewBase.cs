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
                if (parentGameView != null)
                {
                    parentGameView.GlobalCanvas.Children.Remove(handCardArea);
                }
                parentGameView = value;
                handCardArea.ParentGameView = value;
                if (parentGameView != null)
                {
                    parentGameView.GlobalCanvas.Children.Add(handCardArea);
                }
            }
        }

        protected virtual CardStack HandCardStackTemplate
        {
            get { return null; }
        }

        protected void ArrangeCardAreas(Canvas globalCanvas)
        {
            UpdateLayout();
            Point topLeft = HandCardStackTemplate.TranslatePoint(new Point(0, 0), globalCanvas);            
            handCardArea.SetValue(Canvas.LeftProperty, topLeft.X);
            handCardArea.SetValue(Canvas.TopProperty, topLeft.Y);
            handCardArea.Width = HandCardStackTemplate.ActualWidth;
            handCardArea.Height = HandCardStackTemplate.ActualHeight;
            handCardArea.CardAlignment = HandCardStackTemplate.CardAlignment;
            handCardArea.CardCapacity = HandCardStackTemplate.CardCapacity;
            handCardArea.MaxCardSpacing = HandCardStackTemplate.MaxCardSpacing;
            handCardArea.IsCardConsumer = HandCardStackTemplate.IsCardConsumer;
        }

        public void UpdateCardAreas()
        {
            if (parentGameView != null)
            {
                ArrangeCardAreas(ParentGameView.GlobalCanvas);
            }
        }

        public void AddCards(DeckType deck, IList<CardView> cards)
        {
            UpdateCardAreas();
            if (deck == DeckType.Hand)
            {
                handCardArea.AddCards(cards);
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

        public void AddToOtherPiles(string pileName, List<CardView> cards)
        {
        }
    }
}
