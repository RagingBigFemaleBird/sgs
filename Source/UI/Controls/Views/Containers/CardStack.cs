using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;


namespace Sanguosha.UI.Controls
{

    public delegate void HandCardMovedHandler(int from, int to);
    public class CardStack : Canvas
    {
        #region Constructors
        public CardStack()
        {            
            CardAlignment = HorizontalAlignment.Center;
            IsCardConsumer = false;
            CardCapacity = int.MaxValue;
            _cards = new List<CardView>();
            this.SizeChanged += new SizeChangedEventHandler(CardStack_SizeChanged);
            _rearrangeLock = new object();           
        }

        void CardStack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RearrangeCards(0d);
        }

        #endregion

        #region Drag and Drop, Highlighting
        private CardView _interactingCard;

        public CardView InteractingCard
        {
            get { return _interactingCard; }
            set { _interactingCard = value; }
        }

        private CardInteraction _cardInteraction;

        public CardInteraction CardStatus
        {
            get { return _cardInteraction; }
            set { _cardInteraction = value; }
        }

        private int _interactingCardIndex;

        public int InteractingCardIndex
        {
            get
            {
                if (_interactingCard == null || CardStatus != CardInteraction.Drag)
                {
                    return -1;
                }
                return _interactingCardIndex;
            }
            private set
            {
                _interactingCardIndex = value;
            }
        }

        protected int ComputeDragCardNewIndex()
        {
            Trace.Assert(InteractingCard != null);
            lock (_rearrangeLock)
            {
                double right = InteractingCard.Position.X + InteractingCard.Width;
                int i = 0;
                bool skipOne = false;
                for (; i < Cards.Count; i++)
                {
                    var card = Cards[i];
                    if (card == InteractingCard)
                    {
                        skipOne = true;
                        continue;
                    }
                    if (right <= card.Position.X + card.Width)
                    {
                        break;
                    }
                }
                if (skipOne) i--;
                return i;
            }
        }
        #endregion

        #region Card Rearrangement

        public void RearrangeCards(double transitionInSeconds)
        {
            var tmpCards = new List<CardView>(_cards);
            if (_interactingCard != null && _cardInteraction == CardInteraction.Drag)
            {
                InteractingCardIndex = ComputeDragCardNewIndex();
                Trace.Assert(InteractingCardIndex >= 0 && InteractingCardIndex <= _cards.Count);                
                tmpCards.Remove(_interactingCard);
                tmpCards.Insert(InteractingCardIndex, _interactingCard);
            }
            RearrangeCards(tmpCards, transitionInSeconds);
        }

        private static double _extraSpaceForHighlightedCard = 30d;

