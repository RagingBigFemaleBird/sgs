using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Sanguosha.Core.Cards;

namespace Sanguosha.UI.Controls
{
    public class DiscardDeck : CardStack, IDeckContainer
    {
        Timer _cleanUpCounter;
        int _currentTime;

        public DiscardDeck()
        {
            _cleanUpCounter = new Timer(1000);
            _cleanUpCounter.AutoReset = true;
            _cleanUpCounter.Elapsed += _cleanUpCounter_Elapsed;
            _cleanUpCounter.Start();
            _currentTime = 0;
        }

        private static int _ClearanceTimeAllowance = 5;

        private static int _MaxVisibleCards = 10;

        private void _MakeDisappear(CardView card)
        {
            card.DisappearAfterMove = true;
            card.CardOpacity = 0d;
            card.Rebase(0.5d);
            Cards.Remove(card);
        }

        private void MarkClearance(CardView card)
        {
            if (card.DiscardDeckClearTimeStamp > _currentTime)
            {
                card.DiscardDeckClearTimeStamp = _currentTime;
            }
        }

        private void _cleanUpCounter_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool changed = false;
            _currentTime++;
            Application.Current.Dispatcher.Invoke((System.Threading.ThreadStart)delegate()
            {
                for (int i = 0; i < Cards.Count; i++)
                {
                    CardView card = Cards[i];
                    if (_currentTime - card.DiscardDeckClearTimeStamp > _ClearanceTimeAllowance)
                    {
                        changed = true;
                        _MakeDisappear(card);
                        i--;
                    }

                    if (_currentTime > card.DiscardDeckClearTimeStamp)
                    {
                        card.CardModel.IsFaded = true;
                    }
                }

                if (changed)
                {
                    RearrangeCards(0.5d);
                }
            });
        }

        public void AddCards(DeckType deck, IList<CardView> cards)
        {
            if (cards.Count == 0) return;

            int numAdded = cards.Count;
            int numRemoved = Cards.Count - Math.Max(_MaxVisibleCards, numAdded + 1);

            DeckType from = cards[0].Card.Place.DeckType;
            Canvas canvas = cards[0].Parent as Canvas;
            Trace.Assert(canvas != null);
            
            Point rightMost;
            rightMost = TranslatePoint(
                new Point(this.ActualWidth / 2 - cards[0].ActualWidth / 2, 
                this.ActualHeight / 2 - cards[0].ActualHeight / 2), canvas);
            if (Cards.Count > 0)
            {
                CardView lastCard = Cards.Last();
                rightMost = lastCard.TranslatePoint(new Point(lastCard.ActualWidth, 0), canvas);
            }

            if (from == DeckType.Compute)
            {
                foreach (var card in cards)
                {
                    canvas.Children.Remove(card);
                }
                foreach (var card in Cards)
                {
                    MarkClearance(card);
                }
            }
            else if (from == DeckType.Dealing || from == DeckType.JudgeResult)
            {
                foreach (var card in cards)
                {
                    card.Opacity = 0d;
                    card.CardOpacity = 1d;
                    card.CardModel.IsFaded = false;
                    card.SetValue(Canvas.LeftProperty, rightMost.X);
                    card.SetValue(Canvas.TopProperty, rightMost.Y);
                    rightMost.X += card.ActualWidth;
                    Cards.Add(card);
                }
                RearrangeCards(0.3d);
            }
            else
            {
                AddCards(cards, 0.3d);
            }

            foreach (var card in cards)
            {
                card.DiscardDeckClearTimeStamp = int.MaxValue;
            }

            for (int i = 0; i < numRemoved; i++)
            {
                MarkClearance(Cards[i]);
            }
        }

        public IList<CardView> RemoveCards(DeckType deck, IList<Card> cards)
        {
            IList<CardView> result = CardView.CreateCards(cards);

            RemoveCards(result);

            return result;
        }
    }
}
