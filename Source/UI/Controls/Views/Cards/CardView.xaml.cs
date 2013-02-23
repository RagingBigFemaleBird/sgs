using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Diagnostics;

using Sanguosha.Core.Cards;
using Sanguosha.UI.Animations;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for CardView.xaml
    /// </summary>
    public partial class CardView : UserControl
    {
        private static int CardViewPoolSize = 50;

        static CardView()
        {
            _cardViewPool = new Stack<CardView>();
            for (int i = 0; i < CardViewPoolSize; i++)
            {
                _cardViewPool.Push(new CardView());
            }
        }

        public CardView()
        {
            InitializeComponent();            
            
            this.IsEnabledChanged += CardView_IsEnabledChanged;
            this.DataContextChanged += CardView_DataContextChanged;
            this.MouseMove += CardView_MouseMove;
            this.MouseLeftButtonDown += CardView_MouseLeftButtonDown;
            this.MouseLeftButtonUp += CardView_MouseLeftButtonUp;
            this.MouseEnter += CardView_MouseEnter;
            this.MouseLeave += CardView_MouseLeave;
            _OnCardPropertyChangedHandler = new PropertyChangedEventHandler(_OnCardPropertyChanged);
            _OnCardSelectedChangedHandler = new EventHandler(_OnCardSelectedChanged);
            OffsetOnSelect = true;
            Unloaded += CardView_Unloaded;
            Storyboard disappear = Resources["sbDisappear"] as Storyboard;
            disappear.Completed += new EventHandler((o, e2) =>
            {
                RenderTransform = null;
                var panel = Parent as Panel;
                if (panel != null)
                {
                    panel.Children.Remove(this);
                }
                BeginAnimation(Canvas.LeftProperty, null);
                BeginAnimation(Canvas.TopProperty, null);
                BeginAnimation(CardView.OpacityProperty, null);
                SetValue(Canvas.LeftProperty, double.NaN);
                SetValue(Canvas.TopProperty, double.NaN);
                Trace.Assert(Parent == null);
                
                if (_doDestroy)
                {
                    Trace.Assert(!_cardViewPool.Contains(this));
                    if (_cardViewPool.Count < CardViewPoolSize)
                    {
                        _cardViewPool.Push(this);
                    }
                }
            });
        }

        void CardView_Unloaded(object sender, RoutedEventArgs e)
        {
            var oldModel = CardModel;
            if (oldModel != null)
            {
                oldModel.OnSelectedChanged -= _OnCardSelectedChangedHandler;
                oldModel.PropertyChanged -= _OnCardPropertyChangedHandler;
            }
        }

        public CardView(CardViewModel card) : this()
        {            
            this.DataContext = card;            
        }

        void CardView_MouseLeave(object sender, MouseEventArgs e)
        {
            if (CardModel != null && !CardModel.IsSelected)
            {
                (Resources["sbUnHighlight"] as Storyboard).Begin();
            }
        }

        void CardView_MouseEnter(object sender, MouseEventArgs e)
        {
            if (CardModel != null && CardModel.IsEnabled)
            {
                (Resources["sbHighlight"] as Storyboard).Begin();
            }
        }

        void CardView_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                (Resources["sbUnHighlight"] as Storyboard).Begin();                
            }
            else if (IsMouseOver)
            {
                (Resources["sbHighlight"] as Storyboard).Begin();
            }
        }

        void _Repaint()
        {
            var card = CardModel;
            if (card == null)
            {
                tbHint.Text = string.Empty;
                imgCardType.Visibility = Visibility.Collapsed;
                imgSuit.Visibility = Visibility.Collapsed;
                imgRankString.Visibility = Visibility.Collapsed;
                return;
            }
            else if (card is CardSlotViewModel)
            {
                tbHint.Text = (card as CardSlotViewModel).Hint;
                imgCardType.Visibility = Visibility.Collapsed;
                imgSuit.Visibility = Visibility.Collapsed;
                imgRankString.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbTypeString.Text = CardModel.TypeString;
                string key = string.Format("Card.{0}.Image.Normal", card.TypeString);

                if (Resources.Contains(key))
                {
                    imgCardType.Source = Resources[key] as ImageSource;
                }
                else
                {
                    imgCardType.Source = null;
                }

                if (card.Suit != SuitType.None)
                {
                    imgSuit.Source = Resources[string.Format("Card.Suit.{0}.Image.Normal", card.Suit)] as ImageSource;
                }
                else
                {
                    imgSuit.Source = null;
                }

                if (card.Card.Rank > 0 && card.Card.Rank <= 13)
                {
                    imgRankString.Source = Resources[string.Format("Card.Rank.{0}.{1}.Image.Normal", card.SuitColor, card.RankString)] as ImageSource;                    
                }
                else
                {
                    imgRankString.Source = null;
                }
                imgCardType.Visibility = Visibility.Visible;
                imgSuit.Visibility = Visibility.Visible;
                imgRankString.Visibility = Visibility.Visible;
            }
            fadeMask.BeginAnimation(Border.OpacityProperty, null);
            if (card.IsFaded)
            {
                fadeMask.Opacity = 1d;
            }
            else
            {
                fadeMask.Opacity = 0d;
            }
        }

        void CardView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            toolTip.Content = DataContext;

            var oldModel = e.OldValue as CardViewModel;
            if (oldModel != null)
            {
                oldModel.OnSelectedChanged -= _OnCardSelectedChangedHandler;
                oldModel.PropertyChanged -= _OnCardPropertyChangedHandler;
            }

            CardViewModel model = DataContext as CardViewModel;
            if (model != null)
            {
                model.OnSelectedChanged += _OnCardSelectedChangedHandler;
                model.PropertyChanged += _OnCardPropertyChangedHandler;
            }
            _Repaint();
        }

        public bool OffsetOnSelect { get; set; }

        private EventHandler _OnCardSelectedChangedHandler;
        private PropertyChangedEventHandler _OnCardPropertyChangedHandler;

        private void _OnCardPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CardModel == null) return;
            if (e.PropertyName == "IsFaded")
            {
                if (CardModel.IsFaded)
                {
                    (Resources["sbFade"] as Storyboard).Begin();
                }
                else
                {
                    (Resources["sbUnfade"] as Storyboard).Begin();
                }
            }
        }

        private void _OnCardSelectedChanged(object sender, EventArgs args)
        {
            if (OffsetOnSelect)
            {
                CardViewModel model = sender as CardViewModel;
                if (model.IsSelected) (Resources["sbSelect"] as Storyboard).Begin();
                else (Resources["sbDeselect"] as Storyboard).Begin();
            }
        }

        public Card Card
        {
            get
            {
                CardViewModel model = DataContext as CardViewModel;
                if (model == null) return null;
                return model.Card;
            }
        }

        public CardViewModel CardModel
        {
            get
            {
                return DataContext as CardViewModel;
            }
        }

        public static double WidthHeightRatio = 0.7154;        

        /// <summary>
        /// Set position without showing card movement animation.
        /// </summary>
        public void SetCurrentPosition(Point p)
        {
            Position = p;
            BeginAnimation(Canvas.LeftProperty, null);
            BeginAnimation(Canvas.TopProperty, null);
            SetValue(Canvas.LeftProperty, p.X);
            SetValue(Canvas.TopProperty, p.Y);
        }

        public void AddRebaseAnimation(Storyboard storyboard, double transitionInSeconds = 0.4d)
        {
            if (Position == null) return;
            if (Parent == null || !(Parent is Canvas)) return;

            Trace.Assert(storyboard != null);

            double destX = Position.X;
            double destY = Position.Y;

            double x = (double)GetValue(Canvas.LeftProperty);
            double y = (double)GetValue(Canvas.TopProperty);

            if (double.IsNaN(x) || double.IsNaN(y))
            {
                SetValue(Canvas.LeftProperty, destX);
                SetValue(Canvas.TopProperty, destY);
                return;
            }

            var _daMoveX = new DoubleAnimation();
            var _daMoveY = new DoubleAnimation();

            var _moveAnimation = storyboard;

            Storyboard.SetTarget(_daMoveX, this);
            Storyboard.SetTargetProperty(_daMoveX, new PropertyPath(Canvas.LeftProperty));
            Storyboard.SetTarget(_daMoveY, this);
            Storyboard.SetTargetProperty(_daMoveY, new PropertyPath(Canvas.TopProperty));
            _moveAnimation.Children.Add(_daMoveX);
            _moveAnimation.Children.Add(_daMoveY);
            _daMoveX.To = destX;
            _daMoveY.To = destY;
            _daMoveX.Duration = TimeSpan.FromSeconds(transitionInSeconds);
            _daMoveY.Duration = TimeSpan.FromSeconds(transitionInSeconds);
            _moveAnimation.Duration = TimeSpan.FromSeconds(transitionInSeconds);
        }

        /// <summary>
        /// Animate card to <c>Position</c>.
        /// </summary>
        public void Rebase(double transitionInSeconds = 0.4d)
        {
            var anim = new Storyboard();
            AddRebaseAnimation(anim, transitionInSeconds);
            if (anim != null) anim.Begin(this, HandoffBehavior.Compose);
        }

        public void Appear(double duration)
        {
            if (duration == 0) Opacity = 1.0;
            else
            {
                Storyboard appear = Resources["sbAppear"] as Storyboard;
                appear.SpeedRatio = 1 / duration;
                appear.Begin(this, HandoffBehavior.Compose);
            }
        }

        public void Disappear(double duration, bool destroy = false)
        {
            this.IsHitTestVisible = false;            
            Panel panel = this.Parent as Panel;
            if (panel == null) return;
            else if (duration == 0) { panel.Children.Remove(this); }
            else
            {
                Storyboard disappear = Resources["sbDisappear"] as Storyboard;
                _doDestroy = destroy;
                disappear.SpeedRatio = 1 / duration;
                disappear.Begin(this, HandoffBehavior.Compose);
            }
        }

        private bool _doDestroy = false;


        private static void OnOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CardView card = d as CardView;
            if (card == null) return;
            card.Rebase();
        }

        #region Drag and Drop

        public event EventHandler OnDragBegin;

        public event EventHandler OnDragging;

        public event EventHandler OnDragEnd;

        private Point _dragStartPoint;
        private enum DragState
        {
            None,
            MouseDown,
            Dragging
        };

        private Point _ComputeDragOffset(Point currentPos)
        {
            double deltaX = (DragDirection & DragDirection.Horizontal) != 0 ? currentPos.X - _dragStartPoint.X : 0;
            double deltaY = (DragDirection & DragDirection.Vertical) != 0 ? currentPos.Y - _dragStartPoint.Y : 0;
            return new Point(deltaX, deltaY);
        }

        private bool _DragDetected(Point currentPos)
        {
            Point delta = _ComputeDragOffset(currentPos);
            return (Math.Abs(delta.X) >= 15 || Math.Abs(delta.Y) >= 15);
        }

        private DragState  _dragState;
        private Point _startCardPosition;
        private void CardView_MouseMove(object sender, MouseEventArgs e)
        {
            if (DragDirection != DragDirection.None && e.LeftButton == MouseButtonState.Pressed)
            {
                Window wnd = Window.GetWindow(this);
                Point pos = e.MouseDevice.GetPosition(wnd);
                if (_dragState == DragState.MouseDown)
                {
                    if (_DragDetected(pos))
                    {
                        this.BeginAnimation(CardView.OpacityProperty, null);
                        Opacity = 0.8d;
                        _dragStartPoint = pos;
                        _dragState = DragState.Dragging;
                        _startCardPosition = Position;

                        var handle = OnDragBegin;
                        if (handle != null)
                        {
                            handle(this, new EventArgs());
                        }
                    }
                }
                else if (_dragState == DragState.Dragging)
                {
                    Point offset = _ComputeDragOffset(pos);
                    SetCurrentPosition(new Point(_startCardPosition.X + offset.X, _startCardPosition.Y + offset.Y));
                    var handle = OnDragging;
                    if (handle != null)
                    {
                        handle(this, new EventArgs());
                    }

                    e.Handled = true;
                }
            }
        }

        private void CardView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DragDirection != DragDirection.None)
            {               
                if (_dragState == DragState.None)
                {
                    Window wnd = Window.GetWindow(this);
                    Point pos = e.MouseDevice.GetPosition(wnd);
                    _dragStartPoint = pos;
                    this.CaptureMouse();
                }
                e.Handled = true;
            }
            //Trace.Assert(_dragState == DragState.None);
            _dragState = DragState.MouseDown;
        }

        private void _ReleaseMouseCapture()
        {
            this.ReleaseMouseCapture();
            
            if (DragDirection != DragDirection.None && _dragState == DragState.Dragging)
            {
                Opacity = 1.0d;
                var handle = OnDragEnd;
                if (handle != null)
                {
                    handle(this, new EventArgs());
                }
            }
            _dragState = DragState.None;
        }

        private void CardView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {           
            CardViewModel model = DataContext as CardViewModel;
            if (model != null && _dragState == DragState.MouseDown && model.IsEnabled)
            {
                model.IsSelected = !model.IsSelected;                
            }
            _ReleaseMouseCapture();
            e.Handled = true;
        }

        private static void _OnDragDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var card = d as CardView;
            if (card != null)
            {
                card._ReleaseMouseCapture();   
            }
        }
        #endregion

        #region Dependency Properties
       
        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(CardView), new UIPropertyMetadata(new Point(0,0)));
        
        public DragDirection DragDirection
        {
            get { return (DragDirection)GetValue(DragDirectionProperty); }
            set { SetValue(DragDirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragDirection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragDirectionProperty =
            DependencyProperty.Register("DragDirection", typeof(DragDirection), typeof(CardView), new UIPropertyMetadata(DragDirection.None, new PropertyChangedCallback(_OnDragDirectionChanged)));
        
        
        #endregion

        #region Card Creation/Destruction Helpers

        static Stack<CardView> _cardViewPool;

        public static CardView CreateCard(Card card, Panel parent = null, int width = 93, int height = 130)
        {
            return CreateCard(new CardViewModel() { Card = card }, parent, width, height);
        }

        public static CardView CreateCard(CardViewModel card, Panel parent = null, int width = 93, int height = 130)
        {
            if (_cardViewPool.Count == 0) _cardViewPool.Push(new CardView());

            var cardView = _cardViewPool.Pop();

            cardView.Width = width;
            cardView.Height = height;
            cardView.Opacity = 0d;
            cardView.Visibility = Visibility.Visible;
            cardView.DataContext = card;
            cardView.IsHitTestVisible = true;
            Trace.Assert(cardView.Parent == null);

            if (parent != null)
            {
                parent.Children.Add(cardView);
            }
            return cardView;
        }

        public static IList<CardView> CreateCards(IList<CardViewModel> cards, Panel parent = null)
        {
            List<CardView> cardViews = new List<CardView>();
            foreach (var card in cards)
            {
                cardViews.Add(CreateCard(card, parent));
            }
            return cardViews;
        }

        public static IList<CardView> CreateCards(IList<Card> cards, Panel parent = null)
        {
            List<CardView> cardViews = new List<CardView>();            
            foreach (Card card in cards)
            {
                cardViews.Add(CreateCard(card, parent));
            }
            return cardViews;
        }

        public static void ClearCache()
        {
            _cardViewPool.Clear();
        }
        #endregion

        #region Helper Tags
        public int DiscardDeckClearTimeStamp
        {
            get;
            set;
        }
        #endregion
        
        #region Animations
        public void PlayAnimation(AnimationBase animation, Point offset)
        {
            animationCenter.Children.Add(animation);
            animation.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            animation.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            animation.Start();
        }
        #endregion

        public void Update()
        {
            if (CardModel != null)
            {
                CardModel.Update();
            }
            _Repaint();
        }
    }
}
