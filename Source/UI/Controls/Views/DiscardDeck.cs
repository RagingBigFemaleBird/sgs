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

            foreach (var card in cards)
            {
                card.CardModel.UpdateFootnote();
                card.CardModel.IsFootnoteVisible = true;
            }

            // Do not show cards that move from compute area to discard area
            // or from judge result area to discard aresa
            if (from != DeckType.Compute && from != DeckType.JudgeResult)
            {
                // If the card is from dealing area to discard/judge result area,
                // do "append" animation rather than "move" animation
                if (from == DeckType.Dealing)
                {
                    // Compute the position that the cards should appear
                    Point rightMost;
                    rightMost = TranslatePoint(
                        new Point(this.ActualWidth / 2 - cards[0].ActualWidth / 2,
                        this.ActualHeight / 2 - cards[0].ActualHeight / 2), canvas);
                    if (Cards.Count > 0)
                    {
                        CardView lastCard = Cards.Last();
                        rightMost = lastCard.TranslatePoint(new Point(lastCard.ActualWidth * 3, 0), canvas);
                    }

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
                else // from hand/equip to discard area.
                {
                    AddCards(cards, 0.5d);
                }
            }
            else
            {
                foreach (var card in cards)
                {
                    canvas.Children.Remove(card);
                }
            }

            // Card just entered compute area should hold until they enter discard area.
            foreach (var card in cards)
            {
                card.DiscardDeckClearTimeStamp = int.MaxValue;
            }            
            
            // When a card enters discard area, every thing in the deck should fade out (but
            // not disappear).
            if (deck == DeckType.Discard)
            {
                foreach (var card in Cards)
                {
                    MarkClearance(card);
                }
            }
            else
            {
                // When there are too many cards in the deck, remove the dated ones.
                for (int i = 0; i < numRemoved; i++)
                {
                    MarkClearance(Cards[i]);
                }
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
