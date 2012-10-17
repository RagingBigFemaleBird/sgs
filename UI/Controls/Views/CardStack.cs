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
            _cards = new List<CardView>();            
        }

        #endregion


        #region Public Functions

        public void RearrangeCards(double transitionInSeconds)
        {
            RearrangeCards(_cards, transitionInSeconds);
        }

        /// <summary>
        /// Arrange children cards in this stack.
        /// </summary>
        /// <remarks>
        /// Assumes that all cards have the same width.
        /// </remarks>
        protected void RearrangeCards(IEnumerable<CardView> cards, double transitionInSeconds)
        {
            int numCards = cards.Count();
            if (numCards == 0) return;
            Trace.Assert(ParentGameView != null && ParentGameView.GlobalCanvas != null);       
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
                if (!ParentGameView.GlobalCanvas.Children.Contains(card))
                {
                    ParentGameView.GlobalCanvas.Children.Add(card);
                }
                card.Position = this.TranslatePoint(newPosition, ParentGameView.GlobalCanvas);                
                card.Rebase(transitionInSeconds);
                i++;
                
            }
        }        

        #endregion

        #region Public Functions

        public void AddCards(IList<CardView> cards, double transitionInSeconds)
        {
            foreach (var card in cards)
            {                
                card.CardOpacity = 1d;
                if (IsCardConsumer)
                {
                    card.DisappearAfterMove = true;
                }
                else
                {
                    _cards.Add(card);
                }
            }
            RearrangeCards(cards, transitionInSeconds);

        }

        public void RemoveCards(IList<CardView> cards)
        {
            var nonexisted = from c in cards where !_cards.Contains(c)
                             select c;
            RearrangeCards(nonexisted, 0);
            _cards = new List<CardView>(_cards.Except(cards));
            RearrangeCards(_cards, 0);            
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
