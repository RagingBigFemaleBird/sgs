using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Sanguosha.Core.Cards;
using System.Windows.Threading;

namespace Sanguosha.UI.Controls
{
    public class DiscardDeck : CardStack, IDeckContainer
    {
        DispatcherTimer _cleanUpCounter;
        int _currentTime;

        private EventHandler _cleanUpHandler;

        public DiscardDeck()
        {
            _cleanUpCounter = new DispatcherTimer();
            _cleanUpCounter.Interval = TimeSpan.FromSeconds(1.0);
            _cleanUpHandler = _cleanUpCounter_Elapsed;
            _cleanUpCounter.Tick += _cleanUpHandler;
            _cleanUpCounter.Start();
            _currentTime = 0;
            this.Unloaded += DiscardDeck_Unloaded;
        }

        void DiscardDeck_Unloaded(object sender, RoutedEventArgs e)
        {
            _cleanUpCounter.Tick -= _cleanUpHandler;
        }

        private static int _ClearanceTimeAllowance = 5;

        private static int _MaxVisibleCards = 10;

        private void _MakeDisappear(CardView card)
        {            
            card.SetValue(Canvas.ZIndexProperty, (int)card.GetValue(Canvas.ZIndexProperty) - 100);
            Cards.Remove(card);
            card.Disappear(0.5d, true);
        }

        public void Clear()
        {
            var backup = new List<CardView>(Cards);
            Cards.Clear();
            foreach (var card in backup)
            {
                card.Disappear(0.5d, true);
            }            
        }

        private void MarkClearance(CardView card)
        {
            if (card.DiscardDeckClearTimeStamp > _currentTime)
            {
                card.DiscardDeckClearTimeStamp = _currentTime;
            }
        }

        private void _cleanUpCounter_Elapsed(object sender, EventArgs e)
        {
            bool changed = false;
            _currentTime++;
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
                RearrangeCards();
            }         
        }

        /// <summary>
        /// Fade out all the cards currently in the deck and make them disappear after certain interval.
        /// </summary>
        public void UnlockCards()
        {
            foreach (var card in Cards)
            {
                MarkClearance(card);
            }
        }

        private void FilterExistingCards(IList<CardView> cards)
        {            
            int total = Cards.Count;
            for (int i = 0; i < total; i++)
            {
                var card = Cards[i];
                if (!cards.Any(c2 => card.CardModel.Card == c2.CardModel.Card))
                {
                    continue;
                }
                card.Disappear(0d, true);
                Cards.RemoveAt(i);
                i--;
                total--;
            }
        }

        public override void AppendCards(IList<CardView> cards)
        {
            FilterExistingCards(cards);
            foreach (var card in cards)
            {
                card.CardModel.IsFootnoteVisible = true;
            }
            base.AppendCards(cards);
            foreach (var card in cards)
            {
                card.DiscardDeckClearTimeStamp = int.MaxValue;
            }
        }

        public override void AddCards(IList<CardView> cards)
        {
            FilterExistingCards(cards);
            foreach (var card in cards)
            {
                card.CardModel.IsFootnoteVisible = true;
            }
            base.AddCards(cards);
            foreach (var card in cards)
            {
                card.DiscardDeckClearTimeStamp = int.MaxValue;
            }
        }

        public void AddCards(DeckType deck, IList<CardView> cards, bool isFaked)
        {
            AddCards(deck, cards, isFaked, true);
        }

        public void AddCards(DeckType deck, IList<CardView> cards, bool isFaked, bool updateFootnote)
        {
            if (isFaked)
            {
                foreach (var card in cards)
                {
                    card.Disappear(0d);
                }
                return;
            }

            if (cards.Count == 0) return;

            int numAdded = cards.Count;
            int numRemoved = Cards.Count - Math.Max(_MaxVisibleCards, numAdded + 1);

            DeckType from = cards[0].Card.Place.DeckType;
            Canvas canvas = cards[0].Parent as Canvas;
            Trace.Assert(canvas != null);


            if (updateFootnote)
            {
                foreach (var card in cards)
                {
                    card.CardModel.UpdateFootnote();
                }
            }

            // Do not show cards that move from compute area to discard area
            // or from judge result area to discard aresa
            if (from != DeckType.Compute && (from != DeckType.JudgeResult || from == DeckType.JudgeResult && deck != DeckType.Discard) && from != DeckType.Dealing)
            {
                AddCards(cards);
                // Card just entered compute area should hold until they enter discard area.
            }
            else if (from == DeckType.Dealing && deck == DeckType.JudgeResult)
            {
                foreach (var card in Cards) MarkClearance(card);
                AppendCards(cards);
            }
            else
            {                
                foreach (var card in cards)
                {                    
                    canvas.Children.Remove(card);             
                }
            }            
            
            // When a card enters discard area, every thing in the deck should fade out (but
            // not disappear).
            // When there are too many cards in the deck, remove the dated ones.
            for (int i = 0; i < numRemoved; i++)
            {
                MarkClearance(Cards[i]);
            }                      
        }

        public IList<CardView> RemoveCards(DeckType deck, IList<Card> cards, bool isCopy)
        {
            IList<CardView> result = new List<CardView>();
            IList<CardView> remaining = new List<CardView>();

            foreach (var card in cards)
            {
                var oldCardView = Cards.FirstOrDefault(
                    c => ((card.Id > 0 && c.Card.Id == card.Id) || (card.Id <= 0 && c.Card == card)));
                var cardView = CardView.CreateCard(card);
                if (oldCardView != null)
                {                    
                    ParentCanvas.Children.Add(cardView);
                    cardView.SetCurrentPosition(oldCardView.Position);                   
                }
                else
                {
                    remaining.Add(cardView);
                }
                result.Add(cardView);
            }            
            RemoveCards(remaining);            
            return result;
        }
    }
}
