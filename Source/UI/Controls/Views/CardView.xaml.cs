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

namespace Sanguosha.UI.Controls
{
    public class SuitColorToColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SuitColorType color = (SuitColorType)value;
            if (color == SuitColorType.Black)
            {
                return new SolidColorBrush(Colors.Black);
            }
            else if (color == SuitColorType.Red)
            {
                Color red = new Color();
                red.R = 212;
                red.A = 255;
                return new SolidColorBrush(red);
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for CardView.xaml
    /// </summary>
    public partial class CardView : UserControl
    {
        public CardView()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += CardView_MouseLeftButtonDown;
            this.IsEnabledChanged += CardView_IsEnabledChanged;
            this.MouseEnter += CardView_MouseEnter;
            this.MouseLeave += CardView_MouseLeave;
            _daMoveX = new DoubleAnimation();
            _daMoveY = new DoubleAnimation();
            _daOpacity = new DoubleAnimation();      
            Storyboard.SetTarget(_daMoveX, this);
            Storyboard.SetTargetProperty(_daMoveX, new PropertyPath(Canvas.LeftProperty));
            Storyboard.SetTarget(_daMoveY, this);
            Storyboard.SetTargetProperty(_daMoveY, new PropertyPath(Canvas.TopProperty));
            Storyboard.SetTarget(_daOpacity, this);
            Storyboard.SetTargetProperty(_daOpacity, new PropertyPath(CardView.OpacityProperty));
            _DisappearAfterMoveHandler = new EventHandler(_DisappearAfterMove);
            _moveAnimation = new Storyboard();
            _moveAnimation.Children.Add(_daMoveX);
            _moveAnimation.Children.Add(_daMoveY);
            _moveAnimation.Children.Add(_daOpacity);
            _moveAnimation.AccelerationRatio = 0.4;
            _moveAnimation.DecelerationRatio = 0.4;
        }

        void CardView_MouseLeave(object sender, MouseEventArgs e)
        {
            (Resources["sbUnHighlight"] as Storyboard).Begin();
        }

        void CardView_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsEnabled)
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

        void CardView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }        

        void CardView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CardViewModel model = DataContext as CardViewModel;
            if (model == null) return;
            model.IsSelected = !model.IsSelected;
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

        DoubleAnimation _daMoveX;
        DoubleAnimation _daMoveY;
        DoubleAnimation _daOpacity;
        Storyboard _moveAnimation;

        public void Rebase(double transitionInSeconds)
        {
            if (Position == null) return;
            if (Parent == null || !(Parent is Canvas)) return;

            double destX = Position.X + Offset.X;
            double destY = Position.Y + Offset.Y;
            
            lock (_daOpacity)
            {
                double x = (double)GetValue(Canvas.LeftProperty);
                double y = (double)GetValue(Canvas.TopProperty);
                double opacity = (double)GetValue(Canvas.OpacityProperty);

                if (double.IsNaN(x) || double.IsNaN(y))
                {
                    SetValue(Canvas.LeftProperty, destX);
                    SetValue(Canvas.TopProperty, destY);
                    return;
                }


                Point point = new Point(x, y);
                _daMoveX.From = x;
                _daMoveY.From = y;
                _daOpacity.From = opacity;
                _daMoveX.To = destX;
                _daMoveY.To = destY;
                _daMoveX.Duration = TimeSpan.FromSeconds(transitionInSeconds);
                _daMoveY.Duration = TimeSpan.FromSeconds(transitionInSeconds);
                _daOpacity.To = CardOpacity;
                _daOpacity.Duration = TimeSpan.FromSeconds(transitionInSeconds);

                if (DisappearAfterMove)
                {
                    _daOpacity.Completed += _DisappearAfterMoveHandler;
                }

                _moveAnimation.Duration = TimeSpan.FromSeconds(transitionInSeconds);
                _moveAnimation.Begin(this, true);
            }
        }

        private EventHandler _DisappearAfterMoveHandler;

        private void _DisappearAfterMove(object sender, EventArgs e)
        {
            _daOpacity.Completed -= _DisappearAfterMoveHandler;
            _daOpacity.From = (double)GetValue(CardView.OpacityProperty);
            _daOpacity.To = 0.0d;
            _daOpacity.Duration = TimeSpan.FromSeconds(0.2d);            
            _daOpacity.Completed += new EventHandler((o, e2) =>
            {
                Canvas canvas = this.Parent as Canvas;
                Trace.Assert(canvas != null);
                canvas.Children.Remove(this);
            });
            BeginAnimation(Canvas.OpacityProperty, _daOpacity);
            // CardOpacity = 0;
        }

        private static void OnOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CardView card = d as CardView;
            if (card == null) return;
            card.Rebase(0.3d);
        }

        #region Dependency Properties

        public Point Offset
        {
            get { return (Point)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Offset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(Point), typeof(CardView), new UIPropertyMetadata(new Point(0, 0),
                new PropertyChangedCallback(OnOffsetPropertyChanged)));
        
        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(Point), typeof(CardView), new UIPropertyMetadata(new Point(0,0)));

        public double CardOpacity
        {
            get { return (double)GetValue(CardOpacityProperty); }
            set { SetValue(CardOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardOpacityProperty =
            DependencyProperty.Register("CardOpacity", typeof(double), typeof(CardView), new UIPropertyMetadata(0d));

        public bool DisappearAfterMove
        {
            get;
            set;
        }

        #endregion

        #region Card Creation/Destruction Helpers
        public static CardView CreateCard(Card card, int width = 93, int height = 130)
        {
            return new CardView()
            {
                DataContext = new CardViewModel() { Card = card },
                Width = width,
                Height = height
            };
        }

        public static IList<CardView> CreateCards(IList<Card> cards)
        {
            List<CardView> cardViews = new List<CardView>();
            foreach (Card card in cards)
            {
                cardViews.Add(CreateCard(card));
            }
            return cardViews;
        }
        #endregion

        #region Helper Tags
        public int DiscardDeckClearTimeStamp
        {
            get;
            set;
        }
        #endregion
    }
}