        protected static object _rearrangeLock;
        /// <summary>
        /// Arrange children cards in this stack.
        /// </summary>
        /// <remarks>
        /// Assumes that all cards have the same width.
        /// </remarks>
        protected void RearrangeCards(IList<CardView> cards, double transitionInSeconds)
        {
            lock (_rearrangeLock)
            {                
                int numCards = cards.Count();
                if (_cardInteraction == CardInteraction.Drag && cards.Contains(_interactingCard))
                {
                    numCards--;
                }
                if (numCards == 0) return;
                Trace.Assert(ParentCanvas != null);
                double cardHeight = (from c in cards select c.Height).Max();
                double cardWidth = (from c in cards select c.Width).Max();
                if (KeepHorizontalOrder)
                {
                    cards = new List<CardView>(cards.OrderBy(c => c.Position.X));
                }

                int zindex = 0;
                double totalWidth = this.ActualWidth;
                
                // Do not continue if the layout has not been updated yet.
                if (totalWidth == 0) return;

                double extraSpace = Math.Min(MaxCardSpacing - (totalWidth - cardWidth) / (numCards - 1), _extraSpaceForHighlightedCard);
                int highlightIndex = (_interactingCard == null) ? -1 : cards.IndexOf(_interactingCard);
                bool doHighlight = (_cardInteraction != CardInteraction.None && highlightIndex >= 0 && highlightIndex != _cards.Count - 1);

                double maxWidth = doHighlight ? Math.Max(0, totalWidth - extraSpace) : totalWidth;
                double step = Math.Max(0, Math.Min(MaxCardSpacing, (maxWidth - cardWidth) / (numCards - 1)));

                if (step == MaxCardSpacing) doHighlight = false;

                Point topLeft = this.TranslatePoint(new Point(0,0), ParentCanvas);
                double startX = topLeft.X;
                if (CardAlignment == HorizontalAlignment.Center)
                {
                    startX += totalWidth / 2 - step * ((numCards - 1) / 2.0) - cardWidth / 2;
                    if (doHighlight) startX -= _extraSpaceForHighlightedCard / 2;
                }
                else if (CardAlignment == HorizontalAlignment.Right)
                {
                    startX += maxWidth - step * numCards;
                }

                int i = 0;
                double lastX = startX - step;
                foreach (CardView card in cards)
                {                    
                    if (card == _interactingCard && _cardInteraction == CardInteraction.Drag)
                    {
                        i++;
                        Rect lastCardRect = new Rect(lastX, topLeft.Y + ActualHeight / 2 - cardHeight / 2, step, cardHeight);
                        Rect cardRect = new Rect(card.Position, new Size(cardWidth, cardHeight));
                        lastCardRect.Intersect(cardRect);
                        double overlapX = lastCardRect.Size.Width;
                        double overlapY = lastCardRect.Size.Height;
                        if (overlapX < 0)
                        {
                            continue;
                        }
                        else if (overlapX < cardWidth / 2)
                        {
                            lastX += overlapX * overlapY / cardHeight;
                        }
                        else
                        {
                            lastX = Math.Max(startX - step, card.Position.X);
                        }
                        continue;
                    }

                    double newX = lastX + step;
                    if (doHighlight && i == highlightIndex + 1)
                    {
                        newX = Math.Min(newX + _extraSpaceForHighlightedCard, lastX + MaxCardSpacing);
                    }
                    lastX = newX;
                    if (!ParentCanvas.Children.Contains(card))
                    {
                        ParentCanvas.Children.Add(card);
                    }
                    card.Position = new Point(newX, topLeft.Y + ActualHeight / 2 - cardHeight / 2);
                    card.SetValue(Canvas.ZIndexProperty, zindex++);
                    card.Rebase(transitionInSeconds);
                    i++;
                }
            }
        }        

        #endregion

        #region Public Functions

        protected virtual void RegisterCardEvents(CardView card)
        {
        }

        protected virtual void UnRegisterCardEvents(CardView card)
        {
        }

        public void AddCards(IList<CardViewModel> cards, double transitionInSeconds = 0d)
        {
            var cardViews = new List<CardView>();
            foreach (var card in cards)
            {
                var cardView = new CardView(card);
                cardViews.Add(cardView);
                ParentCanvas.Children.Add(cardView);
            }
            AddCards(cardViews, transitionInSeconds);
        }

        public void AddCards(IList<CardView> cards, double transitionInSeconds)
        {
            lock (_cards)
            {
                foreach (var card in cards)
                {
                    card.CardModel.IsSelected = false;                    
                    if (IsCardConsumer)
                    {
                        card.Disappear(transitionInSeconds);
                    }
                    else
                    {
                        card.Appear(transitionInSeconds);
                        _cards.Add(card);
                        RegisterCardEvents(card);
                    }
                }
                if (IsCardConsumer)
                {
                    RearrangeCards(cards, transitionInSeconds);
                }
                else
                {
                    RearrangeCards(_cards, transitionInSeconds);
                }
            }
        }

        public void RemoveCards(IList<CardView> cards)
        {
            lock (_cards)
            {
                foreach (var card in cards)
                {
                    UnRegisterCardEvents(card);
                }
                var nonexisted = from c in cards
                                 where !_cards.Contains(c)
                                 select c;
                RearrangeCards(new List<CardView>(nonexisted), 0d);
                _cards = new List<CardView>(_cards.Except(cards));
                RearrangeCards(_cards, 0.5d);
            }
        }

        #endregion
        
        #region Fields

        public Canvas ParentCanvas { get; set; }

        public double MaxCardSpacing { get; set; }

        public HorizontalAlignment CardAlignment { get; set; }

        public bool IsCardConsumer { get; set; }

        public int CardCapacity { get; set; }

        public bool KeepHorizontalOrder { get; set; }

        private List<CardView> _cards;
        public IList<CardView> Cards
        {
            get { return _cards; }
        }

        public Rect BoundingBox
        {
            get
            {
                Point topLeft = this.TranslatePoint(new Point(0, 0), ParentCanvas);
                return new Rect(topLeft, new Size(ActualWidth, ActualHeight));
            }
        }
        #endregion        
    }
}
