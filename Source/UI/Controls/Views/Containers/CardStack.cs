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
        }

        void CardStack_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RearrangeCards();
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
        #endregion

        #region Card Rearrangement

        public void RearrangeCards()
        {
            var tmpCards = new List<CardView>(_cards);
            if (_interactingCard != null && _cardInteraction == CardInteraction.Drag)
            {
                InteractingCardIndex = ComputeDragCardNewIndex();
                Trace.Assert(InteractingCardIndex >= 0 && InteractingCardIndex <= _cards.Count);
                tmpCards.Remove(_interactingCard);
                tmpCards.Insert(InteractingCardIndex, _interactingCard);
            }
            RearrangeCards(tmpCards);
        }

        private static double _extraSpaceForHighlightedCard = 30d;

        /// <summary>
        /// Arrange children cards in this stack.
        /// </summary>
        /// <remarks>
        /// Assumes that all cards have the same width.
        /// </remarks>
        public void RearrangeCards(IList<CardView> cards)
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

            double totalWidth = this.ActualWidth;

            // Do not continue if the layout has not been updated yet.
            if (totalWidth == 0) return;

            double unQualifiedStep = (totalWidth - cardWidth) / (numCards - 1);
            double step = Math.Max(0, Math.Min(MaxCardSpacing, unQualifiedStep));

            Point topLeft = this.TranslatePoint(new Point(0, 0), ParentCanvas);
            double startX = topLeft.X;
            if (CardAlignment == HorizontalAlignment.Center)
            {
                startX += totalWidth / 2 - step * ((numCards - 1) / 2.0) - cardWidth / 2;
            }
            else if (CardAlignment == HorizontalAlignment.Right)
            {
                startX += totalWidth - step * numCards;
            }

            double y = topLeft.Y + ActualHeight / 2 - cardHeight / 2;

            // First pass: get raw position of all cards;                                
            double posX = startX;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == _interactingCard && _cardInteraction == CardInteraction.Drag) continue;
                cards[i].Position = new Point(posX, y);
                posX += step;
            }
            posX -= step;

            cards = new List<CardView>(cards.OrderBy(c => c.Position.X));
            int splitter = cards.IndexOf(_interactingCard);

            // Second pass: compute final position
            double leftSpace;
            double rightSpace;
            if (splitter >= 0 && cards.Count > 1)
            {
                Rect cardRect = new Rect(_interactingCard.Position, new Size(cardWidth, cardHeight));

                if (_cardInteraction == CardInteraction.Drag)
                {
                    double center = (cardRect.Left + cardRect.Right) / 2.0;
                    double center2;
                    if (splitter >= 1)
                    {
                        center2 = cards[splitter - 1].Position.X + cardWidth;
                    }
                    else
                    {
                        center2 = cards[splitter + 1].Position.X;
                    }
                    leftSpace = rightSpace = cardWidth / 2 - Math.Abs(center - center2);
                }
                else
                {
                    leftSpace = rightSpace = Math.Max(0, Math.Min(MaxCardSpacingOnHighlighted - step, _extraSpaceForHighlightedCard));
                }

                if (leftSpace + rightSpace > 0)
                {
                    if (CardAlignment != System.Windows.HorizontalAlignment.Center)
                    {
                        leftSpace = 0;
                    }

                    // Rearrange left side of splitter
                    if (splitter > 0)
                    {
                        double leftMin = Math.Max(startX - leftSpace, topLeft.X);
                        double leftMax = cards[splitter - 1].Position.X + cardWidth - leftSpace;
                        step = splitter > 1 ? (leftMax - leftMin - cardWidth) / (splitter - 1) : 0;
                        for (int i = 0; i < splitter; i++)
                        {
                            cards[i].Position = new Point(leftMin + step * i, y);
                        }
                    }

                    // Rearrange right side of splitter
                    if (splitter < cards.Count - 1)
                    {
                        double rightMin = cards[splitter + 1].Position.X + rightSpace;
                        double rightMax = Math.Min(posX + cardWidth + rightSpace, topLeft.X + totalWidth);
                        step = (cards.Count - splitter) > 2 ? (rightMax - rightMin - cardWidth) / (cards.Count - splitter - 2) : 0;

                        for (int i = splitter + 1; i < cards.Count; i++)
                        {
                            cards[i].Position = new Point(rightMin + step * (i - splitter - 1), y);
                        }
                    }
                }
            }

            Storyboard sb = new Storyboard();
            int zindex = Panel.GetZIndex(this);
            for (int i = 0; i < cards.Count; i++)
            {
                if (i == splitter && _cardInteraction == CardInteraction.Drag) continue;
                var card = cards[i];
                if (!ParentCanvas.Children.Contains(card))
                {
                    ParentCanvas.Children.Add(card);
                }
                card.SetValue(Canvas.ZIndexProperty, zindex + i);
                card.AddRebaseAnimation(sb, 0.4d);
            }
            sb.DecelerationRatio = 0.5d;
            sb.Begin(this, HandoffBehavior.Compose);
        }

        #endregion

        #region Public Functions

        protected virtual void RegisterCardEvents(CardView card)
        {
        }

        protected virtual void UnregisterCardEvents(CardView card)
        {
        }

        public void AddCards(IList<CardViewModel> cards)
        {
            var cardViews = new List<CardView>();
            foreach (var card in cards)
            {
                var cardView = new CardView(card);
                cardViews.Add(cardView);
                ParentCanvas.Children.Add(cardView);
            }
            AddCards(cardViews);
        }

        public virtual void AppendCards(IList<CardView> cards)
        {
            if (cards.Count == 0) return;
            Canvas canvas = cards[0].Parent as Canvas;
            // Compute the position that the cards should appear
            Point rightMost;

            if (Cards.Count > 0)
            {
                CardView lastCard = Cards.Last();
                rightMost = lastCard.TranslatePoint(new Point(lastCard.ActualWidth * 2, 0), canvas);
            }
            else
            {
                rightMost = TranslatePoint(new Point(this.ActualWidth / 2, 0), canvas);
            }
            rightMost.Y = this.TranslatePoint(new Point(0, this.ActualHeight / 2 - cards[0].Height / 2), canvas).Y;
            foreach (var card in cards)
            {
                if (IsCardConsumer)
                {
                    card.Disappear(_cardOpacityChangeAnimationDurationSeconds, true);
                }
                else
                {
                    card.CardModel.IsFaded = false;
                    card.SetCurrentPosition(rightMost);
                    rightMost.X += card.ActualWidth;
                    card.Appear(0.3d);
                    Cards.Add(card);
                    RegisterCardEvents(card);
                }
            }
            RearrangeCards();
        }

        private double _cardOpacityChangeAnimationDurationSeconds = 0.5d;

        public virtual void AddCards(IList<CardView> cards)
        {
            foreach (var card in cards)
            {
                card.CardModel.IsSelected = false;
                if (IsCardConsumer)
                {
                    card.Disappear(_cardOpacityChangeAnimationDurationSeconds, true);
                }
                else
                {
                    card.Appear(_cardOpacityChangeAnimationDurationSeconds);
                    _cards.Add(card);
                    RegisterCardEvents(card);
                }
            }
            if (IsCardConsumer)
            {
                RearrangeCards(cards);
            }
            else
            {
                RearrangeCards(_cards);
            }
        }

        public void RemoveCards(IList<CardView> cards)
        {
            foreach (var card in cards)
            {
                if (card == _interactingCard)
                {
                    _interactingCard = null;
                    _cardInteraction = CardInteraction.None;
                }
                UnregisterCardEvents(card);
            }
            var nonexisted = new List<CardView>(
                                from c in cards
                                where !_cards.Contains(c)
                                select c);
            var space = MaxCardSpacing;
            MaxCardSpacing = 30;
            RearrangeCards(nonexisted);
            MaxCardSpacing = space;
            if (nonexisted.Count != cards.Count)
            {
                _cards = new List<CardView>(_cards.Except(cards));
                RearrangeCards(_cards);
            }
        }

        #endregion

        #region Fields

        public Canvas ParentCanvas { get; set; }

        public double MaxCardSpacing { get; set; }

        public double MaxCardSpacingOnHighlighted { get; set; }

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
