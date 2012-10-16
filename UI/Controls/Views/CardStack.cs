using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;
using System.Diagnostics;


namespace Sanguosha.UI.Controls
{
    public class CardStack : Canvas
    {
        #region Constructors
        public CardStack()
        {            
            CardAlignment = HorizontalAlignment.Center;
            IsCardConsumer = false;
            CardCapacity = int.MaxValue;
            this.SizeChanged += new SizeChangedEventHandler(CardStack_SizeChanged);
            _cards = new List<CardView>();
        }

        void CardStack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RearrangeCards(_cards);
        }
        #endregion

        #region Private Members and Functions        

        private void _TakeOverCard(CardView card)
        {
            Trace.Assert(card.Parent == null || 
                         card.Parent == ParentGameView.GlobalCanvas);
            Trace.Assert(!_cards.Contains(card));
            if (card.Parent != null)
            {
                Point topLeft = card.TranslatePoint(new Point(0, 0), this);
                ParentGameView.GlobalCanvas.Children.Remove(card);
                this.Children.Add(card);
                card.SetValue(CardStack.LeftProperty, topLeft.X);
                card.SetValue(CardStack.TopProperty, topLeft.Y);
            }
            else
            {
                this.Children.Add(card);                
            }
            _cards.Add(card);
        }

        private void _HandOverCard(CardView card)
        {
            Trace.Assert(card.Parent == this);
            Point topLeft = card.TranslatePoint(new Point(0, 0), ParentGameView.GlobalCanvas);
            this.Children.Remove(card);
            ParentGameView.GlobalCanvas.Children.Add(card);
            card.SetValue(Canvas.LeftProperty, topLeft.X);
            card.SetValue(Canvas.TopProperty, topLeft.Y);
            if (_cards.Contains(card))
            {
                _cards.Remove(card);
            }
        }

        #endregion

        #region Private Functions
        
        /// <summary>
        /// Arrange children cards in this stack.
        /// </summary>
        /// <remarks>
        /// Assumes that all cards have the same width.
        /// </remarks>
        public void RearrangeCards(IEnumerable<CardView> cards)
        {
            int numCards = cards.Count();
            if (numCards == 0) return;
            this.UpdateLayout();
            double cardHeight = (from c in cards select c.Height).Max();
            double cardWidth = (from c in cards select c.Width).Max();
            if (KeepHorizontalOrder)
            {
                cards = _cards.OrderBy(c => c.Position.X);
            }
            double maxWidth = this.ActualWidth;
            double step = Math.Min(MaxCardSpacing, (maxWidth - cardWidth) / (numCards - 1));
            int i = 0;
            foreach (CardView card in cards)
            {
                double newX = 0;
                if (CardAlignment == HorizontalAlignment.Center)
                {
                    newX = maxWidth / 2 + step * (i - (numCards - 1) / 2.0) - cardWidth / 2;
                }
                else if (CardAlignment == HorizontalAlignment.Left)
                {
                    newX = step * i;
                }
                else if (CardAlignment == HorizontalAlignment.Right)
                {
                    newX = maxWidth + step * (i - numCards);
                }
                Point newPosition = new Point(newX, ActualHeight / 2 - cardHeight / 2);
                card.Position = newPosition;
                i++;
                
            }
        }

        #endregion

        #region Public Functions

        public void AddCards(IList<CardView> cards)
        {
            foreach (var card in cards)
            {
                _TakeOverCard(card);
                card.CardOpacity = IsCardConsumer ? 0d : 1d;
            }
            RearrangeCards(cards);
        }

        public void RemoveCards(IList<CardView> cards)
        {
            var nonexisted = from c in cards where !_cards.Contains(c)
                              select c;
            foreach (var card in nonexisted)
            {
                Children.Add(card);
            }
            RearrangeCards(nonexisted);
            foreach (var card in nonexisted)
            {
                Children.Remove(card);
            }
            foreach (var card in cards)
            {
                _HandOverCard(card);
            }
            RearrangeCards(_cards);
        }

        #endregion

        #region Fields

        public GameView ParentGameView { get; set; }

        public int MaxCardSpacing { get; set; }

        public HorizontalAlignment CardAlignment { get; set; }

        public bool IsCardConsumer { get; set; }

        public int CardCapacity { get; set; }

        public bool KeepHorizontalOrder { get; set; }

        private List<CardView> _cards;
        public IList<CardView> Cards
        {
            get { return _cards; }
        }
        #endregion
    }
}
