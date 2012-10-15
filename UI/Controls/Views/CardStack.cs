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
            IsCardProducer = false;
            IsCardConsumer = false;
            CardCapacity = int.MaxValue;
            this.SizeChanged += new SizeChangedEventHandler(CardStack_SizeChanged);
        }

        void CardStack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RearrangeCards(_cards);
        }
        #endregion

        #region Private Members and Functions
        private List<CardView> _cards;

        private void _TakeOverCard(CardView card)
        {
            Trace.Assert(card.Parent == null || 
                         card.Parent == ParentGameView.GlobalCanvas);
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
            }
        }

        private void _HandOverCard(CardView card)
        {
            Trace.Assert(card.Parent == this);
            Point topLeft = card.TranslatePoint(new Point(0, 0), ParentGameView.GlobalCanvas);
            this.Children.Remove(card);
            ParentGameView.GlobalCanvas.Children.Add(card);
            card.SetValue(Canvas.LeftProperty, topLeft.X);
            card.SetValue(Canvas.TopProperty, topLeft.Y);
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
            double cardWidth = _cards.Last().ActualWidth;
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
                Point newPosition = new Point(newX, ActualHeight / 2);
                card.Position = newPosition;
                i++;
            }
        }

        #endregion

        #region Public Functions

        public void AddCards(IList<CardView> cards)
        {
            foreach (CardView card in cards)
            {
                
            }
        }

        public void RemoveCards(IList<CardView> cards)
        {/*
            foreach (CardView card in cards)
            {
                if (_cards.Contains(card))
            }*/
        }

        #endregion

        #region Fields

        public GameView ParentGameView { get; set; }

        public int MaxCardSpacing { get; set; }

        public HorizontalAlignment CardAlignment { get; set; }

        public bool IsCardProducer { get; set; }

        public bool IsCardConsumer { get; set; }

        public int CardCapacity { get; set; }

        public bool KeepHorizontalOrder { get; set; }

        #endregion
    }
}
