using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using System.Collections.ObjectModel;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Notifies movement of a card from a deck to another.
    /// </summary>
    /// <param name="sourceDeckIndex">Index of source deck in all decks.</param>
    /// <param name="sourceCardIndex">Index of the moved card in the source deck.</param>
    /// <param name="destDeckIndex">Index of destination deck in all decks.</param>
    /// <param name="destCardIndex">New position of the moved card in destination deck.</param>
    public delegate void OnCardArrange(int sourceDeckIndex, int sourceCardIndex, int destDeckIndex, int destCardIndex);

    /// <summary>
    /// Interaction logic for CardArrangeBox.xaml
    /// </summary>
    public partial class CardArrangeBox : UserControl
    {

        private static object cardArrangeLock = new object();

        public CardArrangeBox()
        {
            InitializeComponent();
            this.DataContextChanged += CardArrangeBox_DataContextChanged;
            _allCardSlots = new List<CardStack>();
            _allCardStacks = new List<CardStack>();
            _stackInfo = new Dictionary<CardStack, CardChoiceLineViewModel>();
            _originalPlace = new Dictionary<CardView, CardStack>();
            _stackToSlot = new Dictionary<CardStack, CardStack>();
            _resultSlotPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            _resultPanel = new StackPanel() { Orientation = Orientation.Horizontal };
        }

        private StackPanel _resultSlotPanel;
        private StackPanel _resultPanel;

        public event OnCardArrange OnCardMoved;

        void CardArrangeBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {            
            UpdateModel();
        }

        private List<CardStack> _allCardStacks;

        private List<CardStack> _allCardSlots;

        private Dictionary<CardStack, CardChoiceLineViewModel> _stackInfo;

        private Dictionary<CardView, CardStack> _originalPlace;

        private Dictionary<CardStack, CardStack> _stackToSlot;

        private static double _cardXSpacing = 100;
        private static double _cardYSpacing = 150;
        
        public void UpdateModel()
        {
            CardChoiceViewModel model = DataContext as CardChoiceViewModel;
            if (model == null) return;

            _cardStacks.Children.Clear();
            _cardSlots.Children.Clear();
            _canvas.Children.Clear();
            _slotCanvas.Children.Clear();

            _allCardStacks.Clear();
            _allCardSlots.Clear();
            _originalPlace.Clear();
            _stackInfo.Clear();
            _stackToSlot.Clear();
            _resultPanel.Children.Clear();
            _resultSlotPanel.Children.Clear();

            int maxCount = (from line in model.CardStacks
                            select line.Cards.Count).Max();

            var resultDecks = from line in model.CardStacks
                              where line.IsResultDeck
                              select line;

            bool isResultHorizontal = (resultDecks.Max(l => l.Capacity) == 1);

            if (isResultHorizontal)
            {
                maxCount = Math.Max(maxCount, resultDecks.Count());
            }

            _cardStacks.Width = Math.Min(maxCount * _cardXSpacing, 570);
            if (isResultHorizontal)
            {
                _cardStacks.Height = (model.CardStacks.Count - resultDecks.Count() + 1) * _cardYSpacing;
            }
            else
            {
                _cardStacks.Height = model.CardStacks.Count * _cardYSpacing;
            }
            _cardSlots.Width = _cardStacks.Width;
            _cardSlots.Height = _cardStacks.Height;

            ObservableCollection<string> deckNames = new ObservableCollection<string>();

            // First, create layout.
            foreach (var line in model.CardStacks)
            {       

                CardStack slot = new CardStack() { ParentCanvas = _slotCanvas };
                slot.MaxCardSpacing = _cardXSpacing;
                slot.CardAlignment = HorizontalAlignment.Left;
                slot.Height = 130d;
                slot.Margin = new Thickness(1, 10, 1, 10);

                if (isResultHorizontal && line.IsResultDeck)
                {
                    slot.HorizontalAlignment = HorizontalAlignment.Left;
                    slot.Width = line.Capacity * _cardXSpacing;
                    _resultSlotPanel.Children.Add(slot);
                }
                else
                {
                    slot.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _cardSlots.Children.Add(slot);
                }

                if (line.IsResultDeck)
                {
                    string key = string.Format("CardSlot.Hint.{0}", line.DeckName);
                    string hint = (Application.Current.TryFindResource(key) as string) ?? string.Empty;
                    var slots = new List<CardViewModel>();
                    for (int i = 0; i < line.Capacity; i++)
                    {

                        slots.Add(new CardSlotViewModel() { Hint = hint, Card = null });
                    }
                    slot.AddCards(slots);
                    foreach (var cardSlot in slot.Cards)
                    {
                        cardSlot.IsHitTestVisible = false;
                    }
                }
                else
                {
                    deckNames.Add(line.DeckName);
                }

                var stack = new SingleRowCardStack() { ParentCanvas = _canvas };
                stack.IsDraggingHandled = false;
                stack.MaxCardSpacing = _cardXSpacing;
                stack.MaxCardSpacingOnHighlighted = _cardXSpacing;
                stack.CardAlignment = HorizontalAlignment.Left;
                stack.Height = 130d;
                stack.Margin = new Thickness(1, 10, 1, 10);
                stack.AddCards(line.Cards);

                _stackInfo.Add(stack, line);

                foreach (var cardView in stack.Cards)
                {
                    _originalPlace.Add(cardView, stack);
                    if (!model.DisplayOnly)
                    {
                        cardView.DragDirection = DragDirection.Both;
                        cardView.OnDragBegin += cardView_OnDragBegin;
                        cardView.OnDragging += cardView_OnDragging;
                        cardView.OnDragEnd += cardView_OnDragEnd;
                    }
                }

                if (isResultHorizontal && line.IsResultDeck)
                {
                    stack.HorizontalAlignment = HorizontalAlignment.Left;
                    stack.Width = line.Capacity * _cardXSpacing;
                    _resultPanel.Children.Add(stack);
                }
                else
                {
                    stack.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _cardStacks.Children.Add(stack);
                }

                _stackToSlot.Add(stack, slot);

                _allCardStacks.Add(stack);
                _allCardSlots.Add(slot);
            }

            deckIcons.ItemsSource = deckNames;

            if (isResultHorizontal)
            {
                _cardStacks.Children.Add(_resultPanel);
                _cardSlots.Children.Add(_resultSlotPanel);
            }
            _UpdateAnswer();
            _UpdateVerifiedStatus();

            // Progress bar
            if (model.TimeOutSeconds > 0)
            {
                Duration duration = new Duration(TimeSpan.FromSeconds(model.TimeOutSeconds));
                DoubleAnimation doubleanimation = new DoubleAnimation(100d, 0d, duration);
                progressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
            }
        }

        #region Helper Functions
        private CardStack _GetParentCardStack(CardView card)
        {
            foreach (var stack in _allCardStacks)
            {
                if (stack.Cards.Contains(card))
                {
                    return stack;
                }
            }
            return null;
        }
        #endregion

        private CardView _interactingCard;

        public CardView InteractingCard
        {
            get { return _interactingCard; }
            set { _interactingCard = value; }
        }

        private void _UpdateVerifiedStatus()
        {            
            CardChoiceViewModel model = DataContext as CardChoiceViewModel;
            if (model == null || model.DisplayOnly) return;

            int i = 0;
            while (!model.CardStacks[i].IsResultDeck) i++;

            bool enabled = (model.Verifier.Verify(model.Answer) == Core.UI.VerifierResult.Success);
            foreach (var option in model.MultiChoiceCommands)
            {
                var mc = option as MultiChoiceCommand;
                mc.CanExecuteStatus = enabled;
            }

            foreach (var stack in _allCardStacks)
            {
                if (_stackInfo[stack].IsResultDeck) break;

                foreach (var card in stack.Cards)
                {
                    bool possible = false;
                    int j = 0;
                    foreach (var list in model.Answer)
                    {
                        // @todo : For now, we do not verify the order of cards.
                        int capacity = model.CardStacks[i + j].Capacity;
                        Trace.Assert(capacity >= list.Count && !list.Contains(card.Card));
                        if (model.CardStacks[i + j].Capacity == list.Count) continue;
                        list.Add(card.Card);
                        if (model.Verifier.Verify(model.Answer) != Core.UI.VerifierResult.Fail)
                        {
                            possible = true;
                            list.Remove(card.Card);
                            break;
                        }
                        list.Remove(card.Card);
                        j++;
                    }
                    if (possible)
                    {
                        card.DragDirection = DragDirection.Both;
                    }
                    else
                    {
                        card.DragDirection = DragDirection.Horizontal;
                    }
                    card.CardModel.IsEnabled = possible;
                    card.CardModel.IsFaded = !possible;                    
                }
            }
        }

        public void MoveCard(CardRearrangement move)
        {
            if (_allCardStacks.Count <= Math.Max(move.SourceDeckIndex, move.DestDeckIndex)) return;
            var sourceDeck = _allCardStacks[move.SourceDeckIndex];
            var destDeck = _allCardStacks[move.DestDeckIndex];
            
            if (sourceDeck.Cards.Count <= move.SourceCardIndex) return;
            var card = sourceDeck.Cards[move.SourceCardIndex];
            sourceDeck.Cards.RemoveAt(move.SourceCardIndex);

            if (destDeck.Cards.Count < move.DestCardIndex) return;
            destDeck.Cards.Insert(move.DestCardIndex, card);
            sourceDeck.RearrangeCards();
            if (destDeck != sourceDeck)
            {
                destDeck.RearrangeCards();
            }
        }

        private CardStack _sourceDeck;
        
        private void cardView_OnDragEnd(object sender, EventArgs e)
        {
            lock(cardArrangeLock)
            {
                CardChoiceViewModel model = DataContext as CardChoiceViewModel;
                if (model == null) return;

                _ResetHighlightSlot();
                if (_highlightedStack != null)
                {
                    int newPos = _highlightedStack.InteractingCardIndex;
                    List<CardView> backup1 = new List<CardView>(_sourceDeck.Cards);
                    var backup2 = new List<CardView>(_highlightedStack.Cards);
                    int from = _sourceDeck.Cards.IndexOf(InteractingCard);
                    int to = newPos;
                    _sourceDeck.Cards.Remove(InteractingCard);
                    _highlightedStack.Cards.Insert(newPos, InteractingCard);
                    _sourceDeck.InteractingCard = null;
                    _sourceDeck.CardStatus = CardInteraction.None;
                    _sourceDeck.RearrangeCards();
                    if (_sourceDeck != _highlightedStack)
                    {
                        _highlightedStack.InteractingCard = null;
                        _highlightedStack.CardStatus = CardInteraction.None;
                        _highlightedStack.RearrangeCards();
                    }
                    _UpdateAnswer();
                    if (model.Verifier.Verify(model.Answer) == Core.UI.VerifierResult.Fail)
                    {
                        _sourceDeck.Cards.Clear();
                        foreach (var card in backup1) _sourceDeck.Cards.Add(card);
                        _sourceDeck.RearrangeCards();
                        if (_sourceDeck != _highlightedStack)
                        {
                            Trace.Assert(backup2 != null);
                            _highlightedStack.Cards.Clear();
                            foreach (var card in backup2) _highlightedStack.Cards.Add(card);
                            _highlightedStack.InteractingCard = null;
                            _UpdateAnswer();
                            _highlightedStack.CardStatus = CardInteraction.None;
                            _highlightedStack.RearrangeCards();
                        }
                    }
                    else
                    {
                        var handle = OnCardMoved;
                        if (handle != null)
                        {
                            int s = _allCardStacks.IndexOf(_sourceDeck);
                            int d = _allCardStacks.IndexOf(_highlightedStack);
                            Trace.Assert(s >= 0 && d >= 0 && from >= 0 && to >= 0);
                            handle(s, from, d, to);
                        }
                    }
                    _sourceDeck = null;
                    _highlightedStack = null;
                }
                else if (_sourceDeck != null)
                {
                    _sourceDeck.InteractingCard = null;
                    _sourceDeck.CardStatus = CardInteraction.None;
                    _sourceDeck.RearrangeCards();
                }
            

                _UpdateVerifiedStatus();
            }
        }

        private void _UpdateAnswer()
        {
            CardChoiceViewModel model = DataContext as CardChoiceViewModel;
            Trace.Assert(model != null);
            if (model == null) return;
            model.Answer.Clear();
            foreach (var s in _allCardStacks)
            {
                if (!_stackInfo[s].IsResultDeck) continue;
                List<Card> subAnswer = new List<Card>();
                foreach (var c in s.Cards)
                {
                    subAnswer.Add(c.Card);
                }
                model.Answer.Add(subAnswer);
            }
        }

        private CardStack _highlightedStack;
        // private CardStack _highlightedSlot;
        private CardView _highlightedCardSlot;

        private void _ResetHighlightSlot()
        {
            if (_highlightedCardSlot != null)
            {
                _highlightedCardSlot.CardModel.IsFaded = false;
                _highlightedCardSlot = null;
            }            
        }

        void cardView_OnDragging(object sender, EventArgs e)
        {
            lock (cardArrangeLock)
            {
                // First, find the two stacks that the card is hovering above.
                var relevantStacks = new List<CardStack>();
                var yOverlap = new List<double>();
                CardStack resultDeckStack = null;
                double maxOverlap = 0;
                double resultOverlap = 0;

                foreach (var stack in _allCardStacks)
                {
                    Rect rect = stack.BoundingBox;
                    rect.Intersect(new Rect(InteractingCard.Position, new Size(InteractingCard.Width, InteractingCard.Height)));
                    if (rect.Size.Height > 0)
                    {
                        if (stack.Parent != _resultPanel)
                        {
                            relevantStacks.Add(stack);
                            yOverlap.Add(rect.Size.Height);
                        }
                        else if (rect.Size.Width > maxOverlap)
                        {
                            resultDeckStack = stack;
                            maxOverlap = rect.Size.Width;
                            resultOverlap = rect.Size.Height;
                        }
                    }
                }

                if (resultDeckStack != null)
                {
                    relevantStacks.Add(resultDeckStack);
                    yOverlap.Add(resultOverlap);
                }
                Trace.Assert(relevantStacks.Count <= 2 && yOverlap.Count == relevantStacks.Count);

                // Second, set the interacting card of all card stacks accordingly
                foreach (var stack in _allCardStacks)
                {
                    CardInteraction status;
                    if (relevantStacks.Contains(stack) || stack == _sourceDeck)
                    {
                        stack.InteractingCard = InteractingCard;
                        status = CardInteraction.Drag;
                    }
                    else
                    {
                        stack.InteractingCard = null;
                        status = CardInteraction.None;
                    }
                    if (status != stack.CardStatus || status == CardInteraction.Drag)
                    {
                        stack.CardStatus = status;
                        stack.RearrangeCards();
                    }
                }

                // Finally, in the stack with greatest overlapping y-distance, highlight the slot.
                _ResetHighlightSlot();

                if (relevantStacks.Count == 0)
                {
                    _highlightedStack = null;
                }
                else
                {
                    _highlightedStack = relevantStacks[yOverlap.IndexOf(yOverlap.Max())];
                    var highlightedSlot = _stackToSlot[_highlightedStack];
                    if (highlightedSlot.Cards.Count > 0)
                    {
                        int index = Math.Min(_highlightedStack.InteractingCardIndex, highlightedSlot.Cards.Count - 1);
                        _highlightedCardSlot = highlightedSlot.Cards[index];
                        _highlightedCardSlot.CardModel.IsFaded = true;
                    }
                }
            }
        }
        
        void cardView_OnDragBegin(object sender, EventArgs e)
        {
            lock (cardArrangeLock)
            {                
                InteractingCard = sender as CardView;
                InteractingCard.SetValue(Canvas.ZIndexProperty, 1000);
                _sourceDeck = _GetParentCardStack(InteractingCard);
            }
        }
    }
}
